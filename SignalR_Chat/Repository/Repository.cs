using Microsoft.EntityFrameworkCore;
using SignalR_Chat.EF;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using EFCore = Microsoft.EntityFrameworkCore.EF;

namespace SignalR_Chat.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly UserContext _context;

        public Repository(UserContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _context.Set<T>().AsNoTracking().ToListAsync();

        public async Task<User?> GetByLoginAsync(string UserName) =>
            await _context.Set<User>().FirstOrDefaultAsync(e => e.Login == UserName);

        public async Task<User?> GetByIdAsync(string ConnectionId) =>
            await _context.Set<User>().FirstOrDefaultAsync(u => (u.ConnectionId) == ConnectionId);
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task SaveAll()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Message>> GetLatestMessagesAsync(int count = 10)
        {
            return await _context.Messages
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .OrderBy(m => m.CreatedAt) 
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
