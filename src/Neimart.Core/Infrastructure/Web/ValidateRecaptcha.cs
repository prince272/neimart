using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using reCAPTCHA.AspNetCore;

namespace Neimart.Core.Infrastructure.Web
{
    public class ValidateRecaptchaFilter : IAsyncActionFilter
    {
        private readonly RecaptchaService _recaptcha;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly double _minimumScore;
        private readonly string _errorMessage;

        public ValidateRecaptchaFilter(IServiceProvider services, double minimumScore, string errorMessage)
        {
            _recaptcha = services.GetRequiredService<RecaptchaService>();
            _webHostEnvironment = services.GetRequiredService<IWebHostEnvironment>();
            _minimumScore = minimumScore;
            _errorMessage = errorMessage;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!_webHostEnvironment.IsDevelopment())
            {
                if (HttpMethods.IsPost(context.HttpContext.Request.Method))
                {
                    var recaptcha = await _recaptcha.Validate(context.HttpContext.Request);
                    if (!recaptcha.success || recaptcha.score != 0 && recaptcha.score < _minimumScore)
                        context.ModelState.AddModelError(string.Empty, _errorMessage);
                }
            }

            await next();
        }
    }

    /// <summary>
    /// Validates Recaptcha submitted by a form using: @Html.Recaptcha(RecaptchaSettings.Value)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ValidateRecaptchaAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => true;
        private readonly double _minimumScore;
        private readonly string _errorMessage;


        /// <summary>
        /// Validates Recaptcha submitted by a form using: @Html.Recaptcha(RecaptchaSettings.Value)
        /// </summary>
        /// <param name="score">The minimum score you wish to be acceptable for a success.</param>
        /// <param name="errorMessage">Error message you want added to validation model.</param>
        public ValidateRecaptchaAttribute(double score = 0, string errorMessage = "There was an error validating your responses. Please try again, or contact support if the problem persists.")
        {
            _minimumScore = score;
            _errorMessage = errorMessage;
        }

        public IFilterMetadata CreateInstance(IServiceProvider services)
        {
            return new ValidateRecaptchaFilter(services, _minimumScore, _errorMessage);
        }
    }
}