using Asp.Versioning;
using BackEnd.Application.ApplicationServices.Autho;
using BackEnd.Application.DTOs.AuthoDtos.Request;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Api.Controllers.Virsion.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthorizationController : BaseController
    {
        private readonly IAuthorizationService _authorization;

        public AuthorizationController(IAuthorizationService authorization)
        {
            _authorization = authorization;
        }
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginRequestDto loginRequestDto )
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);
            var token = await _authorization.LoginAsync(loginRequestDto);
            return HandleResponse(token);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromForm]RegisterRequestDTo registerRequestDTo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result=await _authorization.RegisterAsync(registerRequestDTo);
            return HandleResponse(result);
        }

        [HttpPost("email-verify/{token:guid}")]
        public async Task<IActionResult> EmailVerifiedAsync([FromRoute] Guid Token)
        {
            if (Token == Guid.Empty)
            {
                return BadRequest("Invalid token.");
            }
            var result = await _authorization.EmailVerifiedAsync(Token);
            return HandleResponse(result);
        }
        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmailAsync([FromForm]ResendVerificationEmailRequestDto requestDto)
        {
            if(!ModelState.IsValid)return BadRequest(ModelState);
            var result = await _authorization.ResendVerificationEmailAsync(requestDto.email, requestDto.Url);
            return HandleResponse(result);
        }
        [HttpPost("ForgotPasswordAsync")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequestDto requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = await _authorization.ForgotPasswordAsync(requestDto.Email);
            return HandleResponse(result);
        }
        [HttpPost("Check-Otp-Code")]
        public async Task<IActionResult> CheckOtpCodeAsync(CheckOtpCodeRequestDto requestDto)
        {
            if(!ModelState.IsValid)
                return BadRequest();
            var result=await _authorization.CheckOtpCodeAsync(requestDto.Code);
            return HandleResponse(result);
        }
        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequestDto requestDto)
        {
            if (ModelState.IsValid) return BadRequest(ModelState);
            var result =await _authorization.ResetPasswordAsync(requestDto);
            return HandleResponse(result);
        }

    }
}
