using BackEnd.Domin.ValueObjects.ValueObjectsUser;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.DTOs.AuthoDtos.Request
{
    public class RegisterRequestDTo
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public IFormFile? file { get; set; }
        [Required]
        public string verify_email_url { get; set; }
        public AddressRequestDTo Address { get; set; }

    }
}
