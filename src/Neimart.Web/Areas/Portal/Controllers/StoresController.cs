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
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Storing;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities;
using Neimart.Core.Utilities.Extensions;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Areas.Portal.Controllers
{
    [Area("Portal")]
    [Authorize(Roles = RoleNames.Admin)]
    [PlanRequired]
    public class StoresController : Controller
    {
        private readonly UserService _userService;
        private readonly MediaService _mediaService;
        private readonly AppService _appService;
        private readonly AppSettings _appSettings;

        public StoresController(IServiceProvider services)
        {
            _userService = services.GetRequiredService<UserService>();
            _mediaService = services.GetRequiredService<MediaService>();
            _appService = services.GetRequiredService<AppService>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page, UserFilter filter)
        {
            var seller = await HttpContext.GetMemberAsync();

            var users = await _userService.GetQuery(filter)
                                          .ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new UserListModel
            {
                Filter = filter
            };

            await _appService.PrepareModelAsync(model, users);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetStoreAccess(long id, StoreAccess access)
        {
            var user = await _userService.FindByIdAsync(id);

            if (user != null)
            {
                user.StoreAccess = access;
                await _userService.UpdateAsync(user);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{user.StoreName}\" store access was mark as {user.StoreAccess.GetEnumText()}.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"User does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(long id)
        {
            var user = await _userService.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserModel();

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
                await _appService.PrepareModelAsync(model, user);
            }

            return PartialView(model);
        }
    }
}