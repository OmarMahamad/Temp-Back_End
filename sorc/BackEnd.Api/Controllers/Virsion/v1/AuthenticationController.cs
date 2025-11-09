using Asp.Versioning;
using BackEnd.Application.ApplicationServices.Autho;
using BackEnd.Application.DTOs.AuthoDtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Api.Controllers.Virsion.v1
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthentication _authentication;

        public AuthenticationController(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> LogoutAsync([FromForm]LogoutRequestDto requestDto)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authentication.LogoutAsync(requestDto.RefreshToken);
            return HandleResponse(result);
        }

        [HttpPost("Logout-FromAllSessions/{userid:guid}")]
        public async Task<IActionResult> LogoutFromAllSessions([FromRoute]Guid userid)
        {
            if (userid == Guid.Empty)
                return BadRequest("Invalid input");
            var result=await _authentication.LogoutFromAllSessions(userid);
            return HandleResponse(result);
        }
        [HttpPost("ValidateToken")]
        public async Task<IActionResult> ValidateTokenAsync([FromBody] ValidateTokenRequestDto requestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result =await _authentication.ValidateTokenAsync(requestDto.token);
            return HandleResponse(result);
        }
        [HttpPost("Refresh-AccessToken")]
        public async Task<IActionResult> RefreshAccessTokenAsync(RefreshAccessTokenRequestDto requestDto)
        {
            if(!ModelState.IsValid)return BadRequest(ModelState);
            var result = await _authentication.RefreshAccessTokenAsync(requestDto.RefreshToken);
            return HandleResponse(result);
        }

    }
}
