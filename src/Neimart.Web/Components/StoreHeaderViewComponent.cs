using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Components
{
    public class StoreHeaderViewComponent : ViewComponent
    {
        private readonly CartService _cartService;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly ReviewService _reviewService;
        private readonly OrderService _orderService;
        private readonly AddressService _addressService;
        private readonly AppService _appService;
        private readonly IOptions<AppSettings> _appsettingsAccessor;

        public StoreHeaderViewComponent(
            CartService cartService,
            ProductService productService,
            CategoryService categoryService,
            ReviewService reviewService,
            OrderService orderService,
            AddressService addressService,
            AppService appService,
            IOptions<AppSettings> appsettingsAccessor)
        {
            _cartService = cartService;
            _productService = productService;
            _categoryService = categoryService;
            _reviewService = reviewService;
            _orderService = orderService;
            _addressService = addressService;
            _appService = appService;
            _appsettingsAccessor = appsettingsAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            var carts = (await _cartService.ListAsync(new CartFilter { SellerId = seller.Id, CustomerId = customer?.Id ?? 0 }));

            // Ensure all carts are validated before evaluation.
            carts = carts.Where(x => _cartService.Validate(x).Success);

            var model = new StoreHeaderModel
            {
                CartListEvaluation = await _cartService.EvaluateAsync(carts.Where(x => x.Type == CartType.Cart)),
                WishlistEvaluation = await _cartService.EvaluateAsync(carts.Where(x => x.Type == CartType.Wishlist))
            };

            var isProductsRoute = string.Equals(Request.RouteValues["action"]?.ToString(), "Products", StringComparison.InvariantCultureIgnoreCase);
            var slug = isProductsRoute ? Request.Query["search"].ToString() : null;

            var categories = await _categoryService.ListAsync(new CategoryFilter { Published = true, SellerId = seller.Id });
            model.CategoryOptions.AddRange(SelectListHelper.GetSelectList(categories, x =>
            {
                return new SelectListItem<Category>(text: x.Name, value: x.Slug, x.Slug == slug || x.Tags.Any(tag => tag.Slug == slug));
            }, defaultText: "Products", defaultSelected: isProductsRoute && string.IsNullOrWhiteSpace(slug)));

            return View(model);
        }
    }
}
