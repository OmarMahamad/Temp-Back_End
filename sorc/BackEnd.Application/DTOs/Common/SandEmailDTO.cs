
using System.ComponentModel.DataAnnotations;

using System.Net.Mail;


namespace BackEnd.Application.DTOs.Common
{
    public class SandEmailDTO
    {
        [EmailAddress, Required]
        public string EmailTo { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }

        public List<Attachment>? Attachments { get; set; }
    }
}
