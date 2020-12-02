using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Storing;
using Neimart.Core.Infrastructure.Paying;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Models;
using Neimart.Web.Models.Account;
using Neimart.Web.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Neimart.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly AppSettings _appSettings;
        private readonly AppService _appService;
        private readonly MediaService _mediaService;
        private readonly OrderService _orderService;
        private readonly SignInService _signInService;
        private readonly MessageService _messageService;
        private readonly TransactionService _transactionService;
        private readonly IPaymentProcessor _paymentProcessor;

        public AccountController(IServiceProvider services)
        {
            _userService = services.GetRequiredService<UserService>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
            _appService = services.GetRequiredService<AppService>();
            _mediaService = services.GetRequiredService<MediaService>();
            _orderService = services.GetRequiredService<OrderService>();
            _signInService = services.GetRequiredService<SignInService>();
            _messageService = services.GetRequiredService<MessageService>();
            _transactionService = services.GetRequiredService<TransactionService>();
            _paymentProcessor = services.GetRequiredService<IPaymentProcessor>();
        }

        [HttpGet, ModelState]
        public async Task<IActionResult> Signup()
        {
            var model = new SignUpModel();
            model.ExternalLogins.AddRange(await _signInService.GetExternalAuthenticationSchemesAsync());
            return View(model);
        }

        [ValidateRecaptcha]
        [HttpPost, ModelState]
        public async Task<IActionResult> Signup(SignUpModel model, string storeName, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var member = new User
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                };

                await _userService.GenerateUserCodeAsync(member);
                await _userService.GenerateUserNameAsync(member);
                await PrepareStoreAsync(member);
                var result = await _userService.CreateAsync(member, model.Password);

                if (result.Succeeded)
                {
                    result = await _userService.AddToRolesAsync(member, new[] { RoleNames.Seller, RoleNames.Customer });

                    if (!result.Succeeded)
                        throw new InvalidOperationException(result.Errors.Select(x => x.Description).Humanize());

                    if (!_signInService.Options.SignIn.RequireConfirmedEmail &&
                        !_signInService.Options.SignIn.RequireConfirmedPhoneNumber)
                    {
                        await _signInService.SignInAsync(member, isPersistent: false);

                        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home", new { area = "Portal" }));
                    }
                    else
                    {
                        var token = await _userService.GenerateEmailConfirmationTokenAsync(member);
                        var link = Url.Action(nameof(VerifyEmail), "Account", new { userId = member.Id, token, returnUrl }, protocol: Request.Scheme);

                        await _messageService.SendEmailAsync(
                                  messageRole: MessageRole.Support,
                                  messageType: MessageType.VerifyEmail,
                                  messageDisplay: "Neimart Support",
                                  email: member.Email,
                                  model: new ValueTuple<User, string>(member, link));

                        TempData.AddAlert(AlertMode.Alert, AlertType.Success, $"Congratulations! Your account has been created successfully. We'll send you an email within a few minutes with instructions to verify your email address. If the email does not arrive soon, check your spam, junk, and bulk mail folders.", title: "Email Verification Required");

                        return RedirectToAction(nameof(Signin), new { returnUrl });
                    }
                }
                else
                {
                    ModelState.AddIdentityResult(result);
                }
            }

            return RedirectToAction(nameof(Signup), new { storeName, returnUrl });
        }

        private async Task PrepareStoreAsync(User member)
        {
            member.StoreStatus = StoreStatus.Opened;
            member.StoreAccess = StoreAccess.Approved;
            member.StorePlanEndedOn = DateTimeOffset.UtcNow.AddDays(_appSettings.PlanTrialDays);
            member.StoreDeliveryRequired = true;

            var theme = await HttpContext.GetThemeAsync();

            member.StoreThemeMode = theme.Mode;
            member.StoreThemeStyle = theme.Style;
        }

        [HttpGet, ModelState]
        public async Task<IActionResult> Signin()
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var model = new SignInModel();
            model.ExternalLogins.AddRange(await _signInService.GetExternalAuthenticationSchemesAsync());

            return View(model);
        }

        [ValidateRecaptcha]
        [HttpPost, ModelState]
        public async Task<IActionResult> Signin(SignInModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var member = await _userService.FindByEmailAsync(model.Email);

                if (member != null)
                {
                    var result = await _signInService.PasswordSignInAsync(member, model.Password, isPersistent: false, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home", new { area = "Portal" }));
                    }
                    else if (result.IsLockedOut)
                    {
                        return RedirectToAction(nameof(Lockout), new { returnUrl });
                    }
                    else if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction(nameof(Signin2fa), new { returnUrl });
                    }
                    else if (result.IsNotAllowed)
                    {
                        if (!await _userService.IsEmailConfirmedAsync(member))
                        {
                            var token = await _userService.GenerateEmailConfirmationTokenAsync(member);
                            var link = Url.Action(nameof(VerifyEmail), "Account", new { userId = member.Id, token, returnUrl }, protocol: Request.Scheme);

                            await _messageService.SendEmailAsync(
                                      messageRole: MessageRole.Support,
                                      messageType: MessageType.VerifyEmail,
                                      messageDisplay: "Neimart Support",
                                      email: member.Email,
                                      model: new ValueTuple<User, string>(member, link));

                            TempData.AddAlert(AlertMode.Alert, AlertType.Info, $"We'll send you an email within a few minutes with instructions to verify your email address. If the email does not arrive soon, check your spam, junk, and bulk mail folders.", title: "Email Verification Required");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "We couldn't sign you in. If you forgot your password, you can request for a password reset.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "We couldn't sign you in. If you don't have an account yet, you should create one.");
                }
            }

            return RedirectToAction(nameof(Signin), new { returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Signout(string returnUrl)
        {
            await _signInService.SignOutAsync();
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }

        [HttpPost]
        public IActionResult SigninExternal(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(SigninExternalCallback), new { returnUrl });
            var properties = _signInService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> SigninExternalCallback(string returnUrl)
        {
            var signinInfo = await _signInService.GetExternalLoginInfoAsync();

            if (signinInfo != null)
            {
                var signInResult = await _signInService.ExternalLoginSignInAsync(signinInfo.LoginProvider, signinInfo.ProviderKey, isPersistent: false, bypassTwoFactor: true);

                if (signInResult.Succeeded)
                {
                    return LocalRedirect(returnUrl ?? Url.Action("Index", "Home", new { area = "Portal" }));
                }
                else
                {
                    var email = signinInfo.Principal.FindFirstValue(ClaimTypes.Email);

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        var member = await _userService.FindByEmailAsync(email);

                        if (member != null)
                        {
                            var result = await _userService.AddLoginAsync(member, signinInfo);

                            if (result.Succeeded)
                            {
                                await _signInService.SignInAsync(member, isPersistent: false);
                                return LocalRedirect(returnUrl ?? Url.Action("Index", "Home", new { area = "Portal" }));
                            }
                        }
                        else
                        {
                            var firstName = signinInfo.Principal.FindFirstValue(ClaimTypes.GivenName);
                            var lastName = signinInfo.Principal.FindFirstValue(ClaimTypes.Surname);
                            var phoneNumber = signinInfo.Principal.FindFirstValue(ClaimTypes.MobilePhone);

                            member = new User
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                Email = email,
                                EmailConfirmed = true,
                                PhoneNumber = phoneNumber,
                            };
                            await _userService.GenerateUserCodeAsync(member);
                            await _userService.GenerateUserNameAsync(member);
                            await PrepareStoreAsync(member);

                            var result = await _userService.CreateAsync(member, Guid.NewGuid().ToString());

                            if (result.Succeeded)
                            {
                                result = await _userService.AddToRolesAsync(member, new[] { RoleNames.Seller, RoleNames.Customer });

                                if (result.Succeeded)
                                {
                                    result = await _userService.AddLoginAsync(member, signinInfo);

                                    if (result.Succeeded)
                                    {
                                        await _signInService.SignInAsync(member, isPersistent: false);
                                        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home", new { area = "Portal" }));
                                    }
                                }
                            }
                        }
                    }

                    TempData.AddAlert(AlertMode.Notify, AlertType.Error, "Signin failed, Please try another signin provider.");
                }
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, "Signin failed, No external signin information was found.");
            }

            return RedirectToAction(nameof(Signin));
        }

        public IActionResult Signin2fa()
        {
            return View();
        }

        public async Task<IActionResult> VerifyEmail(long userId, string token, string returnUrl)
        {
            var member = await _userService.FindByIdAsync(userId);
            if (member != null && !string.IsNullOrWhiteSpace(token))
            {
                var result = await _userService.ConfirmEmailAsync(member, token);

                if (result.Succeeded) TempData.AddAlert(AlertMode.Notify, AlertType.Success, "Thank you for verifying your email address.");
                else TempData.AddAlert(AlertMode.Alert, AlertType.Error, result.Errors.Humanize());
            }
            else TempData.AddAlert(AlertMode.Alert, AlertType.Error, "An error occurred while verifying your email address.");

            return LocalRedirect(returnUrl ?? Url.Action(nameof(Signin)));
        }

        [HttpGet, ModelState]
        public IActionResult ForgotPassword()
        {
            var model = new ForgotPasswordModel();
            return View(model);
        }

        [ValidateRecaptcha]
        [HttpPost, ModelState]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string email = model.Email;
                var member = await _userService.FindByEmailAsync(email);

                if (member != null)
                {
                    var token = await _userService.GeneratePasswordResetTokenAsync(member);
                    var link = Url.Action(nameof(ResetPassword), "Account", new { token, email }, protocol: Request.Scheme);

                    await _messageService.SendEmailAsync(
                        messageRole: MessageRole.Support,
                        messageType: MessageType.ResetPassword,
                        messageDisplay: "Neimart Support",
                        email: email,
                        model: new ValueTuple<User, string>(member, link));

                    TempData.AddAlert(AlertMode.Alert, AlertType.Success, "We'll send you an email within a few minutes with instructions to reset your password. If the email does not arrive soon, check your spam, junk, and bulk mail folders.",
                        returnUrl: Url.Action(nameof(Signin), new { returnUrl }));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"No user registered with {email} email.");
                }
            }

            return RedirectToAction(nameof(ForgotPassword), new { returnUrl });
        }


        [HttpGet, ModelState]
        public IActionResult ResetPassword()
        {
            var model = new ResetPasswordModel();
            return View(model);
        }

        [HttpPost, ModelState]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model, string token, string email, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(token) &&
                    !string.IsNullOrWhiteSpace(email))
                {
                    var member = await _userService.FindByEmailAsync(email);

                    if (member != null)
                    {
                        var result = await _userService.ResetPasswordAsync(member, token, model.Password);

                        if (result.Succeeded)
                        {
                            TempData.AddAlert(AlertMode.Notify, AlertType.Success, "Password was reseted.");

                            return RedirectToAction(nameof(Signin), new { email, returnUrl });
                        }
                        else
                        {
                            ModelState.AddIdentityResult(result);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"No user registered with {email} email.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Sorry, Something went wrong. Please try requesting the password reset link again.");
                }
            }

            return RedirectToAction(nameof(ResetPassword), new { token, email, returnUrl });
        }

        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            var member = await HttpContext.GetMemberAsync();

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    var result = await _userService.ChangePasswordAsync(member, model.CurrentPassword, model.NewPassword);

                    if (result.Succeeded)
                    {
                        TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"Password was changed.");
                    }
                    else
                    {
                        ModelState.AddIdentityResult(result);
                    }
                }
            }

            return PartialView(model);
        }

        [Authorize]
        public async Task<IActionResult> ChangeEmail(ChangeEmailModel model, string returnUrl)
        {
            var member = await HttpContext.GetMemberAsync();

            model.OldEmail = member.Email;

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    var email = model.NewEmail;
                    var token = await _userService.GenerateChangeEmailTokenAsync(member, email);
                    var userId = member.Id;

                    var link = Url.Action(nameof(ChangeEmailCallback), "Account", new { token, userId, email, returnUrl }, protocol: Request.Scheme);
                    await _messageService.SendEmailAsync(
                        messageRole: MessageRole.Support,
                        messageType: MessageType.ChangeEmail,
                        messageDisplay: "Neimart Support",
                        email: email,
                        model: new ValueTuple<User, string>(member, link));

                    TempData.AddAlert(AlertMode.Alert, AlertType.Success, "We'll send you an email within a few minutes with instructions to change your email address. If the email does not arrive soon, check your spam, junk, and bulk mail folders.");
                }
            }

            return PartialView(model);
        }

        public async Task<IActionResult> ChangeEmailCallback(long userId, string email, string token, string returnUrl)
        {
            var member = await _userService.FindByIdAsync(userId);

            if (member != null && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(token))
            {
                var result = await _userService.ChangeEmailAsync(member, email, token);

                if (result.Succeeded)
                {
                    TempData.AddAlert(AlertMode.Alert, AlertType.Success,
                        $"You are now logged in with the email {email}." +
                        $" You will use this email to sign into your account in the future.");

                    await _signInService.RefreshSignInAsync(member);
                }
                else
                {
                    TempData.AddAlert(AlertMode.Alert, AlertType.Error, result.Errors.Humanize());
                }
            }
            else
            {
                TempData.AddAlert(AlertMode.Alert, AlertType.Error, "Error changing email.");
            }

            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }

        [Authorize]
        public async Task<IActionResult> ChangePhoneNumber(ChangePhoneNumberModel model)
        {
            var member = await HttpContext.GetMemberAsync();

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    var result = await _userService.SetPhoneNumberAsync(member, model.NewPhoneNumber);

                    if (result.Succeeded)
                    {
                        TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"Phone number was set to {_appSettings.FormatPhoneNumber(model.NewPhoneNumber)}.");
                    }
                    else
                    {
                        ModelState.AddIdentityResult(result);
                    }
                }
            }

            return PartialView(model);
        }

        [Authorize]
        public async Task<IActionResult> CashIn(CashInModel model, string returnUrl)
        {
            var processors = EnumHelper.GetEnumValues<TransactionProcessor>().ToList();
            if (model.Type == TransactionType.Deposit ||
                model.Type == TransactionType.Withdrawal ||
                model.Type == TransactionType.Subscription)
            {
                processors.Remove(TransactionProcessor.Internal);
                model.ProcessorHide = true;
            }

            model.ProcessorOptions.AddRange(SelectListHelper.GetEnumSelectList(
                enums: processors, getText: x =>
                {
                    if (x == TransactionProcessor.Internal)
                        return "Pay with Wallet";
                    else if (x == TransactionProcessor.External)
                        return $"Pay with Card/MoMo";
                    else
                        throw new InvalidOperationException();

                }, selectedEnum: model.Processor));

            var transactionReferenceObject = default(object);
            var transactionDataList = new Dictionary<string, object>();
            var transactionMemberId = default(long);
            var transactionAccountBalance = default(decimal);
            var transactionAccountName = default(string);
            var transactionAccountEmail = default(string);
            var transactionAccountNumber = default(string);

            if (model.Type == TransactionType.Subscription)
            {
                var member = await _userService.FindByCodeAsync(model.Reference);

                var requestQuery = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString() as object);
                var planType = requestQuery.ValueOrDefault<PlanType>("planType");
                var planPeriod = requestQuery.ValueOrDefault<PlanPeriod>("planPeriod");
                var planRate = _appSettings.PlanRates.ElementAt((int)planType);
                var planAmount = planRate * (int)planPeriod;

                var paymentFee = model.Processor == TransactionProcessor.External ? Math.Round(((_appSettings.PaymentRate * 100) * planAmount) / 100, 2, MidpointRounding.AwayFromZero) : 0;

                model.Amount = planAmount;
                model.Fee = paymentFee;

                transactionMemberId = member.Id;
                transactionAccountName = member.FullName;
                transactionAccountEmail = member.Email;
                transactionAccountNumber = member.PhoneNumber;
                transactionAccountBalance = member.Balance;
                transactionDataList.Add("PlanType", planType);
                transactionDataList.Add("PlanPeriod", planPeriod);
                transactionReferenceObject = member;
            }
            else if (model.Type == TransactionType.Order)
            {
                var order = await _orderService.GetAsync(new OrderFilter { OrderCode = model.Reference ?? string.Empty });
                var orderAmount = order.TotalAmount;

                var paymentFee = model.Processor == TransactionProcessor.External ? Math.Round(((_appSettings.PaymentRate * 100) * orderAmount) / 100, 2, MidpointRounding.AwayFromZero) : 0;

                model.Amount = orderAmount;
                model.Fee = paymentFee;

                transactionMemberId = order.Customer.Id;
                transactionAccountName = order.Customer.FullName;
                transactionAccountEmail = order.Customer.Email;
                transactionAccountNumber = order.Customer.PhoneNumber;
                transactionAccountBalance = order.Customer.Balance;
                transactionReferenceObject = order;
            }
            else if (model.Type == TransactionType.Deposit)
            {
                var member = await _userService.FindByCodeAsync(model.Reference);
                var depositAmount = model.Amount;

                var paymentFee = model.Processor == TransactionProcessor.External ? Math.Round(((_appSettings.PaymentRate * 100) * depositAmount) / 100, 2, MidpointRounding.AwayFromZero) : 0;

                model.Amount = depositAmount;
                model.Fee = paymentFee;

                transactionMemberId = member.Id;
                transactionAccountName = member.FullName;
                transactionAccountEmail = member.Email;
                transactionAccountNumber = member.PhoneNumber;
                transactionAccountBalance = member.Balance;
                transactionReferenceObject = member;
            }

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var transaction = new Transaction
                    {
                        Reference = model.Reference,
                        Type = model.Type,
                        Processor = model.Processor,
                        Amount = model.Amount,
                        Fee = model.Fee,

                        Title = $"Payment for {model.Type.GetEnumText()}.",
                        Description = $"Payment for {model.Type.GetEnumText()}.",
                        Logo = Url.ContentLink(Url.Content("~/img/logo.png")),

                        MemberId = transactionMemberId,
                        AccountName = transactionAccountName,
                        AccountEmail = transactionAccountEmail,
                        AccountNumber = transactionAccountNumber,
                        AccountBalance = transactionAccountBalance,

                        TransactionCode = await _paymentProcessor.GenerateTransactionCodeAsync(),
                        AuthorizationCode = await _paymentProcessor.GetAuthorizationCodeAsync(),

                        IpAddress = await HttpContext.GetIpAddressAsync(),
                        UserAgent = Request.Headers["User-Agent"],

                        RedirectUrl = Url.ContentLink(returnUrl),
                        CallbackUrl = Url.ContentLink(Url.Action(nameof(CashInCallback), "Account")),
                    };

                    foreach (var transactionData in transactionDataList)
                        transaction.SetValue(transactionData.Key, transactionData.Value);

                    if (transaction.Type == TransactionType.Order)
                    {
                        var order = (Order)transactionReferenceObject;

                        if (order.Status == OrderStatus.Cancelled || order.Paid)
                        {
                            ModelState.AddModelError(string.Empty, "The order may have been paid, cancelled or deleted.");
                            return PartialView(model);
                        }
                    }

                    if (transaction.Processor == TransactionProcessor.Internal && transaction.Type == TransactionType.Order)
                    {
                        var order = (Order)transactionReferenceObject;
                        var transfer = await _userService.TransferAsync(order.Customer, order.Seller, transaction.Amount);

                        if (transfer.Success)
                        {
                            transaction.Status = TransactionStatus.Succeeded;
                            transaction.Issuer = "USE";
                            transaction.Mode = PaymentMode.Virtual;

                            await _transactionService.CreateAsync(transaction);

                            // TODO: Repetition, This code has properly been repeated elsewhere. Take note!
                            var orderStatus = order.Seller.StoreDeliveryRequired ?
                                EnumHelper.GetEnumValues<OrderStatus>().NextOrDefault(order.Status, OrderStatus.Complete) : OrderStatus.Complete;

                            order.Status = orderStatus;
                            order.Paid = true;
                            order.PaidOn = DateTimeOffset.UtcNow;

                            await _orderService.UpdateAsync(order);
                            await _userService.DepositAsync(order.Seller, transaction.Amount);

                            // Prepare order model
                            var orderModel = new OrderModel();
                            await _appService.PrepareModelAsync(orderModel, order);

                            // Send order placed email to the seller.
                            await _messageService.SendStoreOrderEmailAsync(orderModel);

                            // Send order status email to the customer.
                            await _messageService.SendCustomerOrderEmailAsync(orderModel);

                            TempData.AddAlert(AlertMode.Notify, AlertType.Success, "Payment successful!");
                            ViewData["Redirect"] = returnUrl;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, transfer.Message);
                        }
                    }
                    else
                    {
                        await _paymentProcessor.ProcessAsync(transaction);

                        if (transaction.Status == TransactionStatus.Pending)
                        {
                            await _transactionService.CreateAsync(transaction);
                            ViewData["Redirect"] = transaction.CheckoutUrl;
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Unable to process payment.");
                        }
                    }
                }
            }

            return PartialView(model);
        }

        [Authorize]
        public async Task<IActionResult> CashInCallback()
        {
            var member = await HttpContext.GetMemberAsync();
            var transactionCode = await _paymentProcessor.GetTransactionCodeAsync();

            var transactions = await _transactionService.ListAsync(new TransactionFilter
            {
                MemberId = member.Id,
                Status = TransactionStatus.Pending,
            });
            var transaction = transactions.FirstOrDefault(x => x.TransactionCode == transactionCode);
            var previousTransactions = transactions.Where(x => x.TransactionCode != transactionCode);

            foreach (var previousTransaction in previousTransactions)
            {
                previousTransaction.Status = TransactionStatus.Failed;
                await _transactionService.UpdateAsync(previousTransaction);
            }

            if (transaction != null)
            {
                await _paymentProcessor.VerifyAsync(transaction);
                await _transactionService.UpdateAsync(transaction);

                if (transaction.Status == TransactionStatus.Succeeded)
                {
                    if (transaction.Type == TransactionType.Order)
                    {
                        var order = await _orderService.GetAsync(new OrderFilter { OrderCode = transaction.Reference ?? string.Empty });

                        // TODO: Repetition, This code has properly been repeated elsewhere. Take note!
                        var orderStatus = order.Seller.StoreDeliveryRequired ?
                            EnumHelper.GetEnumValues<OrderStatus>().NextOrDefault(order.Status, OrderStatus.Complete) : OrderStatus.Complete;

                        order.Status = orderStatus;
                        order.Paid = true;
                        order.PaidOn = DateTimeOffset.UtcNow;

                        await _orderService.UpdateAsync(order);
                        await _userService.DepositAsync(order.Seller, transaction.Amount);

                        // Prepare order model
                        var orderModel = new OrderModel();
                        await _appService.PrepareModelAsync(orderModel, order);

                        // Send order placed email to the seller.
                        await _messageService.SendStoreOrderEmailAsync(orderModel);

                        // Send order status email to the customer.
                        await _messageService.SendCustomerOrderEmailAsync(orderModel);
                    }
                    else if (transaction.Type == TransactionType.Deposit)
                    {
                        var referenceUser = await _userService.FindByCodeAsync(transaction.Reference);
                        await _userService.DepositAsync(referenceUser, transaction.Amount);
                    }
                    else if (transaction.Type == TransactionType.Subscription)
                    {
                        var planType = transaction.GetValue<PlanType>("PlanType");
                        var planPeriod = transaction.GetValue<PlanPeriod>("PlanPeriod");

                        var referenceUser = await _userService.FindByCodeAsync(transaction.Reference);

                        // TODO: Repetition, This code has properly been repeated elsewhere. Take note!
                        var planEnded = referenceUser.StorePlanEnded;
                        var planEndedOn = referenceUser.StorePlanEndedOn;
                        var planRenewedOn = planEnded ? DateTimeOffset.UtcNow.AddMonths((int)planPeriod) :
                                                                  planEndedOn.AddMonths((int)planPeriod);
                        referenceUser.StorePlanType = planType;
                        referenceUser.StorePlanPeriod = planPeriod;
                        referenceUser.StorePlanEndedOn = planRenewedOn;
                        referenceUser.StorePlanUpdated = true;
                        referenceUser.StorePlanUpdatedOn = DateTimeOffset.UtcNow;
                        await _userService.UpdateAsync(referenceUser);
                    }

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, "Payment successful!");
                }
                else
                {
                    TempData.AddAlert(AlertMode.Notify, AlertType.Error, "Unable to process payment.");
                }

                return Redirect(transaction.RedirectUrl);
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, "Unable to process payment.");
            }

            return LocalRedirect(Url.Content("~/"));
        }

        [Authorize]
        public async Task<IActionResult> CashOut(CashOutModel model)
        {
            var member = await HttpContext.GetMemberAsync();
            var issuers = await _paymentProcessor.GetIssuersAsync();

            model.MobileIssuerOptions.AddRange(SelectListHelper.GetSelectList(elements: issuers.Where(x => x.Mode == PaymentMode.Mobile), x => new SelectListItem<PaymentIssuer>(x.Name, x.Code)));
            model.MobileIssuer = member.MobileIssuer;
            model.MobileNumber = member.MobileNumber;

            model.BankIssuerOptions.AddRange(SelectListHelper.GetSelectList(elements: issuers.Where(x => x.Mode == PaymentMode.Bank), x => new SelectListItem<PaymentIssuer>(x.Name, x.Code)));
            model.BankIssuer = member.BankIssuer;
            model.BankNumber = member.BankNumber;

            model.Fee = Math.Round(((_appSettings.PaymentRate * 100) * model.Amount) / 100, 2, MidpointRounding.AwayFromZero);

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    var canWithdraw = _userService.CanWithdraw(member, model.Amount + model.Fee);

                    if (canWithdraw.Success)
                    {
                        if (!Request.IsCallBack())
                        {
                            ViewData["Persistent"] = true;
                        }
                        else
                        {
                            var transaction = new Transaction
                            {
                                Title = $"Payment for Withdrawal.",
                                Description = $"Payment for Withdrawal.",
                                Logo = Url.ContentLink(Url.Content("~/img/logo.png")),
                                MemberId = member.Id,
                                Reference = model.Reference,
                                AccountName = member.FullName,
                                AccountEmail = member.Email,
                                AccountNumber = model.AccountNumber,
                                AccountBalance = member.Balance,

                                TransactionCode = await _paymentProcessor.GenerateTransactionCodeAsync(),
                                AuthorizationCode = await _paymentProcessor.GetAuthorizationCodeAsync(),

                                Issuer = model.Issuer,
                                Mode = model.Mode,
                                Processor = TransactionProcessor.External,

                                Amount = model.Amount,
                                Fee = model.Fee,

                                IpAddress = await HttpContext.GetIpAddressAsync(),
                                UserAgent = Request.Headers["User-Agent"],
                                Type = TransactionType.Withdrawal,
                            };

                            await _paymentProcessor.ProcessAsync(transaction);

                            if (transaction.Status == TransactionStatus.Pending)
                                await _paymentProcessor.VerifyAsync(transaction);

                            if (transaction.Status == TransactionStatus.Succeeded)
                            {
                                await _transactionService.CreateAsync(transaction);
                                await _userService.WithdrawAsync(member, transaction.Amount + transaction.Fee);

                                TempData.AddAlert(AlertMode.Notify, AlertType.Success, "Payment successful!");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Unable to process payment.");
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, canWithdraw.Message);
                    }
                }
            }

            return PartialView(model);
        }

        public IActionResult Lockout()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        [HttpGet, ModelState]
        public async Task<IActionResult> EditProfile()
        {
            var member = await HttpContext.GetMemberAsync();
            var model = new ProfileEditModel();
            await _appService.PrepareModelAsync(model, member);
            return View(model);
        }

        [Authorize]
        [HttpPost, ModelState]
        public async Task<IActionResult> EditProfile(ProfileEditModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var member = await HttpContext.GetMemberAsync();

                await _appService.PrepareProfileAsync(member, model);
                var result = await _userService.UpdateAsync(member);

                if (result.Succeeded)
                {
                    await SaveProfileImageAsync(member, model);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"Profile was updated.");
                }
                else
                {
                    ModelState.AddIdentityResult(result);
                }
            }

            return RedirectToAction(nameof(EditProfile), new { returnUrl });
        }

        [Authorize]
        public async Task<IActionResult> EditPayment(PaymentEditModel model)
        {
            var user = await HttpContext.GetMemberAsync();
            var issuers = await _paymentProcessor.GetIssuersAsync();

            model.MobileIssuerOptions.AddRange(SelectListHelper.GetSelectList(elements: issuers.Where(x => x.Mode == PaymentMode.Mobile), x => new SelectListItem<PaymentIssuer>(x.Name, x.Code)));
            model.BankIssuerOptions.AddRange(SelectListHelper.GetSelectList(elements: issuers.Where(x => x.Mode == PaymentMode.Bank), x => new SelectListItem<PaymentIssuer>(x.Name, x.Code)));

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();

                if (model.Mode == PaymentMode.Mobile)
                {
                    model.MobileIssuer = user.MobileIssuer;
                    model.MobileNumber = user.MobileNumber;
                }
                else if (model.Mode == PaymentMode.Bank)
                {
                    model.BankIssuer = user.BankIssuer;
                    model.BankNumber = user.BankNumber;
                }
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    if (model.Mode == PaymentMode.Mobile)
                    {
                        user.MobileIssuer = model.MobileIssuer;
                        user.MobileNumber = model.MobileNumber;
                    }
                    else if (model.Mode == PaymentMode.Bank)
                    {
                        user.BankIssuer = model.BankIssuer;
                        user.BankNumber = model.BankNumber;
                    }

                    var result = await _userService.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"Payment details was updated.");
                    }
                    else
                    {
                        ModelState.AddIdentityResult(result);
                    }
                }
            }

            return PartialView(nameof(EditPayment), model);
        }

        [Authorize]
        public IActionResult ManageWallet()
        {
            return View();
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(EditProfile));
        }

        [NonAction]
        private async Task SaveProfileImageAsync(User user, ProfileEditModel model)
        {
            var fileClient = HttpContext.RequestServices.GetRequiredService<IFileClient>();
            var imageProcessor = HttpContext.RequestServices.GetRequiredService<IImageProcessor>();

            await _appService.PrepareMediaAsync(new[] { user.UserImage }, new[] { model.UserImage },
                createMedia: async (image) =>
                {
                    image.DirectoryName = user.Id.ToString();

                    var source = await fileClient.GetAsync(image.DirectoryName, image.FileName);

                    if (source != null)
                    {
                        var (imageWidth, imageHeight) = await imageProcessor.GetImageSizeAsync(source);

                        image.Width = imageWidth;
                        image.Height = imageHeight;
                        image.FileSize = source.Length;

                        await _mediaService.CreateAsync(image);

                        user.UserImageId = image.Id;
                        await _userService.UpdateAsync(user);
                    }
                },
                updateMedia: async (media) =>
                {
                    await _mediaService.UpdateAsync(media);
                },
                deleteMedia: async (media) =>
                {
                    user.UserImageId = null;
                    user.UserImage = null;

                    await _userService.UpdateAsync(user);
                    await _mediaService.DeleteAsync(media);
                });
        }

        public IActionResult PlanPricing()
        {
            return View();
        }

        public IActionResult PlanEnded()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> SelectPlan(PlanSelectModel model, string returnUrl)
        {
            model.PlanTypeOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.PlanType));
            model.PlanPeriodOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.PlanPeriod));

            if (HttpMethods.IsGet(Request.Method))
            {
                ModelState.Clear();
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (ModelState.IsValid)
                {
                    var member = await HttpContext.GetMemberAsync();

                    returnUrl = XQueryHelpers.RemoveQueryString(returnUrl, "modalUrl") ?? Url.Content("~/");
                    returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : throw new InvalidOperationException("The supplied URL is not local.");

                    ViewData["Redirect"] = QueryHelpers.AddQueryString(returnUrl, "modalUrl",
                        Url.Action(nameof(CashIn), new
                        {
                            reference = member.UserCode,
                            type = TransactionType.Subscription,
                            planType = model.PlanType,
                            planPeriod = model.PlanPeriod,
                            returnUrl = Url.Action("Index", "Home", new { area = "Portal" })
                        }));
                }
            }

            return PartialView(model);
        }
    }
}