using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealTimeChatApp.Infrastructure.Persistence;
using RealTimeChatApp.Domain.Entities;
using RealTimeChatApp.Application.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly AppDbContext _context;

    public GroupController(AppDbContext context)
    {
        _context = context;
    }


    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized("Invalid user token");

        var group = new Group
        {
            Name = dto.Name,
            IsPrivate = dto.IsPrivate,
            OwnerId = Guid.Parse(userId),
            Members = new List<GroupMember>
            {
                new GroupMember
                {
                    UserId = Guid.Parse(userId),
                    IsApproved = true,
                    JoinedAt = DateTime.UtcNow,
                    IsAdmin = true
                }
            }

        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return Ok(new { group.Id, group.Name });
    }


    [HttpPost("add-member")]
    [Authorize]
    public async Task<IActionResult> AddMemberToGroup([FromBody] AddGroupMemberDto dto)
    {
        var group = await _context.Groups.FindAsync(dto.GroupId);
        if (group == null)
            return NotFound("Group not found.");

        var existingMember = await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == dto.GroupId && m.UserId == dto.UserId);

        if (existingMember != null)
            return BadRequest("User is already a member of the group.");

        var newMember = new GroupMember
        {
            GroupId = dto.GroupId,
            UserId = dto.UserId,
            IsApproved = !group.IsPrivate,
            JoinedAt = DateTime.UtcNow,
            IsAdmin = false
        };

        _context.GroupMembers.Add(newMember);
        await _context.SaveChangesAsync();

        return Ok("User added to group.");
    }


    [HttpPost("{groupId}/invite")]
    [Authorize]
    public async Task<IActionResult> InviteUser(Guid groupId, [FromBody] InviteUserDto dto)
    {
        var inviterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (inviterId == null) return Unauthorized("Invalid user");

        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return NotFound("Group not found.");

        if (!group.IsPrivate)
            return BadRequest("Invites are only for private groups.");

        var inviter = group.Members.FirstOrDefault(m => m.UserId == Guid.Parse(inviterId));
        if (inviter == null || !inviter.IsAdmin)
            return Forbid("Only admins can send invites.");

        var invite = new GroupInvite
        {
            GroupId = groupId,
            InvitedUserId = dto.InvitedUserId,
            InvitedByUserId = Guid.Parse(inviterId),
            CreatedAt = DateTime.UtcNow
        };

        _context.GroupInvites.Add(invite);
        await _context.SaveChangesAsync();

        return Ok("Invite sent.");
    }

    [HttpGet("{groupId}")]
    [Authorize]
    public async Task<IActionResult> GetGroupDetails(Guid groupId)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return NotFound("Group not found.");

        return Ok(new
        {
            group.Id,
            group.Name,
            group.IsPrivate,
            Members = group.Members.Select(m => new {
                m.UserId,
                m.User.Username,
                m.IsAdmin,
                m.IsApproved,
                m.JoinedAt
            })
        });
    }

[HttpGet("my-groups")]
[Authorize]
public async Task<IActionResult> GetMyGroups()
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var groups = await _context.GroupMembers
        .Include(m => m.Group)
        .Where(m => m.UserId == userId)
        .Select(m => new {
            m.Group.Id,
            m.Group.Name,
            m.Group.IsPrivate,
            m.IsAdmin
        })
        .ToListAsync();

    return Ok(groups);
}


[HttpPost("accept-invite/{groupId}")]
[Authorize]
public async Task<IActionResult> AcceptInvite(Guid groupId)
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var invite = await _context.GroupInvites
        .FirstOrDefaultAsync(i => i.GroupId == groupId && i.InvitedUserId == userId);

    if (invite == null) return NotFound("Invite not found.");

    _context.GroupInvites.Remove(invite);
    _context.GroupMembers.Add(new GroupMember
    {
        GroupId = groupId,
        UserId = userId,
        IsApproved = true,
        JoinedAt = DateTime.UtcNow,
        IsAdmin = false
    });

    await _context.SaveChangesAsync();
    return Ok("Joined the group.");
}

[HttpPost("reject-invite/{groupId}")]
[Authorize]
public async Task<IActionResult> RejectInvite(Guid groupId)
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var invite = await _context.GroupInvites
        .FirstOrDefaultAsync(i => i.GroupId == groupId && i.InvitedUserId == userId);

    if (invite == null) return NotFound("Invite not found.");

    _context.GroupInvites.Remove(invite);
    await _context.SaveChangesAsync();
    return Ok("Invite rejected.");
}


[HttpPost("{groupId}/leave")]
[Authorize]
public async Task<IActionResult> LeaveGroup(Guid groupId)
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var member = await _context.GroupMembers
        .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

    if (member == null)
        return NotFound("You're not a member of this group.");

    _context.GroupMembers.Remove(member);
    await _context.SaveChangesAsync();

    return Ok("You left the group.");
}



}
