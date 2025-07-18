namespace RealTimeChatApp.Application.DTOs;

public class AddGroupMemberDto
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
}
