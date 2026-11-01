using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SignalR_Chat.Repository;
using SignalR_Chat.EF;

namespace SignalR_Chat;

/*
Ключовою сутністю в SignalR, через яку клієнти обмінюються повідомленнями 
із сервером та між собою, є хаб (hub). 
Хаб представляє певний клас, який успадкований від абстрактного класу 
Microsoft.AspNetCore.SignalR.Hub.
*/
public class ChatHub(IRepository<Message> repoMessage, IRepository<User> repoUser) : Hub
{
    // private static readonly ConcurrentDictionary<string, string> Users = new();

    // Відправлення повідомлень
    public async Task Send(string username, string message)
    {
        var user = await repoUser.GetByLoginAsync(username);
        if(user != null)
        {
            Message newMessage = new Message()
            {
                MesaageText = message,
                CreatedAt = DateTime.Now,
                UserId = user.id
            };
            await repoMessage.AddAsync(newMessage);
            await repoMessage.SaveAll();

            // Виклик методу AddMessage на всіх клієнтах
            await Clients.All.SendAsync("AddMessage", username, message, DateTime.Now);
        }
    }

    // Підключення нового користувача
    public async Task Connect(string userName)
    {
        try
        {
            var id = Context.ConnectionId;
            var existingUser = await repoUser.GetByLoginAsync(userName);

            User currentUser;

            if (existingUser != null)
            {
                existingUser.ConnectionId = id;
                existingUser.IsOnline = true;
                await repoUser.UpdateAsync(existingUser);
                await repoUser.SaveAll();
                currentUser = existingUser;
            }
            else
            {
                currentUser = new User()
                {
                    ConnectionId = id,
                    Login = userName,
                    IsOnline = true
                };
                await repoUser.AddAsync(currentUser);
                await repoUser.SaveAll();
            }
            var allUsers = (await repoUser.GetAllAsync())
                .Where(u => u.IsOnline)
                .Select(u => new { u.ConnectionId, u.Login, u.IsOnline });

            var lastMessages = (await repoMessage.GetLatestMessagesAsync(10))
                .Select(m => new { m.MesaageText, Username = m.User?.Login, Time = m.CreatedAt });

            // Виклик методу Connected лише на поточному клієнті, який звернувся до сервера
            await Clients.Caller.SendAsync("Connected", currentUser.ConnectionId, currentUser.Login, allUsers, lastMessages);
            // Виклик методу NewUserConnected на всіх клієнтах, окрім клієнта з певним id
            await Clients.AllExcept(id).SendAsync("NewUserConnected", currentUser.ConnectionId, currentUser.Login);
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Connect method: {ex.Message}");
            throw;
        }
    }

    // OnDisconnectedAsync спрацьовує під час відключення клієнта.
    // Як параметр передається повідомлення про помилку, яке описує,
    // чому відбулося відключення.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await repoUser.GetByIdAsync(Context.ConnectionId);
        if (user != null)
        {
            user.IsOnline = false;
            await repoUser.UpdateAsync(user);
            await repoUser.SaveAll();

            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId, user.Login);
        }

        await base.OnDisconnectedAsync(exception);
    }
}