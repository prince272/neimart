using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Infrastructure.Web.Sitemap;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Controllers
{
    public class CompanyController : Controller
    {
        private readonly MessageService _messageService;
        private readonly UserService _userService;
        private readonly ReviewService _reviewService;
        private readonly AppService _appService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(MessageService messageService, UserService userService, ReviewService reviewService, AppService appService, IOptions<AppSettings> appSettingsAccessor, ILogger<CompanyController> logger)
        {
            _messageService = messageService;
            _userService = userService;
            _reviewService = reviewService;
            _appService = appService;
            _appSettings = appSettingsAccessor.Value;
            _logger = logger;
        }

        public Task<IActionResult> Index(int page, UserFilter filter)
        {
            return Stores(page, filter);
        }

        public IActionResult Selling()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Pricing()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet, ModelState]
        public IActionResult Contact()
        {
            return View();
        }

        [ValidateRecaptcha]
        [HttpPost, ModelState]
        public async Task<IActionResult> Contact(ContactModel model)
        {
            if (ModelState.IsValid)
            {
                await _messageService.SendEmailAsync(
                    messageRole: MessageRole.Info,
                    messageType: MessageType.CompanyContact,
                    messageDisplay: "Neimart Support",
                    email: _appSettings.Company.InfoEmail, model);

                TempData.AddAlert(AlertMode.Alert, AlertType.Success, "Your message has been sent. We'll contact you shortly.");
            }

            return RedirectToAction();
        }

        public async Task<IActionResult> Products(int page, UserFilter filter)
        {
            return View();
        }

        public async Task<IActionResult> Stores(int page, UserFilter filter)
        {
            filter.StoreAccess = StoreAccess.Approved;
            filter.StoreSetup = true;
            filter.StorePlanActive = true;

            var users = await _userService.GetQuery(filter)
                                           .ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new UserListModel
            {
                Filter = filter
            };

            await _appService.PrepareModelAsync(model, users,
                prepareChildModel: async (userModel, user) =>
                {
                    userModel.ReviewEvaluation = await _reviewService.EvaluateAsync(new ReviewFilter { SellerId = user.Id, Approved = true, });
                });

            return View(nameof(Stores), model);
        }

        public async Task<IActionResult> StoreSuggestions(string query)
        {
            var storeNames = await _userService.GetQuery(new UserFilter { StoreSetup = true, StorePlanActive = true, StoreAccess = StoreAccess.Approved, Search = query })
                                           .Take(10).Select(x => x.StoreName.ToLowerInvariant()).ToListAsync();

            return Json(storeNames);
        }

        public IActionResult ThrowError() => throw new NotImplementedException();

        // Don't mark the error handler action method with HTTP method attributes, such as HttpGet.
        // Explicit verbs prevent some requests from reaching the method.
        // UseStatusCodePagesWithReExecute Does not captures 400 error
        // source: https://github.com/dotnet/aspnetcore/issues/16634

        // Allow anonymous access to the method so that unauthenticated users are able to receive the error view.
        // source: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-3.1
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        [Route("error/{statusCode?}")]
        public IActionResult Error(int? statusCode)
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var statusReason = statusCode != null ? HttpPhrases.GetStatusReason(statusCode.Value) 
                : "Internet Connection Error";

            var statusMessage = statusCode != null ? HttpPhrases.GetStatusMessage(statusCode.Value) 
                : "Unable to establish a connection to the server. Please check your internet connection or try again later.";

            // Let's log the exception handler error.
            if (exceptionFeature != null &&
                exceptionFeature.Error != null)
            {
                _logger.LogError(exceptionFeature.Error.Message, exceptionFeature.Error);
            }


            if (HttpContext.Request.IsAjax())
            {
                return StatusCode(statusCode ?? StatusCodes.Status503ServiceUnavailable);
            }
            else
            {
                var error = new ProblemDetails
                {
                    Status = statusCode,
                    Title = statusReason,
                    Detail = $"{statusMessage} {(statusCode != null ? $"Status code: {statusCode}." : "")}".Trim(),
                };

                return View(error);
            }
        }

        [ResponseCache(Duration = 1800)]
        public IActionResult SitemapXml()
        {
            var sitemapUrlList = new SitemapUrlList
            {
                SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Index))), 1d),
                SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Pricing)))),
                SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Privacy)))),
                SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(About)))),
                SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Contact)))),
                SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Terms))))
            };

            return File(Encoding.UTF8.GetBytes(sitemapUrlList.ToXml()), WebPathHelper.GetMimeType(".xml"));
        }

        [ResponseCache(Duration = 1800)]
        public async Task<IActionResult> SitemapIndexXml()
        {
            var sitemapList = new SitemapList();
            sitemapList.Sitemaps.Add(Sitemap.CreateSitemap(Url.ContentLink(Url.Action(nameof(SitemapXml)))));

            var users = await _userService.GetQuery(new UserFilter { StoreSetup = true, StoreAccess = StoreAccess.Approved })
                                .Select(x => new { x.StoreSlug })
                                .ToListAsync();

            sitemapList.Sitemaps.AddRange(from user in users select Sitemap.CreateSitemap(Url.ContentLink(Url.Action(nameof(SitemapXml), "Store", new { storeSlug = user.StoreSlug }))));

            return File(Encoding.UTF8.GetBytes(sitemapList.ToXml()), WebPathHelper.GetMimeType(".xml"));
        }
    }
}