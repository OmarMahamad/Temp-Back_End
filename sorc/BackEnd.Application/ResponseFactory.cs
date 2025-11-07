using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application
{
    public static class ResponseFactory
    {
        // ✅ Generic Success (افتراضي)
        public static Response Success(string message = "Operation completed successfully")
            => Response.Success(message, StatusCodeResponeType.Success);

        // ✅ Generic Success with data
        public static Response<T> Success<T>(T data, string message = "Success")
            => Response<T>.Success(data, message, StatusCodeResponeType.Success);

        // ✅ Created
        public static Response<T> Created<T>(T data, string message = "Resource created successfully")
            => Response<T>.Success(data, message, StatusCodeResponeType.Created);

        // ✅ Updated
        public static Response<T> Updated<T>(T data, string message = "Resource updated successfully")
            => Response<T>.Success(data, message, StatusCodeResponeType.Updated);

        // ✅ Deleted
        public static Response Deleted(string message = "Resource deleted successfully")
            => Response.Success(message, StatusCodeResponeType.Deleted);

        // ⚠️ Validation error (مثلاً ModelState)
        public static Response ValidationError(IDictionary<string, string[]> errors,
                                               string message = "Validation failed")
            => Response.Failure(message, StatusCodeResponeType.ValidationError, errors);

        // ⚠️ Unauthorized / Forbidden
        public static Response Unauthorized(string message = "Unauthorized access")
            => Response.Failure(message, StatusCodeResponeType.Unauthorized);

        public static Response Forbidden(string message = "Access denied")
            => Response.Failure(message, StatusCodeResponeType.Forbidden);

        // ⚠️ Business Conflict (مثل Email موجود مسبقًا)
        public static Response Conflict(string message = "Data conflict detected")
            => Response.Failure(message, StatusCodeResponeType.DataConflict);

        // ⚠️ Resource not found
        public static Response NotFound(string message = "Resource not found")
            => Response.Failure(message, StatusCodeResponeType.NotFound);

        // ⚠️ Operation not allowed
        public static Response OperationNotAllowed(string message = "Operation not allowed")
            => Response.Failure(message, StatusCodeResponeType.OperationNotAllowed);

        // ❌ Internal or Database errors
        public static Response InternalError(string message = "Internal server error")
            => Response.Failure(message, StatusCodeResponeType.InternalServerError);

        public static Response DatabaseError(string message = "Database operation failed")
            => Response.Failure(message, StatusCodeResponeType.DatabaseError);

        // ⚠️ External / Network Errors
        public static Response NetworkFailure(string message = "Network connection failed")
            => Response.Failure(message, StatusCodeResponeType.NetworkFailure);

        public static Response ExternalServiceError(string message = "External service failure")
            => Response.Failure(message, StatusCodeResponeType.ExternalServiceError);

        // ⚙️ Locked / State Transition issues
        public static Response Locked(string message = "Entity is locked and cannot be modified")
            => Response.Failure(message, StatusCodeResponeType.Locked);

        public static Response AlreadyExists(string message = "Entity already exists")
            => Response.Failure(message, StatusCodeResponeType.AlreadyExists);
    }
}
