namespace RealTimeChatApp.Domain.Entities
{
public class Message

{
    public int Id { get; set; }
    public Guid GroupId { get; set; }
    public Group Group { get; set; }

     public Guid SenderId { get; set; }
     public User Sender { get; set; }

    public string Content { get; set; }
    public string? FilePath { get; set; } // Optional file/image
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
}