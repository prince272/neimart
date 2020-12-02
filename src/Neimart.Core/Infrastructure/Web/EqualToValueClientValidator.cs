using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Resources;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Neimart.Core.Infrastructure.Web
{
    // Client side validation for bool value #1314
    // source: https://github.com/FluentValidation/FluentValidation/issues/1314
    public class EqualToValueClientValidator : FluentValidation.AspNetCore.ClientValidatorBase
    {
        private readonly IClientModelValidator _originalEqualValidator;

        public EqualToValueClientValidator(PropertyRule rule, IPropertyValidator validator, IClientModelValidator originalEqualValidator) : base(rule, validator)
        {
            _originalEqualValidator = originalEqualValidator;
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            var equalValidator = (EqualValidator)this.Validator;

            // Only want to generate clientside attributes if we're comparing with a boolean.
            if (equalValidator.ValueToCompare is bool comparisonValue)
            {
                MergeAttribute(context.Attributes, "data-val", "true");
                MergeAttribute(context.Attributes, "data-val-equaltovalue", GetErrorMessage(equalValidator, context));
                MergeAttribute(context.Attributes, "data-val-equaltovalue-value", comparisonValue.ToString().ToLowerInvariant());
            }

            // Delegate the remaining tasks to the original equal to client validator.
            _originalEqualValidator.AddValidation(context);
        }

        private string GetErrorMessage(EqualValidator equalValidator, ClientModelValidationContext context)
        {
            var formatter = FluentValidation.ValidatorOptions.MessageFormatterFactory()
                .AppendPropertyName(Rule.GetDisplayName())
                .AppendArgument("ComparisonValue", equalValidator.ValueToCompare);

            string messageTemplate;
            try
            {
                messageTemplate = equalValidator.Options.ErrorMessageSource.GetString(null);
            }
            catch (FluentValidationMessageFormatException)
            {
                messageTemplate = FluentValidation.ValidatorOptions.LanguageManager.GetStringForValidator<EqualValidator>();
            }

            string message = formatter.BuildMessage(messageTemplate);
            return message;
        }
    }
}
