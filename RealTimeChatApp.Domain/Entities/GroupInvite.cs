using RealTimeChatApp.Domain.Entities;

public class GroupInvite
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid InvitedUserId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsAccepted { get; set; } = false;

    public Group Group { get; set; }
    public User InvitedUser { get; set; }
    public User InvitedByUser { get; set; }
}
