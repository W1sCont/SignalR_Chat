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
                UserId = user.id
            };
            await repoMessage.AddAsync(newMessage);
            await repoMessage.SaveAll();

            // Виклик методу AddMessage на всіх клієнтах
            await Clients.All.SendAsync("AddMessage", username, message);
        }
    }

    // Підключення нового користувача
    public async Task Connect(string userName)
    {
        var id = Context.ConnectionId;

        User newUser = new User()
        {
            ConnectionId = id,
            Login = userName
        };
        try
        {
            await repoUser.AddAsync(newUser);
            await repoUser.SaveAll();

            var allUsers = await repoUser.GetAllAsync();

            // Виклик методу Connected лише на поточному клієнті, який звернувся до сервера
            await Clients.Caller.SendAsync("Connected", newUser.ConnectionId, newUser.Login, allUsers);

            // Виклик методу NewUserConnected на всіх клієнтах, окрім клієнта з певним id
            await Clients.AllExcept(id).SendAsync("NewUserConnected", newUser.ConnectionId, newUser.Login);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    // OnDisconnectedAsync спрацьовує під час відключення клієнта.
    // Як параметр передається повідомлення про помилку, яке описує,
    // чому відбулося відключення.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await repoUser.GetByIdAsync(Context.ConnectionId);
        if (user?.ConnectionId == Context.ConnectionId)
        {
            await repoUser.DeleteAsync(user);
            await repoUser.SaveAll();

            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId, user.Login);
        }

        await base.OnDisconnectedAsync(exception);
    }
}