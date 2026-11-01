using System.Collections;

namespace SignalR_Chat.EF
{
    public class User
    {
        public int id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public bool IsOnline { get; set; } = true;
        public ICollection<Message> messages { get; set; } = new List<Message>();
    }
}
