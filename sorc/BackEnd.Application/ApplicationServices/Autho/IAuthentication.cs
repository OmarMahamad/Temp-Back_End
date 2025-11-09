using BackEnd.Application.DTOs.AuthoDtos.Requset;
using BackEnd.Application.DTOs.AuthoDtos.Respone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.ApplicationServices.Autho
{
    public interface IAuthentication
    {
        Task<Response<AuthoResponseDto>> GenerateTokensAsync(AuthorizationRequestDto user); // توليد Access Token و Refresh Token
                                                                                            // توليد Refresh Token
        Task<Response<Guid>> GetUserIdFromTokenAsync(string token);  // استخراج UserId من token
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token); // استخراج ClaimsPrincipal حتى لو الـ token منتهي
        Task<Response> ValidateTokenAsync(string token);
        Task<Response> LogoutFromAllSessions(Guid userid);
        Task<Response<AuthoResponseDto>> RefreshAccessTokenAsync(string refreshToken);
        Task<Response> LogoutAsync(string refreshToken);


    }
}
