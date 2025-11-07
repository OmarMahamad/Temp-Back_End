using BackEnd.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Common
{
    public interface IEmailService
    {
        Task<Response> SendEmailAsync(SandEmailDTO email);

    }
}
