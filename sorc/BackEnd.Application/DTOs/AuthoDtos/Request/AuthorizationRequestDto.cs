using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.DTOs.AuthoDtos.Requset
{
    public class AuthorizationRequestDto
    {
        [Required]
        public Guid id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string Roles { get; set; }
        [Required]
        public string name { get; set; }
    }
}
