using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neimart.Core;
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
    public class TransactionsController : Controller
    {
        private readonly TransactionService _transactionService;
        private readonly AppService _appService;
        private readonly AppSettings _appSettings;
        private readonly UserService _userService;

        public TransactionsController(IServiceProvider services)
        {
            _transactionService = services.GetRequiredService<TransactionService>();
            _appService = services.GetRequiredService<AppService>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
            _userService = services.GetRequiredService<UserService>();
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page, TransactionFilter filter)
        {
            var seller = await HttpContext.GetMemberAsync();

            if (!(await _userService.IsInRoleAsync(seller, RoleNames.Admin)))
                filter.MemberId = seller.Id;

            var categories = await _transactionService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new TransactionListModel
            {
                Filter = filter
            };

            await _appService.PrepareModelAsync(model, categories);

            return View(model);
        }


        public async Task<IActionResult> Details(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var memberId = !(await _userService.IsInRoleAsync(seller, RoleNames.Admin)) ? seller.Id : default(long?);
     
            var transaction = await _transactionService.GetAsync(new TransactionFilter { MemberId = memberId, TransactionId = id });

            if (transaction == null)
            {
                return NotFound();
            }

            var model = new TransactionModel();

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
                await _appService.PrepareModelAsync(model, transaction);
            }

            return PartialView(model);
        }
    }
}