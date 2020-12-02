using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Web.Models;

namespace Neimart.Web.Areas.Portal.Controllers
{
    [Area("Portal")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Seller)]
    [PlanRequired]
    public class HomeController : Controller
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly BannerService _bannerService;
        private readonly ReviewService _reviewService;

        public HomeController(IServiceProvider services)
        {
            _productService = services.GetRequiredService<ProductService>();
            _categoryService = services.GetRequiredService<CategoryService>();
            _bannerService = services.GetRequiredService<BannerService>();
            _reviewService = services.GetRequiredService<ReviewService>();
        }

        public async Task<IActionResult> Index()
        {
            var seller = await HttpContext.GetMemberAsync();

            var model = new DashboardModel
            {
                ProductListEvaluation = await _productService.EvaluateAsync(new ProductFilter { SellerId = seller.Id }),
                CategoryListEvaluation = await _categoryService.EvaluateAsync(new CategoryFilter { SellerId = seller.Id }),
                BannerListEvaluation = await _bannerService.EvaluateAsync(new BannerFilter { SellerId = seller.Id }),
                ReviewListEvaluation = await _reviewService.EvaluateAsync(new ReviewFilter { SellerId = seller.Id })
            };
            return View(model);
        }
    }
}