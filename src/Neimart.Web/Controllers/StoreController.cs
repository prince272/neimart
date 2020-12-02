using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Database;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Infrastructure.Web.Sitemap;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Areas.Portal.Controllers;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Controllers
{
    public class StoreController : Controller
    {
        private readonly AppService _appService;
        private readonly MediaService _mediaService;
        private readonly UserService _userService;
        private readonly ProductService _productService;
        private readonly AddressService _addressService;
        private readonly CategoryService _categoryService;
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly OrderItemService _orderItemService;
        private readonly ReviewService _reviewService;
        private readonly MessageService _messageService;
        private readonly TransactionService _transactionService;
        private readonly AppSettings _appSettings;

        public StoreController(
            AppService appService,
            MediaService mediaService,
            UserService userService,
            ProductService productService,
            AddressService addressService,
            CategoryService categoryService,
            CartService cartService,
            OrderService orderService,
            OrderItemService orderItemService,
            ReviewService reviewService,
            MessageService messageService,
            TransactionService transactionService,
            IOptions<AppSettings> appSettingsAccessor)
        {
            _appService = appService;
            _mediaService = mediaService;
            _userService = userService;
            _productService = productService;
            _addressService = addressService;
            _categoryService = categoryService;
            _cartService = cartService;
            _orderService = orderService;
            _orderItemService = orderItemService;
            _reviewService = reviewService;
            _messageService = messageService;
            _transactionService = transactionService;
            _appSettings = appSettingsAccessor.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Sitemap()
        {
            return View();
        }

        // Cache for 30 minutes.
        [ResponseCache(Duration = 1800)]
        public async Task<IActionResult> SitemapXml()
        {
            var seller = await HttpContext.GetSellerAsync();

            var sitemapList = new SitemapUrlList();

            var products = await _productService.GetQuery().Where(x => x.SellerId == seller.Id && x.Published && !string.IsNullOrWhiteSpace(x.Slug)).Select(x => new { x.Slug }).ToListAsync();
            var categories = await _categoryService.GetQuery().Where(x => x.SellerId == seller.Id && x.Published && !string.IsNullOrWhiteSpace(x.Slug)).Select(x => new { x.Slug }).ToListAsync();

            sitemapList.Add(SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Index)))));
            sitemapList.AddRange(from product in products select SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Products), "Store", new { slug = product.Slug }))));
            sitemapList.AddRange(from category in categories select SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Products), "Store", new { slug = category.Slug }))));
            sitemapList.Add(SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Products)))));
            sitemapList.Add(SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(About)))));
            sitemapList.Add(SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Contact)))));
            sitemapList.Add(SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(Terms)))));
            sitemapList.Add(SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(PrivacyPolicy)))));
            sitemapList.Add(SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(ReturnPolicy)))));
            sitemapList.Add(SitemapUrl.CreateUrl(Url.ContentLink(Url.Action(nameof(ReviewPolicy)))));

            return File(Encoding.UTF8.GetBytes(sitemapList.ToXml()), WebPathHelper.GetMimeType(".xml"));
        }

        public async Task<IActionResult> Suggestions(string query)
        {
            var seller = await HttpContext.GetSellerAsync();

            var categoriesAndTags = await _categoryService.GetQuery(new CategoryFilter { SellerId = seller.Id, Published = true, Search = query })
                                                .Take(6).Select(x => new { Name = x.Name.ToLowerInvariant(), Tags = x.Tags.Select(x => x.Name.ToLowerInvariant()) }).ToListAsync();

            var productsAndTags = await _productService.GetQuery(new ProductFilter { SellerId = seller.Id, Published = true, Search = query })
                                                .Take(3).Select(x => new { Name = x.Name.ToLowerInvariant(), Tags = x.Tags.Select(x => x.Name.ToLowerInvariant()) }).ToListAsync();

            var result = new List<string>();

            result.AddRange(categoriesAndTags.Select(x => x.Name));
            result.AddRange(productsAndTags.Select(x => x.Name));

            result.AddRange(categoriesAndTags.SelectMany(x => x.Tags));
            result.AddRange(productsAndTags.SelectMany(x => x.Tags));

            result = result.Distinct().OrderBy(x => x.StartsWith(query ?? string.Empty, StringComparison.InvariantCultureIgnoreCase) ? 0 : 1).ToList();

            return Json(result);
        }

        [HttpGet, ModelState]
        public IActionResult Contact()
        {
            return View();
        }

        [ValidateRecaptcha]
        [HttpPost, ModelState]
        public async Task<IActionResult> Contact(ContactModel model)
        {
            if (ModelState.IsValid)
            {
                var seller = await HttpContext.GetSellerAsync();
                await _messageService.SendEmailAsync(
                                                       messageRole: MessageRole.Notification,
                                                       messageType: MessageType.StoreContact,
                                                       messageDisplay: $"{seller.StoreName} via Neimart",
                                                       email: seller.Email,
                                                       subject: model.Subject,
                                                       model: (seller, model));

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, "Thank you for contacting us. We will reply as soon as possible.");
            }

            return RedirectToAction();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult ReturnPolicy()
        {
            return View();
        }

        public IActionResult ReviewPolicy()
        {
            return View();
        }

        public IActionResult EditProfile()
        {
            return RedirectToAction(nameof(EditProfile), "Account", new { area = string.Empty, returnUrl = Url.Action(nameof(Index)) });
        }

        [Authorize]
        public async Task<IActionResult> AddReview(ReviewEditModel model, long productId)
        {
            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();

                await _appService.PrepareModelAsync(model, null);
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    var seller = await HttpContext.GetSellerAsync();
                    var customer = await HttpContext.GetMemberAsync();

                    var review = new Review();
                    await _appService.PrepareReviewAsync(review, model);

                    review.ProductId = productId;
                    review.CustomerId = customer.Id;
                    review.Approved = true;

                    await _reviewService.CreateAsync(review);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{review.Title}\" review was added.");
                }
            }

            return PartialView("ReviewEdit", model);
        }

        [Authorize]
        public async Task<IActionResult> EditReview(ReviewEditModel model, long reviewId)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            var review = await _reviewService.GetAsync(new ReviewFilter { SellerId = seller.Id, CustomerId = customer.Id, ReviewId = reviewId });

            if (review == null)
            {
                return NotFound();
            }

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();

                await _appService.PrepareModelAsync(model, review);
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    var existingApproved = review.Approved;
                    await _appService.PrepareReviewAsync(review, model);

                    review.Approved = existingApproved;
                    await _reviewService.UpdateAsync(review);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{review.Title}\" review was updated.");
                }
            }

            return PartialView("ReviewEdit", model);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> DeleteReview(long reviewId, string returnUrl)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();
            var review = await _reviewService.GetAsync(new ReviewFilter { SellerId = seller.Id, CustomerId = customer.Id, ReviewId = reviewId });

            if (review != null)
            {
                await _reviewService.DeleteAsync(review);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{review.Title}\" review was deleted.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Review does not exist.");
            }

            return LocalRedirect(returnUrl ?? Url.Action(nameof(Reviews)));
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Reviews(ReviewFilter filter, int page)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            filter.SellerId = seller.Id;
            filter.CustomerId = customer.Id;

            var reviews = await _reviewService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new ReviewListModel
            {
                Filter = filter
            };

            await _appService.PrepareModelAsync(model, reviews);

            return View("ReviewList", model);
        }

        [HttpGet]
        public async Task<IActionResult> Products(ProductFilter filter, int page)
        {
            var seller = await HttpContext.GetSellerAsync();

            filter.SellerId = seller.Id;
            filter.Slug ??= string.Empty;

            var product = await _productService.GetAsync(filter);

            if (product != null)
            {
                return await PrepareProductDetailsView(product, page);
            }
            else
            {
                filter.Slug = null;
                return await PrepareProductListView(filter, page);
            }
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Orders(OrderFilter filter, int page)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            filter.SellerId = seller.Id;
            filter.CustomerId = customer.Id;

            var orders = await _orderService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new OrderListModel
            {
                Filter = filter
            };
            await _appService.PrepareModelAsync(model, orders);

            return View("OrderList", model);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> OrderDetails(string orderCode)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();
            var order = await _orderService.GetAsync(new OrderFilter { SellerId = seller.Id, CustomerId = customer.Id, OrderCode = orderCode ?? string.Empty });

            if (order == null)
            {
                return NotFound();
            }

            if (HttpMethods.IsGet(HttpContext.Request.Method))
            {
                ModelState.Clear();
            }

            var model = new OrderModel();
            await _appService.PrepareModelAsync(model, order);

            return PartialView("Partials/_SharedOrderDetails", model);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Downloads(int page)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            var orderIds = await _orderService.GetQuery(new OrderFilter
            {
                SellerId = seller.Id,
                CustomerId = customer.Id,
                Status = OrderStatus.Complete
            }).Select(x => x.Id).ToArrayAsync();

            var orderItems = await _orderItemService.GetQuery(new OrderItemFilter
            {
                OrderIds = orderIds,
                DocumentRequired = true
            }).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new OrderItemListModel();

            await _appService.PrepareModelAsync(model, orderItems);

            return View("DownloadList", model);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> DownloadOrderDocument(long orderItemId)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            var orderIds = await _orderService.GetQuery(new OrderFilter
            {
                SellerId = seller.Id,
                CustomerId = customer.Id,
                Status = OrderStatus.Complete,
                Paid = true,
            }).Select(x => x.Id).ToArrayAsync();
            var orderItem = await _orderItemService.GetAsync(new OrderItemFilter { OrderIds = orderIds, OrderItemId = orderItemId, DocumentRequired = true });

            if (orderItem == null)
            {
                return NotFound();
            }

            var token = await _mediaService.GenerateExpiryTokenAsync(orderItem.Document);
            return RedirectToAction("Download", "Media", new { token });
        }

        public async Task<IActionResult> DownloadProductDocument(long productId)
        {
            var seller = await HttpContext.GetSellerAsync();
            var product = await _productService.GetAsync(new ProductFilter { SellerId = seller.Id, ProductId = productId, DocumentRequired = true });

            if (product != null && product.Free)
            {
                var token = await _mediaService.GenerateExpiryTokenAsync(product.Document);
                return RedirectToAction("Download", "Media", new { token });
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> ProcessCart(long productId, CartType? cartType, int? quantity, bool feedback, string returnUrl)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();
            var product = await _productService.GetAsync(new ProductFilter { SellerId = seller.Id, ProductId = productId, Published = true });

            if (product != null)
            {
                var carts = await _cartService.ListAsync(new CartFilter { CustomerId = customer.Id, SellerId = seller.Id });
                var cart = carts.FirstOrDefault(x => x.ProductId == product.Id);

                var cartCountLimitReached = (carts.Where(x => x.Type == CartType.Cart)
                                                  .Count() >= _appSettings.CartMaxCount) && cartType == CartType.Cart ? CartType.Cart :

                                            carts.Where(x => x.Type == CartType.Wishlist)
                                                 .Count() >= _appSettings.CartMaxCount && cartType == CartType.Wishlist ? CartType.Wishlist : (CartType?)null;

                if (quantity == 0)
                {
                    if (cart != null)
                    {
                        await _cartService.DeleteAsync(cart);

                        TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"Product was removed from the {cart.Type.GetEnumText().ToLowerInvariant()}.");
                    }
                    else
                    {
                        TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"Cart does not exist.");
                    }
                }
                else
                {
                    if (cartCountLimitReached == null)
                    {
                        if (cart != null)
                        {
                            int maxQuantity = _appSettings.QuantityMaxValue;
                            int currentQuantity = cart.Quantity += quantity.GetValueOrDefault();

                            cart.Quantity = Math.Max(1, Math.Min(currentQuantity, maxQuantity));
                            cart.Type = cartType.GetValueOrDefault(cart.Type);

                            var result = _cartService.Validate(cart);

                            if (result.Success)
                            {
                                await _cartService.UpdateAsync(cart);

                                if (feedback)
                                {
                                    TempData.AddAlert(AlertMode.Confirm, AlertType.Success, $"{product.Name} was added to {cart.Type.GetEnumText().ToLowerInvariant()}.", returnUrl: Url.Action(nameof(Cart)), returnText: "View cart & checkout", cancelText: "Continue shopping");
                                }
                                else
                                {
                                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"{product.Name} was added to {cart.Type.GetEnumText().ToLowerInvariant()}.");
                                }
                            }
                            else
                            {
                                TempData.AddAlert(AlertMode.Notify, AlertType.Error, result.Message);
                            }
                        }
                        else
                        {
                            int maxQuantity = _appSettings.QuantityMaxValue;
                            int currentQuantity = quantity.GetValueOrDefault();

                            cart = new Cart
                            {
                                Quantity = Math.Max(1, Math.Min(currentQuantity, maxQuantity)),
                                Type = cartType.GetValueOrDefault(),

                                ProductId = product.Id,
                                CustomerId = customer.Id,
                                SellerId = seller.Id,

                                // Validating the cart requires additional properties to be set, but performing 
                                // a database operation when those properties are set it may lead to errors. 
                                Product = product,
                                Customer = customer,
                                Seller = seller
                            };

                            var result = _cartService.Validate(cart);

                            // So we null those properties after they've validated.
                            cart.Product = null;
                            cart.Customer = null;
                            cart.Seller = null;

                            if (result.Success)
                            {
                                await _cartService.CreateAsync(cart);

                                if (feedback)
                                {
                                    TempData.AddAlert(AlertMode.Confirm, AlertType.Success, $"{product.Name} was added to {cart.Type.GetEnumText().ToLowerInvariant()}.", returnUrl: Url.Action(nameof(Cart)), returnText: "View cart & checkout", cancelText: "Continue shopping");
                                }
                                else
                                {
                                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"{product.Name} was added to {cart.Type.GetEnumText().ToLowerInvariant()}.");
                                }
                            }
                            else
                            {
                                TempData.AddAlert(AlertMode.Notify, AlertType.Error, result.Message);
                            }
                        }
                    }
                    else
                    {
                        TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"{cartCountLimitReached.Value.GetEnumText()} limit has been reached.");
                    }
                }
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, "Product does not exist.");
            }

            return LocalRedirect(returnUrl ?? Url.Action(nameof(Index)));
        }

        [HttpGet, Authorize]
        public Task<IActionResult> Cart()
        {
            return PrepareCartListView(CartType.Cart);
        }

        [HttpGet, Authorize]
        public Task<IActionResult> Wishlist()
        {
            return PrepareCartListView(CartType.Wishlist);
        }

        [ModelState]
        [Authorize]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            // Ensure all carts are validated before evaluation. Return to cart page if customers cart is empty.
            var carts = await _cartService.ListAsync(new CartFilter { SellerId = seller.Id, CustomerId = customer.Id, Type = CartType.Cart });        
            carts = carts.Where(x => _cartService.Validate(x).Success);
            if (!carts.Any()) return RedirectToAction(nameof(Cart));

            var addressTypes = EnumHelper.GetEnumValues<AddressType>().ToList();
            if (!seller.StoreDeliveryRequired) addressTypes.Remove(AddressType.Delivery);

            var addresses = await _addressService.ListAsync(new AddressFilter { CustomerId = customer.Id, AddressTypes = addressTypes.ToArray() });
            var billingAddress = addresses.FirstOrDefault(x => x.AddressTypes.Contains(AddressType.Billing));
            var deliveryAddress = addresses.FirstOrDefault(x => x.AddressTypes.Contains(AddressType.Delivery));

            // Get the remaining address types that wasn't found in all address types.
            var unsetAddressTypes = addressTypes.Except(addresses.SelectMany(x => x.AddressTypes).Distinct()).ToList();
            var delivery = deliveryAddress != null ? await _userService.GetDeliveryAsync(seller, deliveryAddress) : null;

            var deliveryRequired = seller.StoreDeliveryRequired;
            var deliveryCalculated = delivery != null;
            var deliveryFee = delivery?.Fee ?? 0;
            var subtotalAmount = (await _cartService.EvaluateAsync(carts)).TotalAmount;
            var totalAmount = subtotalAmount + deliveryFee;

            if (!HttpMethods.IsPost(Request.Method))
            {
                model = new CheckoutModel
                {
                    CartListModel = new CartListModel()
                };

                model.DeliveryRequired = deliveryRequired;
                model.DeliveryCalculated = deliveryCalculated;
                model.DeliveryFee = deliveryFee;
                model.SubtotalAmount = subtotalAmount;
                model.TotalAmount = totalAmount;

                foreach (var addressType in addressTypes)
                {
                    var address = addresses.FirstOrDefault(x => x.AddressTypes.Contains(addressType));

                    var addressModel = new AddressModel
                    {
                        Address = address,
                        AddressType = addressType
                    };

                    model.AddressModels.Add(addressModel);
                }

                await _appService.PrepareModelAsync(model.CartListModel, carts.ToPageable());

                return View(model);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (!unsetAddressTypes.Any())
                    {
                        var orderCode = _appSettings.GenerateCode("Order", seller.StoreName);
                        var trackingCode = _appSettings.GenerateCode("Tracking", seller.StoreName);

                        var order = new Order
                        {
                            SellerId = seller.Id,
                            CustomerId = customer.Id,

                            OrderCode = orderCode,
                            TrackingCode = trackingCode,

                            Status = OrderStatus.Pending,

                            BillingAddress = billingAddress,
                            DeliveryAddress = deliveryAddress,
                            DeliveryFee = deliveryFee,
                            DeliveryRequired = deliveryRequired,
                        };

                        await _orderService.CreateAsync(order);

                        foreach (var cart in carts)
                        {
                            var orderItem = new OrderItem
                            {
                                OrderId = order.Id,
                                ProductId = cart.ProductId,
                                Slug = cart.Product.Slug,
                                Name = cart.Product.Name,
                                Sku = cart.Product.Sku,
                                Price = cart.Product.Price,
                                Quantity = cart.Quantity,
                                Image = cart.Product.Image?.AsMedia(),
                                Document = cart.Product.Document?.AsMedia(),
                            };

                            await _orderItemService.CreateAsync(orderItem);
                            await _cartService.DeleteAsync(cart);

                            order.OrderItems.Add(orderItem);
                        }

                        await _orderService.UpdateAsync(order);

                        // Reload order.
                        order = await _orderService.GetAsync(new OrderFilter { OrderId = order.Id });

                        // Prepare order model
                        var orderModel = new OrderModel();
                        await _appService.PrepareModelAsync(orderModel, order);

                        // Send order placed email to the seller.
                        await _messageService.SendStoreOrderEmailAsync(orderModel);

                        // Send order status email to the customer.
                        await _messageService.SendCustomerOrderEmailAsync(orderModel);


                        return RedirectToAction(nameof(CheckoutComplete), new
                        {
                            orderCode,
                            modalUrl = Url.Action(nameof(AccountController.CashIn), "Account", new
                            {
                                reference = orderCode,
                                type = TransactionType.Order,
                                returnUrl = Url.Action(nameof(CheckoutComplete), new { orderCode })
                            })
                        });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Please add your {unsetAddressTypes.Humanize().ToLowerInvariant()} address.");
                    }
                }

                return RedirectToAction(nameof(Checkout));
            }
        }

        [HttpGet, Authorize]
        public IActionResult CheckoutComplete()
        {
            return View();
        }

        [NonAction]
        public async Task<IActionResult> PrepareProductDetailsView(Product product, int page)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            var model = new ProductModel();
            await _appService.PrepareModelAsync(model, product);

            var reviewFilter = new ReviewFilter
            {
                Approved = true,
                SellerId = seller.Id,
                ProductId = product.Id
            };
            var reviewEvaluation = await _reviewService.EvaluateAsync(reviewFilter);

            model.ReviewListModel = new ReviewListModel
            {
                Filter = reviewFilter,
                Evaluation = reviewEvaluation
            };
            model.ReviewEvaluation = reviewEvaluation;

            if (customer != null)
            {
                model.IsAddedToCart = await _cartService.GetQuery(new CartFilter { CustomerId = customer.Id, SellerId = seller.Id, ProductId = product.Id, Type = CartType.Cart }).AnyAsync();
                model.IsAddedToWishlist = await _cartService.GetQuery(new CartFilter { CustomerId = customer.Id, SellerId = seller.Id, ProductId = product.Id, Type = CartType.Wishlist }).AnyAsync();
            }

            var reviews = await _reviewService.GetQuery(reviewFilter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            await _appService.PrepareModelAsync(model.ReviewListModel, reviews);

            return View("ProductDetails", model);
        }

        [NonAction]
        private async Task<IActionResult> PrepareProductListView(ProductFilter filter, int page)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            var products = await _productService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new ProductListModel
            {
                Filter = filter,
            };

            await _appService.PrepareModelAsync(model, products, seller, customer);

            return View("ProductList", model);
        }

        [NonAction]
        private async Task<IActionResult> PrepareCartListView(CartType cartType)
        {
            var seller = await HttpContext.GetSellerAsync();
            var customer = await HttpContext.GetMemberAsync();

            var filter = new CartFilter { SellerId = seller.Id, CustomerId = customer.Id, Type = cartType };

            var carts = (await _cartService.ListAsync(filter));

            // Ensure all carts are validated before evaluation.
            carts = carts.Where(x => _cartService.Validate(x).Success);

            var model = new CartListModel
            {
                Filter = filter,
                CartType = cartType
            };
            await _appService.PrepareModelAsync(model, carts.ToPageable());

            return View("CartList", model);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Addresses(AddressFilter filter, int page)
        {
            var customer = await HttpContext.GetMemberAsync();

            filter.CustomerId = customer.Id;

            var addresses = await _addressService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new AddressListModel
            {
                Filter = filter,
                AddressType = null
            };

            await _appService.PrepareModelAsync(model, addresses);
            return View("AddressList", model);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> AddressBook(AddressType? showAddressType)
        {
            var customer = await HttpContext.GetMemberAsync();

            var filter = new AddressFilter { CustomerId = customer.Id };
            var addresses = await _addressService.ListAsync(filter);

            var model = new AddressListModel
            {
                Filter = filter,
                AddressType = showAddressType
            };

            await _appService.PrepareModelAsync(model, addresses.ToPageable());

            return PartialView("AddressBook", model);
        }

        [Authorize]
        public async Task<IActionResult> AddAddress(AddressEditModel model, AddressType? addressType)
        {
            var customer = await HttpContext.GetMemberAsync();

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();

                if (addressType != null)
                    model.AddressTypes.Add(addressType.Value);

                // Set predefined values when adding a new address.
                model.FirstName = customer.FirstName;
                model.LastName = customer.LastName;
                model.Email = customer.Email;
                model.PhoneNumber = customer.PhoneNumber;
                model.Organization = customer.StoreName;

                await _appService.PrepareModelAsync(model, null);
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    var addresses = await _addressService.ListAsync(new AddressFilter { CustomerId = customer.Id });
                    var address = new Address();
                    await _appService.PrepareAddressAsync(address, model);

                    address.CustomerId = customer.Id;
                    await _addressService.CreateAsync(address);
                    await _addressService.ResolveAddressTypesAsync(address, addresses);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, "Address was added.");
                }
            }

            return PartialView("AddressEdit", model);
        }

        [Authorize]
        public async Task<IActionResult> EditAddress(AddressEditModel model, long addressId)
        {
            var customer = await HttpContext.GetMemberAsync();

            var addresses = await _addressService.ListAsync(new AddressFilter { CustomerId = customer.Id });
            var address = addresses.FirstOrDefault(x => x.Id == addressId);

            if (address == null)
            {
                return NotFound();
            }

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();

                await _appService.PrepareModelAsync(model, address);
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    await _appService.PrepareAddressAsync(address, model);

                    await _addressService.UpdateAsync(address);
                    await _addressService.ResolveAddressTypesAsync(address, addresses);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, "Address was updated.");
                }
            }

            return PartialView("AddressEdit", model);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> DeleteAddress(long addressId, string returnUrl)
        {
            var customer = await HttpContext.GetMemberAsync();
            var addresses = await _addressService.ListAsync(new AddressFilter { CustomerId = customer.Id });
            var address = addresses.FirstOrDefault(x => x.Id == addressId);

            if (address != null)
            {
                // Ensure the addresses does not contain the current address since it'll be deleted soon.
                addresses = addresses.Where(x => x.Id != address.Id);

                await _addressService.DeleteAsync(address);
                await _addressService.ResolveAddressTypesAsync(null, addresses);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"Address was deleted.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Address does not exist.");
            }

            return LocalRedirect(returnUrl ?? Url.Action(nameof(Addresses)));
        }


        [HttpPost, Authorize]
        public async Task<IActionResult> SetAddressType(long addressId, AddressType addressType, bool toggle, string returnUrl)
        {
            var customer = await HttpContext.GetMemberAsync();
            var addresses = await _addressService.ListAsync(new AddressFilter { CustomerId = customer.Id });
            var address = addresses.FirstOrDefault(x => x.Id == addressId);

            if (address != null)
            {
                if (toggle) address.AddressTypes.Add(addressType);
                else address.AddressTypes.Remove(addressType);

                await _addressService.UpdateAsync(address);
                await _addressService.ResolveAddressTypesAsync(address, addresses);
                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"One or more address types was changed.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Address does not exist.");
            }

            return LocalRedirect(returnUrl ?? Url.Action(nameof(Addresses)));
        }

        public async Task<IActionResult> Unavailable()
        {
            if (Request.IsAjax())
                return Forbid();

            var seller = await HttpContext.GetSellerAsync();

            if (seller.StoreAccess != StoreAccess.Pending &&
                seller.StoreAccess != StoreAccess.Rejected && 
               !seller.StorePlanEnded)
                return RedirectToAction(nameof(Index));

            return View();
        }

        public async Task<IActionResult> Closed()
        {
            if (Request.IsAjax())
                return Forbid();

            var seller = await HttpContext.GetSellerAsync();

            if (seller.StoreStatus != StoreStatus.Closed)
                return RedirectToAction(nameof(Index));

            return View();
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionDescriptor = (context.ActionDescriptor as ControllerActionDescriptor);

            if (actionDescriptor != null)
            {
                // Allow access to the sitemap without any code logics.
                if (actionDescriptor.ActionName == nameof(SitemapXml)) { }
                else
                {
                    var seller = await HttpContext.GetSellerAsync();

                    // Redirect to unavailable when the sellers store status is marked as pending, rejected or his plan has ended.
                    if (seller.StoreAccess == StoreAccess.Pending || 
                        seller.StoreAccess == StoreAccess.Rejected || 
                        seller.StorePlanEnded)
                    {
                        if (actionDescriptor.ActionName != nameof(Unavailable))
                            context.Result = RedirectToAction(nameof(Unavailable));
                    }

                    // Redirect to unavailable page when the sellers store status is marked as closed.
                    else if (seller.StoreStatus == StoreStatus.Closed)
                    {
                        if (actionDescriptor.ActionName != nameof(Closed))
                            context.Result = RedirectToAction(nameof(Closed));
                    }
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}