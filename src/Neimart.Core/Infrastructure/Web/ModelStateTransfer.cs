using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

// POST-REDIRECT-GET using TempData in ASP.NET Core
// source: https://andrewlock.net/post-redirect-get-using-tempdata-in-asp-net-core/

namespace Neimart.Core.Infrastructure.Web
{
    public class ModelStateTransferValue
    {
        public string Key { get; set; }
        public string AttemptedValue { get; set; }
        public object RawValue { get; set; }
        public ICollection<string> ErrorMessages { get; set; } = new List<string>();
    }

    public static class ModelStateHelper
    {
        public static string SerialiseModelState(ModelStateDictionary modelState)
        {
            var errorList = modelState
                .Select(kvp => new ModelStateTransferValue
                {
                    Key = kvp.Key,
                    AttemptedValue = kvp.Value.AttemptedValue,
                    RawValue = kvp.Value.RawValue,
                    ErrorMessages = kvp.Value.Errors.Select(err => err.ErrorMessage).ToList(),
                });

            return JsonConvert.SerializeObject(errorList);
        }

        public static ModelStateDictionary DeserialiseModelState(string serialisedErrorList)
        {
            var errorList = JsonConvert.DeserializeObject<List<ModelStateTransferValue>>(serialisedErrorList);
            var modelState = new ModelStateDictionary();

            foreach (var item in errorList)
            {
                var array = item.RawValue as Newtonsoft.Json.Linq.JArray;
                var value = array?.ToObject<string[]>() ?? item.RawValue;

                modelState.SetModelValue(item.Key, value, item.AttemptedValue);
                foreach (var error in item.ErrorMessages)
                {
                    modelState.AddModelError(item.Key, error);
                }
            }
            return modelState;
        }
    }

    public abstract class ModelStateTransfer : ActionFilterAttribute
    {
        protected const string Key = nameof(ModelStateTransfer);
    }

    public sealed class ModelStateAttribute : ModelStateTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (HttpMethods.IsGet(filterContext.HttpContext.Request.Method))
            {
                var controller = filterContext.Controller as Controller;
                var serialisedModelState = controller?.HttpContext.Session.GetString(Key);

                if (serialisedModelState != null)
                {
                    //Only Import if we are viewing
                    if (filterContext.Result is ViewResult)
                    {
                        var modelState = ModelStateHelper.DeserialiseModelState(serialisedModelState);
                        filterContext.ModelState.Merge(modelState);
                    }

                    controller.HttpContext.Session.Remove(Key);
                }
            }
            else if (HttpMethods.IsPost(filterContext.HttpContext.Request.Method))
            {
                //Only export when ModelState is not valid
                if (!filterContext.ModelState.IsValid)
                {
                    //Export if we are redirecting
                    if (filterContext.Result is RedirectResult
                        || filterContext.Result is RedirectToRouteResult
                        || filterContext.Result is RedirectToActionResult)
                    {
                        var controller = filterContext.Controller as Controller;
                        if (controller != null && filterContext.ModelState != null)
                        {
                            var modelState = ModelStateHelper.SerialiseModelState(filterContext.ModelState);
                            controller.HttpContext.Session.SetString(Key, modelState);
                        }
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
