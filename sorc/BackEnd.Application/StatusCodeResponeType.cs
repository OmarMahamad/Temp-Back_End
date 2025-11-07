using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application
{
    /// <summary>
    /// رموز الاستجابة الموحدة المستخدمة داخل الطبقات (Application/Domain).
    /// يتم ترجمتها إلى HTTP Codes عند الإرسال عبر API.
    /// </summary>
    public enum StatusCodeResponeType
    {
        #region Success & Info (1xx)
        Success = 100,
        Created = 101,
        Updated = 102,
        Deleted = 103,
        NoContent = 104,
        #endregion

        #region Logical Warnings (2xx)
        ValidationError = 200,
        DuplicateRecord = 201,
        DataConflict = 202,
        OperationNotAllowed = 203,
        PartialSuccess = 204,
        #endregion

        #region Auth / Security (3xx)
        Unauthorized = 300,
        Forbidden = 301,
        TokenExpired = 302,
        InvalidToken = 303,
        AccessDenied = 304,
        SessionExpired = 305,
        #endregion

        #region Client Errors (4xx)
        BadRequest = 400,
        NotFound = 401,
        InvalidParameters = 402,
        MissingRequiredField = 403,
        UnsupportedOperation = 404,
        ModelBindingError = 405,
        #endregion

        #region Server Errors (5xx)
        InternalServerError = 500,
        DatabaseError = 501,
        ExternalServiceError = 502,
        NetworkFailure = 503,
        UnknownError = 504,
        #endregion

        #region Data / State Control (6xx)
        NotModified = 600,
        AlreadyExists = 601,
        InUse = 602,
        Locked = 603,
        StateTransitionNotAllowed = 604
        #endregion
    }

}
