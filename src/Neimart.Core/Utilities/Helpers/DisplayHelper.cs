using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Humanizer;
using Microsoft.AspNetCore.Mvc;

namespace Neimart.Core.Utilities.Helpers
{
    public static class DisplayHelper
    {
        public static DisplayAttribute GetDisplayByMember<TObject>(TObject obj, Expression<Func<TObject, object>> action)
        {
            var displayAttribute = AttributeHelper.GetMemberAttribute<TObject, DisplayAttribute>(action);
            return displayAttribute;
        }

        public static string GetDisplayText<TObject>(TObject obj, Expression<Func<TObject, object>> action)
        {
            var displayAttribute = AttributeHelper.GetMemberAttribute<TObject, DisplayAttribute>(action);
            return displayAttribute?.Name ?? ((MemberExpression)action.Body).Member.Name.Humanize(LetterCasing.Sentence);
        }
    }
}