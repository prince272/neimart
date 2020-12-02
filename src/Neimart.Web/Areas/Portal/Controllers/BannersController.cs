using System;
using System.Collections.Generic;
using System.IO;
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
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Storing;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities;
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Areas.Portal.Controllers
{
    [Area("Portal")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Seller)]
    [PlanRequired]
    public class BannersController : Controller
    {
        private readonly AppService _appService;
        private readonly BannerService _bannerService;
        private readonly MediaService _mediaService;
        private readonly AppSettings _appSettings;

        public BannersController(IServiceProvider services)
        {
            _appService = services.GetRequiredService<AppService>();
            _bannerService = services.GetRequiredService<BannerService>();
            _mediaService = services.GetRequiredService<MediaService>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page, BannerFilter filter)
        {
            var seller = await HttpContext.GetMemberAsync();

            filter.SellerId = seller.Id;

            var banners = await _bannerService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new BannerListModel
            {
                Filter = filter
            };

            await _appService.PrepareModelAsync(model, banners);

            return View(model);
        }

        [HttpGet, ModelState]
        public async Task<IActionResult> Add(BannerSize size)
        {
            var seller = await HttpContext.GetMemberAsync();
            var model = new BannerEditModel();
            await _appService.PrepareModelAsync(model, null);

            model.Size = size;
            return View(nameof(Edit), model);
        }

        [HttpPost, ModelState]
        public async Task<IActionResult> Add(BannerEditModel model, BannerSize size)
        {
            if (ModelState.IsValid)
            {
                var seller = await HttpContext.GetMemberAsync();
                var banner = new Banner();
                await _appService.PrepareBannerAsync(banner, model);

                banner.SellerId = seller.Id;
                await _bannerService.CreateAsync(banner);
                await SaveBannerImage(banner, model);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{banner.Title}\" banner was added.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet, ModelState]
        public async Task<IActionResult> Edit(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var banner = await _bannerService.GetAsync(new BannerFilter() { SellerId = seller.Id, BannerId = id });

            if (banner != null)
            {
                var model = new BannerEditModel();

                await _appService.PrepareModelAsync(model, banner);
                return View(nameof(Edit), model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ModelState]
        public async Task<IActionResult> Edit(BannerEditModel model)
        {
            if (ModelState.IsValid)
            {
                var seller = await HttpContext.GetMemberAsync();
                var banner = await _bannerService.GetAsync(new BannerFilter() { SellerId = seller.Id, BannerId = model.Id });

                if (banner != null)
                {
                    await _appService.PrepareBannerAsync(banner, model);

                    banner.SellerId = seller.Id;
                    await _bannerService.UpdateAsync(banner);
                    await SaveBannerImage(banner, model);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{banner.Title}\" banner was updated.");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var banner = await _bannerService.GetAsync(new BannerFilter() { SellerId = seller.Id, BannerId = id });

            if (banner != null)
            {
                await _bannerService.DeleteAsync(banner);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{banner.Title}\" banner was deleted.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Banner does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Publish(long id, bool toggle)
        {
            var seller = await HttpContext.GetMemberAsync();
            var banner = await _bannerService.GetAsync(new BannerFilter() { SellerId = seller.Id, BannerId = id });
            var toggleName = toggle ? "Published" : "Unpublished";

            if (banner != null)
            {
                banner.Published = toggle;
                await _bannerService.UpdateAsync(banner);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{banner.Title}\" banner was {toggleName.ToLowerInvariant()}.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Banner does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }


        [NonAction]
        private async Task SaveBannerImage(Banner banner, BannerEditModel model)
        {
            var fileClient = HttpContext.RequestServices.GetRequiredService<IFileClient>();
            var imageProcessor = HttpContext.RequestServices.GetRequiredService<IImageProcessor>();

            await _appService.PrepareMediaAsync(new[] { banner.Image }, new[] { model.Image },
                createMedia: async (image) =>
                {
                    image.DirectoryName = banner.SellerId.ToString();

                    var source = await fileClient.GetAsync(image.DirectoryName, image.FileName);

                    if (source != null)
                    {
                        var (imageWidth, imageHeight) = await imageProcessor.GetImageSizeAsync(source);

                        image.Width = imageWidth;
                        image.Height = imageHeight;
                        image.FileSize = source.Length;

                        await _mediaService.CreateAsync(image);

                        banner.ImageId = image.Id;
                        await _bannerService.UpdateAsync(banner);
                    }
                },
                updateMedia: async (image) =>
                {
                    await _mediaService.UpdateAsync(image);
                },
                deleteMedia: async (image) =>
                {
                    banner.ImageId = null;
                    banner.Image = null;

                    await _bannerService.UpdateAsync(banner);
                    await _mediaService.DeleteAsync(image);
                });
        }
    }
}