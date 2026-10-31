namespace SignalR_Chat.EF
{
    public class Message
    {
        public int id { get; set; }
        public string MesaageText { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
