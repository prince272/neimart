using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core.Utilities.Helpers
{
    // Regular Expression Library
    // source: http://regexlib.com/Default.aspx
    public static class ValidationHelper
    {
        public static string PhoneRegexPattern = @"^\+[0-9]{12,15}$";

        public static string EmailRegexPattern = @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$";

        public static string PasswordRegexPattern = @".{6,}";

        public static string UrlRegexPattern = @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$";

        public static string SlugRegexPattern = @"^[a-z\d](?:[a-z\d-]*[a-z\d])?$";

        public static string PhoneRegexMessage = "The '{PropertyName}' is not in the correct format.";

        public static string EmailRegexMessage = "The '{PropertyName}' must be in the correct format. eg: someone@example.com";

        public static string PasswordRegexMessage = "The '{PropertyName}' must contain at least 8 alphanumeric characters and any special characters.";

        public static string UrlRegexMessage = "The '{PropertyName}' must be in the correct format. eg: www.example.com";

        public static string SlugRegexMessage = "The '{PropertyName}' must be in the correct format. eg: the-quick-brown-fox";
    }
}
