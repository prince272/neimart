using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Neimart.Core.Infrastructure.Web
{
    // A Humanizer Display Metadata Provider for ASP .Net Core
    // source: https://www.michael-whelan.net/using-humanizer-with-asp-dotnet-core/
    public class HumanizerMetadataProvider : IDisplayMetadataProvider
    {
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            if (IsTransformRequired(propertyName, modelMetadata, propertyAttributes))
            {
                modelMetadata.DisplayName = () => propertyName.Humanize(LetterCasing.Sentence);
            }
        }

        private static bool IsTransformRequired(string propertyName, DisplayMetadata modelMetadata, IReadOnlyList<object> propertyAttributes)
        {
            if (!string.IsNullOrWhiteSpace(modelMetadata.SimpleDisplayProperty))
                return false;

            if (propertyAttributes.OfType<DisplayNameAttribute>().Any())
                return false;

            if (propertyAttributes.OfType<DisplayAttribute>().Any())
            {
                var displayName = modelMetadata.DisplayName?.Invoke();
                return string.IsNullOrWhiteSpace(displayName);
            }

            if (string.IsNullOrWhiteSpace(propertyName))
                return false;

            return true;
        }
    }
}
