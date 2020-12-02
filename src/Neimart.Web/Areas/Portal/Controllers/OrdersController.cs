using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
using Neimart.Core.Utilities.Extensions;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Areas.Portal.Controllers
{
    [Area("Portal")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Seller)]
    [PlanRequired]
    public class OrdersController : Controller
    {
        private readonly OrderService _orderService;
        private readonly UserService _userService;
        private readonly MessageService _messageService;
        private readonly AppService _appService;
        private readonly AppSettings _appSettings;

        public OrdersController(IServiceProvider services)
        {
            _orderService = services.GetRequiredService<OrderService>();
            _userService = services.GetRequiredService<UserService>();
            _messageService = services.GetRequiredService<MessageService>();
            _appService = services.GetRequiredService<AppService>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page, OrderFilter filter)
        {
            var seller = await HttpContext.GetMemberAsync();

            if (!(await _userService.IsInRoleAsync(seller, RoleNames.Admin)))
                filter.SellerId = seller.Id;

            var banners = await _orderService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new OrderListModel
            {
                Filter = filter
            };

            await _appService.PrepareModelAsync(model, banners);

            return View(model);
        }

        public async Task<IActionResult> Details(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var filter = new OrderFilter { OrderId = id };

            if (!(await _userService.IsInRoleAsync(seller, RoleNames.Admin)))
                filter.SellerId = seller.Id;

            var order = await _orderService.GetAsync(filter);

            if (order == null)
            {
                return NotFound();
            }

            var model = new OrderModel();

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
                await _appService.PrepareModelAsync(model, order);
            }

            return PartialView("Partials/_SharedOrderDetails", model);
        }

        public async Task<IActionResult> Status(OrderStatusModel model)
        {
            var seller = await HttpContext.GetMemberAsync();
            var filter = new OrderFilter { OrderId = model.Id };

            if (!(await _userService.IsInRoleAsync(seller, RoleNames.Admin)))
                filter.SellerId = seller.Id;

            var order = await _orderService.GetAsync(filter);

            if (order == null)
            {
                return NotFound();
            }

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
                await _appService.PrepareModelAsync(model, order);
            }

            if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    var canChangeStatus = await _orderService.CanChangeStatusAsync(order, model.Status);

                    if (canChangeStatus.Success)
                    {
                        order.Status = model.Status;

                        if (order.Status == OrderStatus.Processing)
                        {
                            order.ProcessingInfo = model.ProcessingInfo;
                        }
                        if (order.Status == OrderStatus.Delivering)
                        {
                            order.DeliveryInfo = model.DeliveryInfo;
                        }
                        else if (order.Status == OrderStatus.Complete)
                        {

                        }
                        else if (order.Status == OrderStatus.Cancelled)
                        {
                            order.CancelReason = model.CancelReason;
                            order.Refunded = model.Refunded;

                            if (order.Refunded && order.Paid)
                            {
                                var transfer = await _userService.TransferAsync(order.Seller, order.Customer, order.TotalAmount);

                                if (!transfer.Success)
                                {
                                    ModelState.AddModelError(string.Empty, transfer.Message);
                                    return PartialView(model);
                                }
                            }
                        }

                        await _orderService.UpdateAsync(order);

                        TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{order.OrderCode}\" order was updated to {order.Status.GetEnumText().ToLowerInvariant()}.");

                        // Prepare order model
                        var orderModel = new OrderModel();
                        await _appService.PrepareModelAsync(orderModel, order);

                        // Send order status email to the customer.
                        await _messageService.SendCustomerOrderEmailAsync(orderModel);
                    }
                    else
                    {
                        ModelState.AddResult(canChangeStatus);
                    }
                }
            }

            return PartialView(model);
        }
    }
}