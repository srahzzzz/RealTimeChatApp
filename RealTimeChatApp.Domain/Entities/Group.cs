namespace RealTimeChatApp.Domain.Entities
{
    public class Group
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }

        public ICollection<GroupMember> Members { get; set; }
        public Guid OwnerId { get; set; }
        public User Owner { get; set; }

        public ICollection<GroupInvite> Invites { get; set; }
    }

}
