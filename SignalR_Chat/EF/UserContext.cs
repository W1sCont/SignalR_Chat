using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SignalR_Chat.EF
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }

        // Клас необхідний виключно для міграцій
        public class SampleContextFactory : IDesignTimeDbContextFactory<UserContext>
        {
            public UserContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<UserContext>();

                // Отримуємо конфігурацію із файлу appsettings.json
                ConfigurationBuilder builder = new ConfigurationBuilder();
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile("appsettings.json");
                IConfigurationRoot config = builder.Build();

                // Отримуємо рядок підключення з файлу appsettings.json
                string? connectionString = config.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
                return new UserContext(optionsBuilder.Options);
            }
        }
    }
}
