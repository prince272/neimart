using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Neimart.Core;

namespace Neimart.Core.Infrastructure.Web
{
    public static class ModelStateExtensions
    {
        public static void AddIdentityResult(this ModelStateDictionary modelState, IdentityResult result)
        {
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    string errorKey = error.Code;
                    string errorDescription = error.Description;


                    switch (error.Code)
                    {
                        case "DuplicateEmail":
                            errorKey = "Email";
                            errorDescription = $"The email is already taken. It's possible you've already registered an account with us.";
                            break;

                        case "DuplicateStoreSlug":
                            errorKey = "StoreSlug";
                            errorDescription = $"The store slug is already taken.";
                            break;

                        case "InvalidEmail":
                            errorKey = "Email";
                            break;

                        case "DuplicateName":
                        case "InvalidUserName":
                            errorKey = "UserName";
                            break;

                        case "PasswordMismatch":
                        case "PasswordRequireDigit":
                        case "PasswordRequireLower":
                        case "PasswordRequireNonLetterOrDigit":
                        case "PasswordRequireUpper":
                        case "PasswordTooShort":
                            errorKey = "Password";
                            break;
                    }

                    modelState.AddModelError(errorKey, errorDescription);
                }
            }
        }

        public static void AddResult(this ModelStateDictionary modelState, Result result)
        {
            modelState.AddModelError(string.Empty, result.Message);
        }
    }
}
