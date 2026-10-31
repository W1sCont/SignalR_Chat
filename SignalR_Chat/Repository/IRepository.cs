using SignalR_Chat.EF;
namespace SignalR_Chat.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string ConnectionId);
        Task<User> GetByLoginAsync(string UserName);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveAll();
    }
}
