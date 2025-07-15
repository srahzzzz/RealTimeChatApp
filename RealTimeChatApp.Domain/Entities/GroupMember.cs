namespace RealTimeChatApp.Domain.Entities
{
    public class GroupMember
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid GroupId { get; set; }
        public Group Group { get; set; }

        public bool IsApproved { get; set; } = false;
    }
}
