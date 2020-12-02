using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Areas.Portal.Controllers
{
    [Area("Portal")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Seller)]
    [PlanRequired]
    public class ReviewsController : Controller
    {
        private readonly AppService _appService;
        private readonly ReviewService _reviewService;
        private readonly AppSettings _appSettings;

        public ReviewsController(IServiceProvider services)
        {
            _appService = services.GetRequiredService<AppService>();
            _reviewService = services.GetRequiredService<ReviewService>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
        }

        public async Task<IActionResult> Index(ReviewFilter filter, int page)
        {
            var seller = await HttpContext.GetMemberAsync();

            filter.SellerId = seller.Id;

            var reviews = await _reviewService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new ReviewListModel
            {
                Filter = filter
            };

            await _appService.PrepareModelAsync(model, reviews);

            return View(model);
        }

        [HttpGet, ModelState]
        public async Task<IActionResult> Edit(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var review = await _reviewService.GetAsync(new ReviewFilter() { SellerId = seller.Id, ReviewId = id });

            if (review != null)
            {
                var model = new ReviewEditModel();

                await _appService.PrepareModelAsync(model, review);
                return View(nameof(Edit), model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ModelState]
        public async Task<IActionResult> Edit(ReviewEditModel model)
        {
            if (ModelState.IsValid)
            {
                var seller = await HttpContext.GetMemberAsync();
                var review = await _reviewService.GetAsync(new ReviewFilter() { SellerId = seller.Id, ReviewId = model.Id });

                if (review != null)
                {
                    await _appService.PrepareReviewAsync(review, model);
                    await _reviewService.UpdateAsync(review);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{review.Title}\" review was updated.");
                }
            }

            return RedirectToAction();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var review = await _reviewService.GetAsync(new ReviewFilter() { SellerId = seller.Id, ReviewId = id });

            if (review != null)
            {
                await _reviewService.DeleteAsync(review);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{review.Title}\" review was deleted.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Review does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Approve(long id, bool toggle)
        {
            var seller = await HttpContext.GetMemberAsync();
            var review = await _reviewService.GetAsync(new ReviewFilter() { SellerId = seller.Id, ReviewId = id });
            var toggleName = toggle ? "Approved" : "Rejected";

            if (review != null)
            {
                review.Approved = toggle;
                await _reviewService.UpdateAsync(review);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{review.Title}\" review was {toggleName.ToLowerInvariant()}.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Review does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}