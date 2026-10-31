using Microsoft.EntityFrameworkCore;
using EFCore = Microsoft.EntityFrameworkCore.EF;
using SignalR_Chat.EF;

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
            await _context.Set<User>().AsNoTracking().FirstOrDefaultAsync(e => e.Login == UserName);

        public async Task<T?> GetByIdAsync(string ConnectionId) =>
            await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => EFCore.Property<string>(e, "ConnectionId") == ConnectionId);
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
    }
}
