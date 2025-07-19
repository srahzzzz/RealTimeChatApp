namespace RealTimeChatApp.Application.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; }
    }
}
