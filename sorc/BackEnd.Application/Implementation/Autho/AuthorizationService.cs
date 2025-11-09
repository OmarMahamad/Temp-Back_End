using BackEnd.Application.ApplicationServices.Autho;
using BackEnd.Application.Common;
using BackEnd.Application.DTOs.AuthoDtos.Request;
using BackEnd.Application.DTOs.AuthoDtos.Requset;
using BackEnd.Application.DTOs.Common;
using BackEnd.Domin.Entity;
using BackEnd.Domin.Entity.Enums;
using BackEnd.Domin.ValueObjects;
using BackEnd.Domin.ValueObjects.ValueObjectsUser;
using BackEnd.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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

        public async Task<Response> CheckOtpCodeAsync(string code)
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

        public async Task<Response> EmailVerifiedAsync(Guid Token)
        {
            try
            {
                var exit = await _unitOfWork.Repository<EmailVerificationToken>().GetItemAsync(o => o.Token == Token,
                include: i => i.Include(i => i.User));
                if (exit is null)
                    return ResponseFactory.NotFound();

                if (!exit.IsValid())
                    return ResponseFactory.AlreadyExists("this token not valid ");

                var user = exit.User;

                if (user is null)
                    return ResponseFactory.NotFound();
                if (user.IsEmailVerified)
                    return ResponseFactory.AlreadyExists("this email already virfy");
                exit.MarkAsUsed();
                user.VerifyEmail();
                await _unitOfWork.Repository<User>().UpdateItemAsync(user, user.Id);
                await _unitOfWork.Repository<EmailVerificationToken>().UpdateItemAsync(exit, exit.Id);
                await _unitOfWork.SaveChangesAsync();
                return ResponseFactory.Success();

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response.Failure(ex.Message);
            }
        }

        public async Task<Response> ForgotPasswordAsync(string email)
        {
            try
            {
                var emailv = Email.Create(email); // سيرمي exception إذا كان غير صحيح
                var emailValue = emailv.Value; // احفظ القيمة في string
                var exit = await _unitOfWork.Repository<User>().GetItemAsync(i => i.Email.Value == emailValue);

                if (exit is null) return ResponseFactory.NotFound();

                var code = _securtyService.GenerateOtpCode();
                var otp = OtpCode.Create(exit.Id, code);

                var body = $@"
                <h2>Password Reset Request</h2>
                <p>Your OTP code is: <strong>{otp.Code}</strong></p>
                <p>This code will expire in 10 minutes.</p>";
               
                var sandEmail = new SandEmailDTO
                {
                    Body = body,
                    Subject = "Password Reset OTP",
                    EmailTo = exit.Email.Value
                };
                await _emailService.SendEmailAsync(sandEmail);
                await _unitOfWork.Repository<OtpCode>().AddItemAsync(otp);
                await _unitOfWork.SaveChangesAsync();
                return Response.Success();
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response.Failure(ex.Message);
            }

        }

        public async Task<Response> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var email = Email.Create(request.Email); // سيرمي exception إذا كان غير صحيح
                var emailValue = email.Value; // احفظ القيمة في string
                var exit = await _unitOfWork.Repository<User>().GetItemAsync(r => r.Email.Value== emailValue);
                if (exit is null)
                {
                    return ResponseFactory.NotFound();
                }
                var hashpassword = _securtyService.VerifyPassword(exit.Password.Hash, request.Password, exit.Password.Salt);
                if (!hashpassword)
                    return Response.Failure("Password or Email is not match ");
                if (!exit.IsEmailVerified)
                    return ResponseFactory.Conflict("Please verify your email address before logging in.");

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
                return ResponseFactory.Success(autho.Data);
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response.Failure(ex.Message);
            }

        }

        public async Task<Response> RegisterAsync(RegisterRequestDTo request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // ✅ 1. التحقق من صحة البريد أولاً
                var email = Email.Create(request.Email); // سيرمي exception إذا كان غير صحيح
                var emailValue = email.Value; // احفظ القيمة في string

                // ✅ 2. البحث باستخدام string value
                var userExit = await _unitOfWork.Repository<User>()
                    .GetItemAsync(u => u.Email.Value == emailValue);

                if (userExit is not null)
                {
                    return ResponseFactory.AlreadyExists("Email already exists");
                }

                string imageUrl = "/images/default.png";
                string? imagePublicId = null;

                if (request.file != null)
                {
                    if (request.file.Length > 2 * 1024 * 1024)
                        return Response.Failure("Image size too large (max 2MB)");

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var ext = Path.GetExtension(request.file.FileName).ToLower();

                    if (!allowedExtensions.Contains(ext))
                        return Response.Failure("Invalid image format. Allowed: JPG, PNG, JPEG");

                    using var stream = request.file.OpenReadStream();
                    var fileResult = await _fileService.UploadAsync(stream, request.file.FileName, DefaultImageType.User);

                    if (!fileResult.IsSuccess)
                        return Response.Failure("Failed to upload image");

                    imageUrl = fileResult.Data.Url;
                    imagePublicId = fileResult.Data.PublicId;
                }

                var hashedPassword = _securtyService.HashPassword(request.Password, out string salt);
                var add = Address.Create(request.Address.Street, request.Address.City);
                await _unitOfWork.Repository<Address>().AddItemAsync(add);

                var user = User.Create(
                    addressId: add.Id,
                    name: new UserName(request.Name),
                    phone: new PhoneNumber(request.Phone),
                    email: email, // ✅ استخدم الـ email object الذي أنشأته في البداية
                    password: new Password(hashedPassword, salt),
                    profile: new ProfileImage(imageUrl, imagePublicId),
                    role: RoleType.user
                );

                await _unitOfWork.Repository<User>().AddItemAsync(user);
                await _unitOfWork.CommitAsync();
                await SandVerifiedTokenToEmailAsync(user.Id, user.Name.Value, request.verify_email_url);
                await _unitOfWork.SaveChangesAsync();

                return ResponseFactory.Success();
            }
            catch (ArgumentException ex) // ✅ لالتقاط أخطاء الـ validation
            {
                await _unitOfWork.RollbackAsync();
                return Response.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Response.Failure(ex.Message);
            }
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

        public async Task<Response> ResetPasswordAsync(ResetPasswordRequestDto dto)
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
