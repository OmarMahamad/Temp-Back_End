using Asp.Versioning;
using BackEnd.Application.ApplicationServices.Autho;
using BackEnd.Application.DTOs.AuthoDtos.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackEnd.Api.Controllers.Virsion.v1
{
    /// <summary>
    /// واجهة برمجية للمصادقة وإدارة الجلسات
    /// </summary>
    /// <remarks>
    /// توفر هذه الواجهة endpoints للتحكم في عمليات تسجيل الخروج، التحقق من الرموز، وتحديث رموز الوصول
    /// </remarks>
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

        /// <summary>
        /// تسجيل خروج المستخدم من الجلسة الحالية
        /// </summary>
        /// <remarks>
        /// يقوم بإنهاء الجلسة الحالية للمستخدم باستخدام Refresh Token
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authentication/Logout
        ///     Content-Type: multipart/form-data
        ///     Authorization: Bearer {your_access_token}
        ///     
        ///     RefreshToken: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
        /// 
        /// </remarks>
        /// <param name="requestDto">بيانات تسجيل الخروج تحتوي على Refresh Token</param>
        /// <returns>نتيجة عملية تسجيل الخروج</returns>
        /// <response code="200">
        /// تم تسجيل الخروج بنجاح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم تسجيل الخروج بنجاح",
        ///       "data": null
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// بيانات الإدخال غير صالحة أو Refresh Token غير صحيح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "البيانات المدخلة غير صحيحة",
        ///       "errors": {
        ///         "RefreshToken": ["حقل RefreshToken مطلوب"]
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="401">المستخدم غير مصرح له بالوصول - Access Token منتهي أو غير صالح</response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("Logout")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "تسجيل الخروج من الجلسة الحالية",
            Description = "إنهاء الجلسة الحالية للمستخدم باستخدام Refresh Token. يجب إرسال RefreshToken كـ form-data.",
            OperationId = "Logout",
            Tags = new[] { "Authentication" }
        )]
        [SwaggerResponse(200, "تم تسجيل الخروج بنجاح", typeof(object))]
        [SwaggerResponse(400, "بيانات غير صالحة")]
        [SwaggerResponse(401, "غير مصرح")]
        public async Task<IActionResult> LogoutAsync([FromForm] LogoutRequestDto requestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authentication.LogoutAsync(requestDto.RefreshToken);
            return HandleResponse(result);
        }

        /// <summary>
        /// تسجيل خروج المستخدم من جميع الجلسات النشطة
        /// </summary>
        /// <remarks>
        /// يقوم بإنهاء جميع الجلسات النشطة للمستخدم على كافة الأجهزة
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authentication/Logout-FromAllSessions/123e4567-e89b-12d3-a456-426614174000
        ///     Authorization: Bearer {your_access_token}
        /// 
        /// Use cases:
        /// - المستخدم يشتبه في اختراق حسابه
        /// - المستخدم يريد تسجيل الخروج من جميع الأجهزة
        /// - تم تغيير كلمة المرور
        /// </remarks>
        /// <param name="userid">معرف المستخدم الفريد (GUID)</param>
        /// <returns>نتيجة عملية تسجيل الخروج من جميع الجلسات</returns>
        /// <response code="200">
        /// تم تسجيل الخروج من جميع الجلسات بنجاح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم تسجيل الخروج من جميع الجلسات بنجاح",
        ///       "data": {
        ///         "sessionsTerminated": 5
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// معرف المستخدم غير صالح (GUID خاطئ أو فارغ)
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "Invalid input"
        ///     }
        /// 
        /// </response>
        /// <response code="401">المستخدم غير مصرح له بالوصول</response>
        /// <response code="404">المستخدم غير موجود</response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("Logout-FromAllSessions/{userid:guid}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "تسجيل الخروج من جميع الجلسات",
            Description = "إنهاء جميع الجلسات النشطة للمستخدم على كافة الأجهزة. مفيد في حالات الأمان أو تغيير كلمة المرور.",
            OperationId = "LogoutFromAllSessions",
            Tags = new[] { "Authentication" }
        )]
        [SwaggerResponse(200, "تم تسجيل الخروج من جميع الجلسات بنجاح", typeof(object))]
        [SwaggerResponse(400, "GUID غير صالح")]
        [SwaggerResponse(404, "المستخدم غير موجود")]
        public async Task<IActionResult> LogoutFromAllSessions(
            [FromRoute]
            [SwaggerParameter("معرف المستخدم بصيغة GUID", Required = true)]
            Guid userid)
        {
            if (userid == Guid.Empty)
                return BadRequest("Invalid input");
            var result = await _authentication.LogoutFromAllSessions(userid);
            return HandleResponse(result);
        }

        /// <summary>
        /// التحقق من صلاحية Access Token
        /// </summary>
        /// <remarks>
        /// يتحقق من صلاحية وسلامة Access Token المقدم
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authentication/ValidateToken
        ///     Content-Type: application/json
        ///     Authorization: Bearer {your_access_token}
        ///     
        ///     {
        ///       "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
        ///     }
        /// 
        /// يتحقق من:
        /// - Token لم ينته صلاحيته
        /// - Token لم يتم التلاعب به
        /// - الجلسة ما زالت نشطة
        /// </remarks>
        /// <param name="requestDto">بيانات الطلب تحتوي على Token المراد التحقق منه</param>
        /// <returns>نتيجة التحقق من صلاحية Token مع تفاصيل إضافية</returns>
        /// <response code="200">
        /// تم التحقق بنجاح (سواء كان Token صالح أو غير صالح)
        /// 
        /// Example response (Valid Token):
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "الرمز صالح",
        ///       "data": {
        ///         "isValid": true,
        ///         "userId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "expiresAt": "2025-11-09T15:30:00Z"
        ///       }
        ///     }
        /// 
        /// Example response (Invalid Token):
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "الرمز منتهي الصلاحية أو غير صالح",
        ///       "data": {
        ///         "isValid": false
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="400">بيانات الإدخال غير صالحة أو Token مفقود</response>
        /// <response code="401">المستخدم غير مصرح له بالوصول</response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("ValidateToken")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "التحقق من صلاحية Token",
            Description = "التحقق من أن Access Token صالح ولم ينته ولم يتم التلاعب به",
            OperationId = "ValidateToken",
            Tags = new[] { "Authentication" }
        )]
        [SwaggerResponse(200, "نتيجة التحقق", typeof(object))]
        [SwaggerResponse(400, "Token مفقود أو غير صالح")]
        public async Task<IActionResult> ValidateTokenAsync(
            [FromBody]
            [SwaggerRequestBody("بيانات Token للتحقق منه", Required = true)]
            ValidateTokenRequestDto requestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authentication.ValidateTokenAsync(requestDto.token);
            return HandleResponse(result);
        }

        /// <summary>
        /// تحديث Access Token باستخدام Refresh Token
        /// </summary>
        /// <remarks>
        /// يقوم بإصدار Access Token جديد باستخدام Refresh Token صالح
        /// 
        /// Sample request:
        /// 
        ///     POST /api/v1/Authentication/Refresh-AccessToken
        ///     Content-Type: application/json
        ///     Authorization: Bearer {your_access_token}
        ///     
        ///     {
        ///       "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
        ///     }
        /// 
        /// Use cases:
        /// - Access Token الحالي انتهى
        /// - حصلت على خطأ 401 Unauthorized
        /// - تحديث استباقي قبل انتهاء الصلاحية
        /// 
        /// Important notes:
        /// - Refresh Token صالح لمدة أطول (7-30 يوم)
        /// - احفظ Refresh Token بشكل آمن
        /// - لا ترسل Refresh Token في URLs
        /// </remarks>
        /// <param name="requestDto">بيانات الطلب تحتوي على Refresh Token</param>
        /// <returns>Access Token جديد مع تفاصيل الصلاحية</returns>
        /// <response code="200">
        /// تم تحديث Access Token بنجاح
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "تم تحديث رمز الوصول بنجاح",
        ///       "data": {
        ///         "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.NEW_TOKEN_HERE",
        ///         "expiresIn": 3600,
        ///         "tokenType": "Bearer"
        ///       }
        ///     }
        /// 
        /// </response>
        /// <response code="400">
        /// Refresh Token غير صالح، منتهي الصلاحية، أو تم استخدامه مسبقاً
        /// 
        /// Example response:
        /// 
        ///     {
        ///       "success": false,
        ///       "message": "رمز التحديث غير صالح أو منتهي الصلاحية",
        ///       "data": null
        ///     }
        /// 
        /// </response>
        /// <response code="401">المستخدم غير مصرح له بالوصول</response>
        /// <response code="500">خطأ داخلي في الخادم</response>
        [HttpPost("Refresh-AccessToken")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "تحديث Access Token",
            Description = "الحصول على Access Token جديد باستخدام Refresh Token صالح. استخدمه عند انتهاء صلاحية Token الحالي.",
            OperationId = "RefreshAccessToken",
            Tags = new[] { "Authentication" }
        )]
        [SwaggerResponse(200, "تم التحديث بنجاح - يحتوي على Access Token الجديد", typeof(object))]
        [SwaggerResponse(400, "Refresh Token غير صالح أو منتهي")]
        public async Task<IActionResult> RefreshAccessTokenAsync(
            [FromBody]
            [SwaggerRequestBody("بيانات Refresh Token", Required = true)]
            RefreshAccessTokenRequestDto requestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authentication.RefreshAccessTokenAsync(requestDto.RefreshToken);
            return HandleResponse(result);
        }
    }
}