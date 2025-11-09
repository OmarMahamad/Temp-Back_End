using BackEnd.Application.DTOs.AuthoDtos.Request;
using BackEnd.Application.DTOs.AuthoDtos.Respone;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.ApplicationServices.Autho
{
    public interface IAuthorizationService
    {
        Task<Response> LoginAsync(LoginRequestDto login);
        Task<Response> RegisterAsync(RegisterRequestDTo register);
        Task<Response> SandVerifiedTokenToEmailAsync(Guid userId, string userName, string frontendUrl);
        Task<Response> EmailVerifiedAsync(Guid Token);
        Task<Response> ResendVerificationEmailAsync(string Email, string url);
        Task<Response> ForgotPasswordAsync(string email);
        Task<Response> ResetPasswordAsync(ResetPasswordRequestDto dto);
        Task<Response> CheckOtpCodeAsync(string code);
    }

}
