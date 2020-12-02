using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Infrastructure.Web
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, string> Name<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            var options = ruleBuilder
                .MaximumLength(255)
                // Most efficient regex for checking if a string contains at least 3 alphanumeric characters
                // source: https://stackoverflow.com/questions/30723346/most-efficient-regex-for-checking-if-a-string-contains-at-least-3-alphanumeric-c
                .Matches(@"^(?:[^a-zA-Z]*[a-zA-Z]){3}.*") // code was modified.
                .WithMessage("'{PropertyName}' must contain at least 3 alpha characters.");

            return options;
        }

        public static IRuleBuilderOptions<T, string> Text<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            var options = ruleBuilder
                .MaximumLength(255);

            return options;
        }

        public static IRuleBuilderOptions<T, string> Email<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            var options = ruleBuilder
                .Matches(ValidationHelper.EmailRegexPattern)
                .WithMessage(ValidationHelper.EmailRegexMessage);

            return options;
        }

        public static IRuleBuilderOptions<T, string> Phone<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            var options = ruleBuilder
                .Matches(ValidationHelper.PhoneRegexPattern)
                .WithMessage(ValidationHelper.PhoneRegexMessage);

            return options;
        }

        public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            var options = ruleBuilder
                .Matches(ValidationHelper.PasswordRegexPattern)
                .WithMessage(ValidationHelper.PasswordRegexMessage);

            return options;
        }

        public static IRuleBuilderOptions<T, string> Url<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            var options = ruleBuilder
                .Matches(ValidationHelper.UrlRegexPattern)
                .WithMessage(ValidationHelper.UrlRegexMessage);

            return options;
        }

        public static IRuleBuilderOptions<T, string> Slug<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            var options = ruleBuilder
                .MinimumLength(3)
                .MaximumLength(255)
                .Must(x => string.IsNullOrWhiteSpace(x) || !string.IsNullOrWhiteSpace(SanitizerHelper.GenerateSlug(x)))
                .WithMessage(ValidationHelper.SlugRegexMessage);

            return options;
        }
    }
}