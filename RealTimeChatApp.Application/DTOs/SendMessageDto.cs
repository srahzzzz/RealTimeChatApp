public class SendMessageDto
{
    public Guid GroupId { get; set; }
    public string Content { get; set; }
    // We’ll get senderId from the token — not from the body
}
