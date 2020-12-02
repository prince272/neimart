using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Neimart.Core;
using Neimart.Core.Utilities.Extensions;

namespace Neimart.Core.Infrastructure.Web
{
    public class Alert
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public AlertMode Mode { get; set; }

        public AlertType Type { get; set; }

        public string ReturnText { get; set; }

        public string ReturnUrl { get; set; }

        public string CancelText { get; set; }

        public string CancelUrl { get; set; }
    }

    public static class DictionaryExtensions
    {
        private const string AlertDataKey = "Neimart.Alerts";

        public static void AddAlert(this IDictionary<string, object> data, AlertMode mode, AlertType type,
                                    string message, string title = null, string returnUrl = null,
                                    string returnText = null, string cancelUrl = null, string cancelText = null)
        {
            var alerts = data.TryGetValue(AlertDataKey, out object alertsJson) ? alertsJson.ToString().ToJsonObject<List<Alert>>() : new List<Alert>();

            alerts.Add(new Alert()
            {
                Mode = mode,
                Type = type,
                Message = message,
                Title = title,
                ReturnUrl = returnUrl,
                ReturnText = returnText,
                CancelUrl = cancelUrl,
                CancelText = cancelText
            });

            data[AlertDataKey] = alerts.ToJsonString();
        }

        public static void AddResultAlert(this IDictionary<string, object> data, AlertMode mode, Result result, string returnUrl = null,
                                    string returnText = null, string cancelUrl = null, string cancelText = null)
        {
            if (result.Success)
                data.AddAlert(mode, AlertType.Success, result.Message, null, returnUrl, returnText, cancelUrl, cancelText);
            else
                data.AddAlert(mode, AlertType.Error, result.Message);
        }

        public static IEnumerable<Alert> GetAlerts(this IDictionary<string, object> data)
        {
            var alerts = data.TryGetValue(AlertDataKey, out object alertsJson) ? alertsJson.ToString().ToJsonObject<List<Alert>>() : new List<Alert>();
            return alerts.ToList();
        }
    }

    public enum AlertType
    {
        [Display(Name = "Information")]
        Info,

        [Display(Name = "Warning")]
        Warning,

        [Display(Name = "Error")]
        Error,

        [Display(Name = "Success")]
        Success,
    }

    public enum AlertMode
    {
        Alert,
        Notify,
        Confirm
    }
}
