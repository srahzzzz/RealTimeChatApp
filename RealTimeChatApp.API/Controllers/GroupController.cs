
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealTimeChatApp.Infrastructure.Persistence;
using RealTimeChatApp.Domain.Entities;
using RealTimeChatApp.Application.DTOs;
using System.Security.Claims;

public class GroupController : ControllerBase
{
    private readonly AppDbContext _context;

    public GroupController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var group = new Group
        {
            Name = dto.Name,
            IsPrivate = dto.IsPrivate,
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
}
