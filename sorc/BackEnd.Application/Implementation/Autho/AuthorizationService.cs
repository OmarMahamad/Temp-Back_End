using BackEnd.Application.ApplicationServices.Autho;
using BackEnd.Application.Common;
using BackEnd.Application.DTOs.AuthoDtos.Request;
using BackEnd.Application.DTOs.AuthoDtos.Requset;
using BackEnd.Application.DTOs.Common;
using BackEnd.Domin.Entity;
using BackEnd.Domin.ValueObjects.ValueObjectsUser;
using BackEnd.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Implementation.Autho
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ISecurtyService _securtyService;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly  IAuthentication _authorization;

        public AuthorizationService(IUnitOfWork unitOfWork, IAuthentication authorization, ISecurtyService securtyService, IEmailService emailService, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _authorization = authorization;
            _securtyService = securtyService;
            _emailService = emailService;
            _fileService = fileService;

        }

        public async Task<Response> CheckOtpCode(string code)
        {
            try
            {
                // 🔹 جلب الكود من المستودع
                var codeEntity = await _unitOfWork.Repository<OtpCode>().GetItemAsync(c => c.Code == code);

                // 🔹 تحقق من أن البيانات موجودة وليست فارغة
                if (codeEntity == null)
                    return ResponseFactory.NotFound();

                // 🔹 تحقق من انتهاء صلاحية الكود
                if (codeEntity.ExpiryDate < DateTime.UtcNow)
                    return Response.Failure("Code expired");

                // 🔹 تحقق إذا تم استخدام الكود مسبقًا
                if (codeEntity.IsUsed)
                    return Response.Failure("Code already used");

                // 🔹 حذف أو تحديث الكود في قاعدة البيانات (حسب تصميمك)
                await _unitOfWork.Repository<OtpCode>().DeleteItemAsync(codeEntity.Id);
                await _unitOfWork.SaveChangesAsync();

                // 🔹 إعادة النتيجة بنجاح
                return Response.Success("Code is valid");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                // 🔹 إعادة أي خطأ غير متوقع
                return Response.Failure($"Error: {ex.Message}",StatusCodeResponeType.ExternalServiceError);
            }
        }

        public Task<Response> EmailVerifiedAsync(Guid Token)
        {
            throw new NotImplementedException();
        }

        public Task<Response> ForgotPasswordAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> LoginAsync(LoginRequestDto request)
        {
            var exit=await _unitOfWork.Repository<User>().GetItemAsync(r=>r.Email.Value==request.Email);
            if (exit is null)
            {
                return ResponseFactory.NotFound();
            }
            var hashpassword = _securtyService.VerifyPassword(exit.Password.Hash, request.Password, exit.Password.Salt);
            if(!hashpassword)
                return Response.Failure("Password or Email is not match ");
            var authodto = new AuthorizationRequestDto
            {
                email = exit.Email.Value,
                name = exit.Name.Value,
                id = exit.Id,
                Roles = exit.Role.ToString(),
            };
            var autho = await _authorization.GenerateTokensAsync(authodto);
            if (autho is null)
                return ResponseFactory.NotFound();
            return ResponseFactory.Success(autho);

        }

        public Task<Response> RegisterAsync(RegisterRequestDTo request)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> ResendVerificationEmailAsync(string Email, string url)
        {
            try
            {
                var user = await _unitOfWork.Repository<User>().GetItemAsync(e => e.Email.Value == Email);
                if (user is null)
                    return ResponseFactory.NotFound();

                if (user.IsEmailVerified)
                    return Response.Failure("Email already verified");

                // إنشاء وإرسال توكن جديد
                return await SandVerifiedTokenToEmailAsync(user.Id, user.Name.Value, url);
            }
            catch (Exception ex)
            {
                return Response.Failure(ex.Message);
            }
        }

        public async Task<Response> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                var uesrexit = await _unitOfWork.Repository<User>().GetItemAsync(u => u.Email.Value == dto.Email);
                if (uesrexit == null)
                    return ResponseFactory.NotFound();
                var user = uesrexit;
                var hashedPassword = _securtyService.HashPassword(dto.NewPassword, out string salt);

                user.ChangePassword(new Password(hashedPassword,salt));
                
                await _unitOfWork.Repository<User>().UpdateItemAsync(user, user.Id);
                var body = $@"
                <h2>Password Changed Successfully</h2>
                <p>Your password has been changed successfully. If you did not initiate this change, please contact support immediately.</p>";
                var sandEmail = new SandEmailDTO
                {
                    Body = body,
                    Subject = "Password Changed",
                    EmailTo = user.Email.Value
                };
                await _emailService.SendEmailAsync(sandEmail);
                await _unitOfWork.SaveChangesAsync();
                return ResponseFactory.Success("Password changed successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response.Failure(ex.Message);
            }
        }

        public async Task<Response> SandVerifiedTokenToEmailAsync(Guid userId, string userName, string frontendUrl)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // ✅ إنشاء توكن جديد
                var token = Guid.NewGuid();
                var tokenEntity = EmailVerificationToken.Create(userId, 120); // صالح لمدة 120 دقيقة

                await _unitOfWork.Repository<EmailVerificationToken>().AddItemAsync(tokenEntity);
                await _unitOfWork.SaveChangesAsync();

                // ✅ الرابط يكون لصفحة الفرونت (اللي فيها صفحة verify-email)
                var verificationLink = $"{frontendUrl}?token={token}";

                // ✅ جلب المستخدم للحصول على البريد الإلكتروني
                var userResult = await _unitOfWork.Repository<User>().GetItemByIdAsync(userId);
                if (userResult is null)
                    return ResponseFactory.NotFound();

                var userEmail = userResult.Email.Value;

                // ✅ رسالة البريد
                var body = $@"
        <h2>مرحبًا {userName}</h2>
        <p>يرجى النقر على الرابط أدناه لتفعيل بريدك الإلكتروني:</p>
        <a href='{verificationLink}' target='_blank'>تفعيل البريد الإلكتروني</a>
        <p>سينتهي هذا الرابط خلال ساعتين.</p>";

                var sandEmail = new SandEmailDTO
                {
                    EmailTo = userEmail,
                    Subject = "تفعيل البريد الإلكتروني",
                    Body = body
                };

                await _emailService.SendEmailAsync(sandEmail);

                return ResponseFactory.Success();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response.Failure(ex.Message);
            }
        }
    }
}
