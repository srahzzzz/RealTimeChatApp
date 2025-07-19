using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RealTimeChatApp.Application.DTOs
{
    public class UploadFileDto
    {
        [Required]
        public Guid GroupId { get; set; }

        public string? Content { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}
