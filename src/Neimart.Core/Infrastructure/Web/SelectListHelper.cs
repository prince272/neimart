using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Infrastructure.Web
{
    public static class SelectListHelper
    {
        public static IEnumerable<SelectListItem<TElement>> GetSelectList<TElement>(
            IEnumerable<TElement> elements, Func<TElement, SelectListItem<TElement>> converter, string defaultText = null, string defaultValue = null, bool? defaultSelected = null)
        {
            var selectListItems = new List<SelectListItem<TElement>>();

            foreach (var element in elements)
            {
                var selectListItem = converter(element);
                selectListItem.RawValue = element;
                selectListItems.Add(selectListItem);
            }

            if (defaultText != null)
            {
                defaultText = string.IsNullOrWhiteSpace(defaultText) ?
                    $"No {typeof(TElement).Name.Humanize(LetterCasing.LowerCase)}" : defaultText;

                // TODO: The defaultValue should not be null or else the appropriate element won't be rendered.
                var defaultItem = new SelectListItem<TElement>(
                    text: defaultText,
                    value: defaultValue ?? string.Empty,
                    selected: defaultSelected ?? !selectListItems.Any(x => x.Selected));

                selectListItems.Insert(0, defaultItem);
            }

            return selectListItems;
        }


        public static IEnumerable<SelectListItem<TEnum>> GetEnumSelectList<TEnum>(IEnumerable<TEnum> enums = null,
                                                                                  TEnum? selectedEnum = default,
                                                                                  Func<TEnum, string> getText = null,
                                                                                  Func<TEnum, string> getValue = null,
                                                                                  string defaultText = null,
                                                                                  string defaultValue = null)
            where TEnum : struct, Enum
        {
            var enumType = Nullable.GetUnderlyingType(typeof(TEnum)) ?? typeof(TEnum);

            if (!enumType.IsEnum)
                throw new NotSupportedException($"{enumType} must be an enumerated type.");

            var hasEnumFlags = AttributeHelper.GetTypeAttribute<FlagsAttribute>(enumType) != null;

            enums ??= Enum.GetValues(enumType).Cast<TEnum>();

            return GetSelectList(enums, (enumValue) =>
            {
                var text = getText != null ? getText(enumValue) : enumValue.GetEnumText();
                var value = getValue != null ? getValue(enumValue) : enumValue.ToString().Camelize();
                var selected = (selectedEnum != null && (hasEnumFlags ? (selectedEnum as Enum).HasFlag(enumValue) : Enum.Equals(selectedEnum, enumValue)));
               
                return new SelectListItem<TEnum>(text: text, value: value, selected: selected);
            }, defaultText, defaultValue, null);
        }

        public static IEnumerable<SelectListItem<bool>> GetBoolSelectList(string trueText,
                                                                          string falseText,
                                                                          bool? selectedBool = null,
                                                                          string defaultText = null,
                                                                          string defaultValue = null)
        {
            var boolValues = new[] { true, false };

            return GetSelectList(boolValues, (boolValue) =>
            {
                var text = boolValue ? trueText : falseText;
                var value = boolValue.ToString().Camelize();
                var selected = boolValue.Equals(selectedBool);
                return new SelectListItem<bool>(text: text, value: value, selected: selected);
            }, defaultText, defaultValue, null);
        }
    }

    public class SelectListItem<T> : SelectListItem
    {
        public SelectListItem(string text, string value) : base(text, value)
        {
        }

        public SelectListItem(string text, string value, bool selected) : base(text, value, selected)
        {
        }

        public SelectListItem(string text, string value, bool selected, bool disabled) : base(text, value, selected, disabled)
        {
        }

        public T RawValue { get; set; }
    }
}