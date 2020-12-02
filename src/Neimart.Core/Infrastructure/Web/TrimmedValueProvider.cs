using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Neimart.Core.Infrastructure.Web
{
    // Best way to trim strings after data entry. Should I create a custom model binder?
    // source: https://stackoverflow.com/questions/1718501/best-way-to-trim-strings-after-data-entry-should-i-create-a-custom-model-binder/59313908#59313908
    public class TrimmedFormValueProvider
        : FormValueProvider
    {
        public TrimmedFormValueProvider(IFormCollection values)
            : base(BindingSource.Form, values, CultureInfo.InvariantCulture)
        { }

        public override ValueProviderResult GetValue(string key)
        {
            ValueProviderResult baseResult = base.GetValue(key);
            string[] trimmedValues = baseResult.Values.Select(v => v?.Trim()).ToArray();
            return new ValueProviderResult(new StringValues(trimmedValues));
        }
    }

    public class TrimmedQueryStringValueProvider
        : QueryStringValueProvider
    {
        public TrimmedQueryStringValueProvider(IQueryCollection values)
            : base(BindingSource.Query, values, CultureInfo.InvariantCulture)
        { }

        public override ValueProviderResult GetValue(string key)
        {
            ValueProviderResult baseResult = base.GetValue(key);
            string[] trimmedValues = baseResult.Values.Select(v => v?.Trim()).ToArray();
            return new ValueProviderResult(new StringValues(trimmedValues));
        }
    }

    public class TrimmedFormValueProviderFactory
        : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            if (context.ActionContext.HttpContext.Request.HasFormContentType)
                context.ValueProviders.Add(new TrimmedFormValueProvider(context.ActionContext.HttpContext.Request.Form));
            return Task.CompletedTask;
        }
    }

    public class TrimmedQueryStringValueProviderFactory
        : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            context.ValueProviders.Add(new TrimmedQueryStringValueProvider(context.ActionContext.HttpContext.Request.Query));
            return Task.CompletedTask;
        }
    }
}
