using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RealTimeChatApp.Infrastructure.Persistence;
using RealTimeChatApp.Domain.Entities;
using RealTimeChatApp.Application.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RealTimeChatApp.Application.DTOs;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;



namespace RealTimeChatApp.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Cloudinary _cloudinary;

        public MessagesController(AppDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }



        // POST: api/messages/send
        [HttpPost("send")]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var groupExists = await _context.Groups.AnyAsync(g => g.Id == dto.GroupId);
            if (!groupExists)
                return NotFound("Group not found.");

            var message = new Message
            {
                GroupId = dto.GroupId,
                SenderId = Guid.Parse(userId),
                Content = dto.Content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message.Id,
                message.GroupId,
                message.SenderId,
                message.Content,
                message.SentAt
            });
        }


        // GET: api/messages/group/{groupId}
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetMessagesByGroup(Guid groupId)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.GroupId == groupId && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                GroupId = m.GroupId,
                SenderId = m.SenderId,
                SenderName = m.Sender?.Username ?? "Unknown",
                Content = m.Content,
                FilePath = m.FilePath,
                SentAt = m.SentAt,
                IsEdited = m.IsEdited
            }).ToList();

            return Ok(messageDtos);
        }

        // PUT: api/messages/{id}/edit
        [HttpPut("{id}/edit")]
        public async Task<IActionResult> EditMessage(int id, [FromBody] string newContent)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
                return NotFound("Message not found.");

            if (message.SenderId != userId)
                return Forbid("You can only edit your own messages.");

            message.Content = newContent;
            message.IsEdited = true;
            message.EditedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(message);
        }

        // DELETE: api/messages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);

            if (message == null)
                return NotFound("Message not found.");

            if (message.SenderId != userId)
                return Forbid("You can only delete your own messages.");

            message.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/messages/upload
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file uploaded.");

            var groupExists = await _context.Groups.AnyAsync(g => g.Id == dto.GroupId);
            if (!groupExists)
                return NotFound("Group not found.");

            // Create a unique file name and path
            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            // Save to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }



        string cloudUrl;

        using (var stream = dto.File.OpenReadStream())
        {
            var uploadParams = new CloudinaryDotNet.Actions.RawUploadParams
            {
                File = new FileDescription(dto.File.FileName, stream),
                Folder = "chat_uploads"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            cloudUrl = uploadResult.SecureUrl.ToString();
        }

        return Ok(new { Url = cloudUrl });
            // Save metadata in DB
            var message = new Message
            {
                GroupId = dto.GroupId,
                SenderId = userId,
                Content = dto.Content ?? "", // optional text
                FilePath = filePath, // or a cloud URL if you upload
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message.Id,
                message.GroupId,
                message.SenderId,
                message.FilePath,
                message.Content,
                message.SentAt
            });
        }



        // GET: api/messages/download/{id}
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null || string.IsNullOrEmpty(message.FilePath))
                return NotFound("File not found.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(message.FilePath);
            var fileName = Path.GetFileName(message.FilePath);

            return File(fileBytes, "application/octet-stream", fileName);
        }

        //pagination
        [HttpGet]
            public async Task<IActionResult> GetMessages(
                Guid groupId,
                int page = 1,
                int pageSize = 20,
                string? search = null)
            {
                var query = _context.Messages
                    .Where(m => m.GroupId == groupId && !m.IsDeleted);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(m =>
                        EF.Functions.ILike(m.Content, $"%{search}%"));
                }

                var totalCount = await query.CountAsync();

                var messages = await query
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    totalCount,
                    messages
                });
            }

            [HttpGet("ping")]
            [AllowAnonymous]
            public IActionResult Ping() => Ok("pong");



    }
}
