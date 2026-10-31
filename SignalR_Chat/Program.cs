using SignalR_Chat.Extensions;
using SignalR_Chat;
using SignalR_Chat.Repository;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSignalR();

string? connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (connection != null)
    builder.Services.AddUserContext(connection);

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chat");   

app.Run();