using Asp.Versioning;
using BackEnd.Application.ApplicationServices.Autho;
using BackEnd.Application.DTOs.AuthoDtos.Request;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackEnd.Api.Controllers.Virsion.v1
{
    /// <summary>
    /// واجهة برمجية للتسجيل والتفويض
    /// </summary>
    /// <remarks>
    /// توفر هذه الواجهة endpoints لتسجيل الدخول، إنشاء حساب جديد، التحقق من البريد الإلكتروني، واستعادة كلمة المرور
    /// </remarks>
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

        /// <summary>
        /// تسجيل دخول المستخدم للنظام
        /// </summary>
        /// <remarks>
        /// يقوم بمصادقة المستخدم وإصدار Access Token و Refresh Token
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authorization/Login
        ///     Content-Type: application/json
        ///     
        ///     {
        ///       "email": "user@example.com",
        ///       "password": "P@ssw0rd123",
        ///       "rememberMe": true
        ///     }
        /// 
        /// ملاحظات:
        /// - يجب أن يكون البريد الإلكتروني مفعّلاً
        /// - rememberMe: true يجعل الجلسة تستمر لفترة أطول
        /// - يتم إرجاع Access Token و Refresh Token عند النجاح
        /// </remarks>
        /// <param name="loginRequestDto">بيانات تسجيل الدخول (البريد الإلكتروني وكلمة المرور)</param>
        /// <returns>رموز المصادقة (Access Token & Refresh Token)</returns>
        /// <response code="200">
        /// تم تسجيل الدخول بنجاح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم تسجيل الدخول بنجاح",
        ///       "data": {
        ///         "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///         "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///         "expiresIn": 3600,
        ///         "tokenType": "Bearer",
        ///         "user": {
        ///           "id": "123e4567-e89b-12d3-a456-426614174000",
        ///           "email": "user@example.com",
        ///           "userName": "username",
        ///           "roles": ["User"]
        ///         }
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// بيانات الإدخال غير صالحة
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "البيانات المدخلة غير صحيحة",
        ///       "errors": {
        ///         "Email": ["البريد الإلكتروني مطلوب"],
        ///         "Password": ["كلمة المرور مطلوبة"]
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="401">
        /// بيانات تسجيل الدخول غير صحيحة
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "البريد الإلكتروني أو كلمة المرور غير صحيحة"
        ///     }
        /// 
        /// </response>
        /// <response code="403">
        /// الحساب غير مفعّل أو محظور
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "يرجى تفعيل البريد الإلكتروني أولاً"
        ///     }
        /// 
        /// </response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("Login")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "تسجيل الدخول",
            Description = "مصادقة المستخدم والحصول على رموز الوصول (Access & Refresh Tokens)",
            OperationId = "Login",
            Tags = new[] { "Authorization" }
        )]
        [SwaggerResponse(200, "تم تسجيل الدخول بنجاح - يحتوي على Tokens", typeof(object))]
        [SwaggerResponse(400, "بيانات غير صالحة")]
        [SwaggerResponse(401, "بيانات تسجيل الدخول خاطئة")]
        [SwaggerResponse(403, "الحساب غير مفعّل")]
        public async Task<IActionResult> LoginAsync(
            [FromBody]
            [SwaggerRequestBody("بيانات تسجيل الدخول", Required = true)]
            LoginRequestDto loginRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var token = await _authorization.LoginAsync(loginRequestDto);
            return HandleResponse(token);
        }

        /// <summary>
        /// تسجيل مستخدم جديد في النظام
        /// </summary>
        /// <remarks>
        /// يقوم بإنشاء حساب مستخدم جديد وإرسال رابط تفعيل البريد الإلكتروني
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authorization/Register
        ///     Content-Type: multipart/form-data
        ///     
        ///     FirstName: أحمد
        ///     LastName: محمد
        ///     Email: ahmed@example.com
        ///     Password: P@ssw0rd123
        ///     ConfirmPassword: P@ssw0rd123
        ///     PhoneNumber: +201234567890
        ///     Address.city: city
        ///     address.street: street
        ///     profile :[file]
        /// 
        /// متطلبات كلمة المرور:
        /// - 8 أحرف على الأقل
        /// - حرف كبير واحد على الأقل
        /// - حرف صغير واحد على الأقل
        /// - رقم واحد على الأقل
        /// - رمز خاص واحد على الأقل (@$!%*?&amp;)
        /// 
        /// ملاحظات:
        /// - يتم إرسال بريد إلكتروني للتفعيل تلقائياً
        /// - لا يمكن تسجيل الدخول قبل تفعيل البريد
        /// - صورة الملف الشخصي اختيارية
        /// </remarks>
        /// <param name="registerRequestDTo">بيانات المستخدم الجديد</param>
        /// <returns>نتيجة عملية التسجيل مع رسالة تأكيد</returns>
        /// <response code="200">
        /// تم التسجيل بنجاح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم إنشاء الحساب بنجاح. يرجى التحقق من بريدك الإلكتروني لتفعيل الحساب",
        ///       
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// بيانات الإدخال غير صالحة أو البريد مسجل مسبقاً
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "البريد الإلكتروني مسجل مسبقاً",
        ///       "errors": {
        ///         "Email": ["البريد الإلكتروني مستخدم بالفعل"]
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("Register")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "تسجيل مستخدم جديد",
            Description = "إنشاء حساب جديد مع إرسال رابط تفعيل البريد الإلكتروني. استخدم multipart/form-data لإرسال الصورة الشخصية",
            OperationId = "Register",
            Tags = new[] { "Authorization" }
        )]
        [SwaggerResponse(200, "تم التسجيل بنجاح - تحقق من البريد للتفعيل", typeof(object))]
        [SwaggerResponse(400, "بيانات غير صالحة أو البريد مسجل مسبقاً")]
        public async Task<IActionResult> RegisterAsync(
            [FromForm]
            [SwaggerRequestBody("بيانات المستخدم الجديد مع الصورة الشخصية (اختياري)", Required = true)]
            RegisterRequestDTo registerRequestDTo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authorization.RegisterAsync(registerRequestDTo);
            return HandleResponse(result);
        }

        /// <summary>
        /// تفعيل البريد الإلكتروني للمستخدم
        /// </summary>
        /// <remarks>
        /// يتم استخدام هذا Endpoint من خلال الرابط المرسل في البريد الإلكتروني
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authorization/emailverify/123e4567-e89b-12d3-a456-426614174000
        /// 
        /// ملاحظات:
        /// - الرابط صالح لمدة 10 دقائق فقط
        /// - بعد التفعيل يمكن تسجيل الدخول مباشرة
        /// - في حالة انتهاء الرابط، استخدم "إعادة إرسال رابط التفعيل"
        /// </remarks>
        /// <param name="tokenValue">رمز التفعيل (GUID) المرسل في البريد الإلكتروني</param>
        /// <returns>نتيجة عملية التفعيل</returns>
        /// <response code="200">
        /// تم تفعيل البريد الإلكتروني بنجاح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم تفعيل بريدك الإلكتروني بنجاح. يمكنك الآن تسجيل الدخول",
        ///       "data": {
        ///         "emailVerified": true,
        ///         "verifiedAt": "2025-11-09T12:00:00Z"
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// رمز التفعيل غير صالح أو منتهي
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "رابط التفعيل غير صالح أو منتهي الصلاحية. يرجى طلب رابط جديد"
        ///     }
        /// 
        /// </response>
        /// <response code="404">رمز التفعيل غير موجود</response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("emailverify/{tokenValue:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "تفعيل البريد الإلكتروني",
            Description = "تفعيل حساب المستخدم باستخدام رمز التفعيل المرسل في البريد",
            OperationId = "EmailVerified",
            Tags = new[] { "Authorization" }
        )]
        [SwaggerResponse(200, "تم التفعيل بنجاح", typeof(object))]
        [SwaggerResponse(400, "رمز التفعيل غير صالح أو منتهي")]
        [SwaggerResponse(404, "رمز التفعيل غير موجود")]
        public async Task<IActionResult> EmailVerifiedAsync(
            [FromRoute]
            [SwaggerParameter("رمز التفعيل (GUID) من رابط البريد الإلكتروني", Required = true)]
            Guid tokenValue)
        {
            if (tokenValue == Guid.Empty)
            {
                return BadRequest("Invalid token.");
            }
            var result = await _authorization.EmailVerifiedAsync(tokenValue);
            return HandleResponse(result);
        }

        /// <summary>
        /// إعادة إرسال رابط تفعيل البريد الإلكتروني
        /// </summary>
        /// <remarks>
        /// يقوم بإرسال رابط تفعيل جديد في حالة انتهاء صلاحية الرابط السابق
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authorization/resend-verification-email
        ///     Content-Type: application/json
        ///     
        ///     {
        ///       "email": "user@example.com",
        ///       "url": "https://yourapp.com/verify"
        ///     }
        /// 
        /// ملاحظات:
        /// - Url: الرابط الأساسي لتطبيقك (سيتم إضافة token له)
        /// </remarks>
        /// <param name="requestDto">البريد الإلكتروني والرابط الأساسي للتطبيق</param>
        /// <returns>نتيجة عملية إعادة الإرسال</returns>
        /// <response code="200">
        /// تم إعادة إرسال رابط التفعيل بنجاح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم إرسال رابط التفعيل إلى بريدك الإلكتروني",
        ///       "data": {
        ///         "emailSent": true,
        ///         "expiresIn": 86400
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// بيانات غير صالحة أو محاولة إرسال متكررة
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "يرجى الانتظار 5 دقائق قبل طلب رابط جديد"
        ///     }
        /// 
        /// </response>
        /// <response code="404">
        /// البريد الإلكتروني غير مسجل
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "البريد الإلكتروني غير مسجل"
        ///     }
        /// 
        /// </response>
        /// <response code="409">
        /// البريد مفعّل بالفعل
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "البريد الإلكتروني مفعّل بالفعل"
        ///     }
        /// 
        /// </response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("resend-verification-email")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "إعادة إرسال رابط التفعيل",
            Description = "إرسال رابط تفعيل جديد للبريد الإلكتروني. لا يمكن الإرسال أكثر من مرة كل 5 دقائق",
            OperationId = "ResendVerificationEmail",
            Tags = new[] { "Authorization" }
        )]
        [SwaggerResponse(200, "تم إرسال رابط التفعيل بنجاح", typeof(object))]
        [SwaggerResponse(400, "طلب متكرر - انتظر 5 دقائق")]
        [SwaggerResponse(404, "البريد غير مسجل")]
        [SwaggerResponse(409, "البريد مفعّل بالفعل")]
        public async Task<IActionResult> ResendVerificationEmailAsync(
            [FromBody]
            [SwaggerRequestBody("البريد الإلكتروني ورابط التطبيق", Required = true)]
            ResendVerificationEmailRequestDto requestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authorization.ResendVerificationEmailAsync(requestDto.email, requestDto.Url);
            return HandleResponse(result);
        }

        /// <summary>
        /// طلب إعادة تعيين كلمة المرور (نسيت كلمة المرور)
        /// </summary>
        /// <remarks>
        /// يقوم بإرسال رمز OTP (6 أرقام) إلى البريد الإلكتروني لإعادة تعيين كلمة المرور
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authorization/ForgotPasswordAsync
        ///     Content-Type: application/json
        ///     
        ///     {
        ///       "email": "user@example.com"
        ///     }
        /// 
        /// ملاحظات:
        /// - يتم إرسال رمز OTP مكون من 6 أرقام
        /// - الرمز صالح لمدة 15 دقيقة فقط
        /// - لا يمكن طلب رمز جديد قبل مرور 2 دقيقة
        /// - بعد استلام OTP، استخدم endpoint "Check-Otp-Code"
        /// </remarks>
        /// <param name="requestDto">البريد الإلكتروني للمستخدم</param>
        /// <returns>نتيجة عملية الإرسال</returns>
        /// <response code="200">
        /// تم إرسال رمز OTP بنجاح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم إرسال رمز التحقق إلى بريدك الإلكتروني",
        ///       "data": {
        ///         "otpSent": true,
        ///         "expiresInMinutes": 15
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// بيانات غير صالحة أو محاولة متكررة
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "يرجى الانتظار دقيقتين قبل طلب رمز جديد"
        ///     }
        /// 
        /// </response>
        /// <response code="404">
        /// البريد الإلكتروني غير مسجل
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "البريد الإلكتروني غير موجود"
        ///     }
        /// 
        /// </response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("ForgotPasswordAsync")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "نسيت كلمة المرور",
            Description = "إرسال رمز OTP (6 أرقام) إلى البريد الإلكتروني لإعادة تعيين كلمة المرور. الرمز صالح لمدة 15 دقيقة",
            OperationId = "ForgotPassword",
            Tags = new[] { "Authorization" }
        )]
        [SwaggerResponse(200, "تم إرسال رمز OTP بنجاح", typeof(object))]
        [SwaggerResponse(400, "طلب متكرر - انتظر دقيقتين")]
        [SwaggerResponse(404, "البريد غير مسجل")]
        public async Task<IActionResult> ForgotPasswordAsync(
            [FromBody]
            [SwaggerRequestBody("البريد الإلكتروني للمستخدم", Required = true)]
            ForgotPasswordRequestDto requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = await _authorization.ForgotPasswordAsync(requestDto.Email);
            return HandleResponse(result);
        }

        /// <summary>
        /// التحقق من صحة رمز OTP
        /// </summary>
        /// <remarks>
        /// يتحقق من صحة رمز OTP المرسل في البريد الإلكتروني
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authorization/Check-Otp-Code
        ///     Content-Type: application/json
        ///     
        ///     {
        ///       "code": "123456"
        ///     }
        /// 
        /// ملاحظات:
        /// - الرمز مكون من 6 أرقام
        /// - صالح لمدة 15 دقيقة فقط
        /// - عند النجاح، استخدم endpoint "Reset-Password" لتغيير كلمة المرور
        /// </remarks>
        /// <param name="requestDto">رمز OTP المرسل في البريد</param>
        /// <returns>نتيجة التحقق من الرمز</returns>
        /// <response code="200">
        /// رمز OTP صحيح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "رمز التحقق صحيح",
        ///       "data": {
        ///         "isValid": true,
        ///         "resetToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// رمز OTP غير صحيح أو منتهي
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "رمز التحقق غير صحيح أو منتهي الصلاحية",
        ///       "data": {
        ///         "attemptsRemaining": 2
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="429">
        /// تجاوز عدد المحاولات المسموحة
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "تجاوزت عدد المحاولات المسموحة. يرجى طلب رمز جديد"
        ///     }
        /// 
        /// </response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("Check-Otp-Code")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "التحقق من رمز OTP",
            Description = "التحقق من صحة رمز OTP (6 أرقام) المرسل في البريد. محاولات محدودة: 3 مرات فقط",
            OperationId = "CheckOtpCode",
            Tags = new[] { "Authorization" }
        )]
        [SwaggerResponse(200, "رمز OTP صحيح - يمكنك الآن إعادة تعيين كلمة المرور", typeof(object))]
        [SwaggerResponse(400, "رمز OTP خاطئ أو منتهي")]
        [SwaggerResponse(429, "تجاوز عدد المحاولات المسموحة")]
        public async Task<IActionResult> CheckOtpCodeAsync(
            [FromBody]
            [SwaggerRequestBody("رمز OTP المكون من 6 أرقام", Required = true)]
            CheckOtpCodeRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var result = await _authorization.CheckOtpCodeAsync(requestDto.Code);
            return HandleResponse(result);
        }

        /// <summary>
        /// إعادة تعيين كلمة المرور
        /// </summary>
        /// <remarks>
        /// يقوم بتغيير كلمة المرور بعد التحقق من رمز OTP
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authorization/Reset-Password
        ///     Content-Type: application/json
        ///     
        ///     {
        ///       "email": "user@example.com",
        ///       "resetToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///       "newPassword": "NewP@ssw0rd123",
        ///       "confirmPassword": "NewP@ssw0rd123"
        ///     }
        /// 
        /// متطلبات كلمة المرور الجديدة:
        /// - 8 أحرف على الأقل
        /// - حرف كبير واحد على الأقل
        /// - حرف صغير واحد على الأقل
        /// - رقم واحد على الأقل
        /// - رمز خاص واحد على الأقل (@$!%*?&amp;)
        /// 
        /// ملاحظات:
        /// - يجب استخدام resetToken المستلم من "Check-Otp-Code"
        /// - بعد التغيير، يتم تسجيل خروج جميع الجلسات النشطة
        /// - يجب تسجيل الدخول مرة أخرى بكلمة المرور الجديدة
        /// </remarks>
        /// <param name="requestDto">بيانات إعادة تعيين كلمة المرور</param>
        /// <returns>نتيجة عملية تغيير كلمة المرور</returns>
        /// <response code="200">
        /// تم تغيير كلمة المرور بنجاح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم تغيير كلمة المرور بنجاح. يرجى تسجيل الدخول مرة أخرى",
        ///       "data": {
        ///         "passwordChanged": true,
        ///         "sessionsTerminated": 3
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// بيانات غير صالحة أو كلمة المرور لا تطابق المتطلبات
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "كلمة المرور يجب أن تحتوي على 8 أحرف على الأقل",
        ///       "errors": {
        ///         "NewPassword": ["كلمة المرور ضعيفة جداً"],
        ///         "ConfirmPassword": ["كلمة المرور غير متطابقة"]
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="401">
        /// Reset Token غير صالح أو منتهي
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "رمز إعادة التعيين غير صالح أو منتهي الصلاحية"
        ///     }
        /// 
        /// </response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("Reset-Password")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "إعادة تعيين كلمة المرور",
            Description = "تغيير كلمة المرور باستخدام Reset Token المستلم من التحقق من OTP. يتم تسجيل خروج جميع الجلسات تلقائياً",
            OperationId = "ResetPassword",
            Tags = new[] { "Authorization" }
        )]
        [SwaggerResponse(200, "تم تغيير كلمة المرور بنجاح", typeof(object))]
        [SwaggerResponse(400, "بيانات غير صالحة أو كلمة مرور ضعيفة")]
        [SwaggerResponse(401, "Reset Token غير صالح")]
        public async Task<IActionResult> ResetPasswordAsync(
            [FromBody]
            [SwaggerRequestBody("بيانات كلمة المرور الجديدة مع Reset Token", Required = true)]
            ResetPasswordRequestDto requestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authorization.ResetPasswordAsync(requestDto);
            return HandleResponse(result);
        }
    }
}