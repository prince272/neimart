using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Events;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Storing;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Models;
using Neimart.Web.Models.Account;
using Neimart.Web.Services;

namespace Neimart.Web.Areas.Portal.Controllers
{
    [Area("Portal")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Seller)]
    [PlanRequired]
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly SignInService _signInService;
        private readonly MediaService _mediaService;
        private readonly MessageService _messageService;
        private readonly AppService _appService;

        public AccountController(UserService userService, SignInService signInService, MediaService mediaService, MessageService messageService, AppService appService)
        {
            _userService = userService;
            _signInService = signInService;
            _mediaService = mediaService;
            _messageService = messageService;
            _appService = appService;
        }


        [HttpGet, ModelState]
        public async Task<IActionResult> EditStore()
        {
            var seller = await HttpContext.GetMemberAsync();
            var model = new StoreEditModel();
            await _appService.PrepareModelAsync(model, seller);
            return View(model);
        }

        [HttpPost, ModelState]
        public async Task<IActionResult> EditStore(StoreEditModel model)
        {
            if (ModelState.IsValid)
            {
                var seller = await HttpContext.GetMemberAsync();
                var firstStoreSetup = false;

                await _appService.PrepareStoreAsync(seller, model);

                if (!seller.StoreSetup)
                {
                    seller.StoreSetup = true;
                    firstStoreSetup = true;
                }

                var result = await _userService.CheckStoreSlugAsync(seller);
                result = result.Succeeded ? await _userService.UpdateAsync(seller) : result;

                if (result.Succeeded)
                {
                    if (firstStoreSetup)
                    {
                        await _messageService.SendEmailAsync(
                                  messageRole: MessageRole.Notification,
                                  messageType: MessageType.CompanyWelcome,
                                  messageDisplay: "Neimart",
                                  email: seller.Email,
                                  subject: "You have just started a new business",
                                  model: new ValueTuple<User, object>(seller, null));
                    }

                    await SaveStoreLogoAsync(seller, model);
                    await SaveStoreDocumentAsync(seller, model);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"Store was updated.");
                }
                else
                {
                    ModelState.AddIdentityResult(result);
                }
            }

            return RedirectToAction();
        }

        [NonAction]
        private async Task SaveStoreLogoAsync(User user, StoreEditModel model)
        {
            var fileClient = HttpContext.RequestServices.GetRequiredService<IFileClient>();
            var imageProcessor = HttpContext.RequestServices.GetRequiredService<IImageProcessor>();

            await _appService.PrepareMediaAsync(new[] { user.StoreLogo }, new[] { model.StoreLogo },
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

                        user.StoreLogoId = image.Id;
                        await _userService.UpdateAsync(user);
                    }
                },
                updateMedia: async (media) =>
                {
                    await _mediaService.UpdateAsync(media);
                },
                deleteMedia: async (media) =>
                {
                    user.StoreLogoId = null;
                    user.StoreLogo = null;

                    await _userService.UpdateAsync(user);
                    await _mediaService.DeleteAsync(media);
                });
        }

        [NonAction]
        private async Task SaveStoreDocumentAsync(User user, StoreEditModel model)
        {
            // The user is already approved and isn't allowed to update his store document.
            if (user.StoreAccess == StoreAccess.Approved)
            {
                return;
            }

            var fileClient = HttpContext.RequestServices.GetRequiredService<IFileClient>();
            var imageProcessor = HttpContext.RequestServices.GetRequiredService<IImageProcessor>();

            await _appService.PrepareMediaAsync(new[] { user.StoreDocument }, new[] { model.StoreDocument },
                createMedia: async (document) =>
                {
                    document.DirectoryName = user.Id.ToString();

                    var source = await fileClient.GetAsync(document.DirectoryName, document.FileName);

                    if (source != null)
                    {
                        document.FileSize = source.Length;

                        await _mediaService.CreateAsync(document);

                        user.StoreDocumentId = document.Id;
                        await _userService.UpdateAsync(user);
                    }
                },
                updateMedia: async (document) =>
                {
                    await _mediaService.UpdateAsync(document);
                },
                deleteMedia: async (document) =>
                {
                    user.StoreDocumentId = null;
                    user.StoreDocument = null;

                    await _userService.UpdateAsync(user);
                    await _mediaService.DeleteAsync(document);
                });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateNote(NoteTemplateType? noteTemplateType)
        {
            var seller = await HttpContext.GetMemberAsync();

            if (noteTemplateType != null)
            {
                var content = await _appService.GenerateNoteAsync(seller, noteTemplateType.Value);
                return Content(content);
            }

            return BadRequest();
        }
    }
}