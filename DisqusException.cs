using System;

namespace Disqus
{
    public class DisqusException : Exception
    {
        /// <summary>
        /// The Disqus error code from https://disqus.com/api/docs/errors/
        /// </summary>
        public DisqusErrorCode ErrorCode { get; set; }

        public DisqusException(int code, string message) : base(message)
        {
            ErrorCode = (DisqusErrorCode)code;
        }

        public enum DisqusErrorCode
        {
            ENDPOINT_INVALID = 1,
            MISSING_ARGS = 2,
            ENDPOINT_RESOURCE_INVALID = 3,
            AUTHENTICATION_REQUIRED = 4,
            INVALID_API_KEY = 5,
            INVALID_API_VERSION = 6,
            INVALID_VERB = 7,
            OBJECT_NOT_FOUND = 8,
            INACCESSIBLE_WITH_KEY = 9,
            OPERATION_UNSUPPORTED = 10,
            INVALID_KEY_FOR_DOMAIN = 11,
            INSUFFICIENT_APP_PRIVILEGES = 12,
            RATE_LIMIT_RESOURCE = 13,
            RATE_LIMIT_ACCOUNT = 14,
            INTERNAL_ERROR = 15,
            REQUEST_TIMEOUT = 16,
            USER_ACCESS_DENIED = 17,
            INVALID_AUTH_SIGNATURE = 18,
            RESUBMIT_CAPTCHA = 19,
            MAINTENANCE_SAVED = 20,
            MAINTENANCE_NOTSAVED = 21,
            RESOURCE_PERMISSION_DENIED = 22,
            AUTHENTICATION_VERIFICATION_REQUIRED = 23,
            EXCEEDED_CREATE_QUOTA = 24,
            ERROR_THIRD_PARTY = 25
        }
    }
}
