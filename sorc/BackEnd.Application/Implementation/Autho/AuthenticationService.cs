using BackEnd.Application.ApplicationServices.Autho;
using BackEnd.Application.DTOs.AuthoDtos.Requset;
using BackEnd.Application.DTOs.AuthoDtos.Respone;
using BackEnd.Domin.Entity;
using BackEnd.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Implementation.Autho
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IConfiguration _config;

        private readonly IUnitOfWork _unitOfWork;

        public AuthenticationService(IConfiguration config, IUnitOfWork unitOfWork)
        {
            _config = config;
            _unitOfWork = unitOfWork;
        }
        private async Task<Response<string>> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                var token = Convert.ToBase64String(randomNumber);

                return Response<string>.Success(token, "Token generated successfully");
            }
        }

        public async Task<Response<AuthoResponseDto>> GenerateTokensAsync(AuthorizationRequestDto user)
        {
            try
            {
                // 1. توليد Access Token
                var accessTokenResponse = await GenerateTokenAsync(user);
                if (!accessTokenResponse.IsSuccess)
                    return Response<AuthoResponseDto>.Failure(accessTokenResponse.Message);

                // 2. توليد Refresh Token
                var refreshTokenResponse = await GenerateRefreshTokenAsync();
                if (!refreshTokenResponse.IsSuccess)
                    return Response<AuthoResponseDto>.Failure(refreshTokenResponse.Message);

                var refreshTokenEntity = AuthoRepository.Create(
                    userId: user.id,
                    token: accessTokenResponse.Data,
                    refresh: refreshTokenResponse.Data,
                    validMinutes: 7 * 24 * 60 // 7 أيام
                );

                await _unitOfWork.Repository<AuthoRepository>().AddItemAsync(refreshTokenEntity);
                await _unitOfWork.SaveChangesAsync();
                // 4. إرجاع النتيجة النهائية
                var dto = new AuthoResponseDto
                {
                    AccessToken = accessTokenResponse.Data,
                    RefreshToken = refreshTokenResponse.Data
                };

                return Response<AuthoResponseDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response<AuthoResponseDto>.Failure(ex.Message);
            }
        }


        private async Task<Response<string>> GenerateTokenAsync(AuthorizationRequestDto user)
        {
            try
            {
                var authClaims = new List<Claim>
                {
                    new Claim("NameIdentifier", user.id.ToString()),
                    new Claim("Roles", user.Roles),
                    new Claim("Name",user.name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var authSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"], // ← Audience متطابقة
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:DurationInMinutes"])),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Response<string>.Success(tokenString);
            }
            catch (Exception ex)
            {
                return Response<string>.Failure(ex.Message);
            }
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                ValidateLifetime = false // ← أهم نقطة هنا
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtToken
                || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public async Task<Response> Logout(string refreshToken)
        {
            try
            {
                var authoRespones = await _unitOfWork.Repository<AuthoRepository>().GetItemAsync(t => t.Refresh == refreshToken);

                if (authoRespones is null)
                    return Response.Failure("Refresh token not found.");

                var autho = authoRespones;
                if (autho == null)
                    return Response.Failure("Unexpected error: Token not found.");

                if (autho.IsRevoked)
                    return Response.Failure("This session is already canceled.");

                autho.IsValid();
                await _unitOfWork.Repository<AuthoRepository>().UpdateItemAsync(autho, autho.Id);
                await _unitOfWork.SaveChangesAsync();

                return Response.Success("You have been logged out of this session.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response.Failure(ex.Message);
            }
        }
        public async Task<Response> LogoutFromAllSessions(Guid userid)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var authoRespones = await _unitOfWork.Repository<AuthoRepository>().GetAllItemsAsync(t => t.UserId == userid);
                if (!authoRespones.Any())
                    return Response.Failure("No active sessions found for this user.");
                var authoList = authoRespones.Where(t => !t.IsRevoked).ToList();
                if (!authoList.Any())
                    return Response.Failure("All sessions are already canceled.");
                foreach (var autho in authoList)
                {
                    autho.IsValid();
                    await _unitOfWork.Repository<AuthoRepository>().UpdateItemAsync(autho, autho.Id);
                }
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return Response.Success("You have been logged out from all sessions.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response.Failure(ex.Message);
            }
        }


        public async Task<Response<Guid>> GetUserIdFromTokenAsync(string token)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(token);
                if (principal == null)
                    throw new SecurityTokenException("Invalid token");

                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    throw new Exception("UserId not found in token");
                var convertToGuid = Guid.Parse(userIdClaim.Value);
                return Response<Guid>.Success(convertToGuid);
            }
            catch (Exception ex)
            {
                return Response<Guid>.Failure(ex.Message);
            }


        }


        public async Task<Response> ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["Jwt:Key"])),

                    ValidateLifetime = true
                };

                tokenHandler.ValidateToken(token, validationParameters, out _);
                return Response.Success();
            }
            catch (Exception ex)
            {
                return Response.Failure(ex.Message);
            }
        }

        public async Task<Response<AuthoResponseDto>> RefreshAccessTokenAsync(string refreshToken)
        {
            try
            {
                var authoresult = await _unitOfWork.Repository<AuthoRepository>().GetItemAsync(t => t.Refresh == refreshToken && !t.IsRevoked, include: i => i.Include(u => u.User));
                var autho = authoresult;
                if (autho == null)
                    return Response<AuthoResponseDto>.Failure("Invalid refresh token.");

                if (autho.ExpireAt < DateTime.UtcNow)
                    return Response<AuthoResponseDto>.Failure("Refresh token has expired. Please log in again.");

                var validateTokenResult = await ValidateToken(autho.Token);
                if (validateTokenResult.IsSuccess)
                    return Response<AuthoResponseDto>.Success(
                        new AuthoResponseDto
                        {
                            AccessToken = autho.Token,
                            RefreshToken = autho.Refresh
                        },
                        "Access token is still valid."
                    );

                var userIdResponse = await GetUserIdFromTokenAsync(autho.Token);
                if (!userIdResponse.IsSuccess)
                    return Response<AuthoResponseDto>.Failure("Failed to extract user ID from token.");

                var user = new AuthorizationRequestDto
                {
                    id = userIdResponse.Data,
                    Roles = autho.User.Role.ToString(),
                    email = autho.User.Email.Value,
                };

                var tokenResponse = await GenerateTokenAsync(user);
                if (!tokenResponse.IsSuccess)
                    return Response<AuthoResponseDto>.Failure("Failed to generate new tokens.");
                var refreshTokenResponse = await GenerateRefreshTokenAsync();
                if (!refreshTokenResponse.IsSuccess)
                    return Response<AuthoResponseDto>.Failure("Failed to generate new refresh token.");

                var newrefreshToken = refreshTokenResponse.Data;
                var newTokens = tokenResponse.Data;

                // Rotation: استبدل الريفرش توكن كمان
                autho.RotateTokens(newTokens, newrefreshToken, validMinutes: 7 * 24 * 60);
                await _unitOfWork.Repository<AuthoRepository>().UpdateItemAsync(autho, autho.Id);
                await _unitOfWork.SaveChangesAsync();
                return Response<AuthoResponseDto>.Success(new AuthoResponseDto
                {
                    AccessToken = newTokens,
                    RefreshToken = newrefreshToken
                }, "Token refreshed successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response<AuthoResponseDto>.Failure(ex.Message);
            }
        }

    }
}
