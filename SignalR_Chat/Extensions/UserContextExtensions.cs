using Microsoft.EntityFrameworkCore;
using SignalR_Chat.EF;

namespace SignalR_Chat.Extensions
{
    public static class UserContextExtensions
    {
        public static void AddUserContext(this IServiceCollection services, string connection) =>
            services.AddDbContext<UserContext>(options => options.UseSqlServer(connection));
    }
}
