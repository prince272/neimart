using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Utilities;
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Components
{
    public class StoreBannersViewComponent : ViewComponent
    {
        private readonly AppService _appService;
        private readonly BannerService _bannerService;

        public StoreBannersViewComponent(AppService appService, BannerService bannerService)
        {
            _appService = appService;
            _bannerService = bannerService;
        }

        public async Task<IViewComponentResult> InvokeAsync(BannerListModel model)
        {
            var seller = await HttpContext.GetSellerAsync();
            model.Filter.SellerId = seller.Id;
            model.Filter.Published = true;

            if (model.SuggestItems)
            {
                var banners = await _bannerService.GetQuery(model.Filter)
                                                  .SuggestAsync(model.Page, model.PageSize);
                await _appService.PrepareModelAsync(model, banners);
            }

            return View(model);
        }
    }
}
