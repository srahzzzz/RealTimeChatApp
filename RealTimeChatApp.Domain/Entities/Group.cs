namespace RealTimeChatApp.Domain.Entities
{
    public class Group
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }

        public ICollection<GroupMember> Members { get; set; }
    }
}
