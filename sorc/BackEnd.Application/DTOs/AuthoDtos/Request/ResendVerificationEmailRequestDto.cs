using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.DTOs.AuthoDtos.Request
{
    public class ResendVerificationEmailRequestDto
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string Url { get; set; }
    }
}
