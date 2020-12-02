using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Utilities;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Components
{
    public class StoreCategoriesViewComponent : ViewComponent
    {
        private readonly AppService _appService;
        private readonly CategoryService _categoryService;

        public StoreCategoriesViewComponent(AppService appService, CategoryService categoryService)
        {
            _appService = appService;
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync(CategoryListModel model)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            model.Filter.SellerId = seller.Id;
            model.Filter.Published = true;

            if (model.SuggestItems)
            {
                var categories = await _categoryService.GetQuery(model.Filter).SuggestAsync(model.Page, model.PageSize);
                await _appService.PrepareModelAsync(model, categories);
            }

            return View(model);
        }
    }
}
