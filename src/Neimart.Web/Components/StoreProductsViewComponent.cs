using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
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
    public class StoreProductsViewComponent : ViewComponent
    {
        private readonly AppService _appService;
        private readonly ProductService _productService;
        private readonly CartService _cartService;
        private readonly ReviewService _reviewService;

        public StoreProductsViewComponent(AppService appService, ProductService productService, CartService cartService, ReviewService reviewService)
        {
            _appService = appService;
            _productService = productService;
            _cartService = cartService;
            _reviewService = reviewService;
        }

        public async Task<IViewComponentResult> InvokeAsync(ProductListModel model)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            model.Filter.SellerId = seller.Id;
            model.Filter.Published = true;

            if (model.SuggestItems)
            {
                var products = await _productService.GetQuery(model.Filter).SuggestAsync(model.Page, model.PageSize);
                await _appService.PrepareModelAsync(model, products, seller, customer);
            }

            return View(model);
        }
    }
}