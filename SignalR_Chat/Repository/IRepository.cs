using SignalR_Chat.EF;
namespace SignalR_Chat.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<User?> GetByIdAsync(string ConnectionId);
        Task<User?> GetByLoginAsync(string UserName);
        Task<IEnumerable<Message>> GetLatestMessagesAsync(int count = 10);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveAll();
    }
}
