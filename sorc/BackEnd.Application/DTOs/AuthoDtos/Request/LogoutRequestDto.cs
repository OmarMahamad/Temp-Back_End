using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.DTOs.AuthoDtos.Request
{
    public class LogoutRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
