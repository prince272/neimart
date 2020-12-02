using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
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
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Models;
using Neimart.Web.Services;

namespace Neimart.Web.Areas.Portal.Controllers
{
    [Area("Portal")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Seller)]
    [PlanRequired]
    public class CategoriesController : Controller
    {
        private readonly AppService _appService;
        private readonly MediaService _mediaService;
        private readonly CategoryService _categoryService;
        private readonly TagService _tagService;
        private readonly AppSettings _appSettings;

        public CategoriesController(IServiceProvider services)
        {
            _appService = services.GetRequiredService<AppService>();
            _mediaService = services.GetRequiredService<MediaService>();
            _categoryService = services.GetRequiredService<CategoryService>();
            _tagService = services.GetRequiredService<TagService>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page, CategoryFilter filter)
        {
            var seller = await HttpContext.GetMemberAsync();

            filter.SellerId = seller.Id;

            var categories = await _categoryService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new CategoryListModel
            {
                Filter = filter
            };

            await _appService.PrepareModelAsync(model, categories);

            return View(model);
        }

        [HttpGet, ModelState]
        public async Task<IActionResult> Add()
        {
            var seller = await HttpContext.GetMemberAsync();
            var model = new CategoryEditModel();

            await _appService.PrepareModelAsync(model, null, seller);
            return View(nameof(Edit), model);
        }

        [HttpPost, ModelState]
        public async Task<IActionResult> Add(CategoryEditModel model)
        {
            if (ModelState.IsValid)
            {
                var seller = await HttpContext.GetMemberAsync();
                var category = new Category();
                await _appService.PrepareCategoryAsync(category, model);

                category.SellerId = seller.Id;
                await _categoryService.CreateAsync(category);
                await SaveCategoryImage(category, model);
                await SaveCategoryTags(category, model);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{category.Name}\" category was added.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet, ModelState]
        public async Task<IActionResult> Edit(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var category = await _categoryService.GetAsync(new CategoryFilter() { SellerId = seller.Id, CategoryId = id });

            if (category != null)
            {
                var model = new CategoryEditModel();

                await _appService.PrepareModelAsync(model, category, seller);
                return View(nameof(Edit), model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ModelState]
        public async Task<IActionResult> Edit(CategoryEditModel model)
        {
            if (ModelState.IsValid)
            {
                var seller = await HttpContext.GetMemberAsync();
                var category = await _categoryService.GetAsync(new CategoryFilter() { SellerId = seller.Id, CategoryId = model.Id });

                if (category != null)
                {
                    await _appService.PrepareCategoryAsync(category, model);

                    category.SellerId = seller.Id;
                    await _categoryService.UpdateAsync(category);
                    await SaveCategoryImage(category, model);
                    await SaveCategoryTags(category, model);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{category.Name}\" category was updated.");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var category = await _categoryService.GetAsync(new CategoryFilter() { SellerId = seller.Id, CategoryId = id });

            if (category != null)
            {
                await _categoryService.DeleteAsync(category);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{category.Name}\" category was deleted.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Category does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Publish(long id, bool toggle)
        {
            var seller = await HttpContext.GetMemberAsync();
            var category = await _categoryService.GetAsync(new CategoryFilter() { SellerId = seller.Id, CategoryId = id });
            var toggleName = toggle ? "Published" : "Unpublished";

            if (category != null)
            {
                category.Published = toggle;
                await _categoryService.UpdateAsync(category);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{category.Name}\" category was {toggleName.ToLowerInvariant()}.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Category does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        private async Task SaveCategoryImage(Category category, CategoryEditModel model)
        {
            var fileClient = HttpContext.RequestServices.GetRequiredService<IFileClient>();
            var imageProcessor = HttpContext.RequestServices.GetRequiredService<IImageProcessor>();

            await _appService.PrepareMediaAsync(new[] { category.Image }, new[] { model.Image },
                createMedia: async (image) =>
                {
                    image.DirectoryName = category.SellerId.ToString();

                    var source = await fileClient.GetAsync(image.DirectoryName, image.FileName);

                    if (source != null)
                    {
                        var (imageWidth, imageHeight) = await imageProcessor.GetImageSizeAsync(source);

                        image.Width = imageWidth;
                        image.Height = imageHeight;
                        image.FileSize = source.Length;

                        await _mediaService.CreateAsync(image);

                        category.ImageId = image.Id;
                        await _categoryService.UpdateAsync(category);
                    }
                },
                updateMedia: async (image) =>
                {
                    await _mediaService.UpdateAsync(image);
                },
                deleteMedia: async (image) =>
                {
                    category.ImageId = null;
                    category.Image = null;

                    await _categoryService.UpdateAsync(category);
                    await _mediaService.DeleteAsync(image);
                });
        }

        [NonAction]
        private async Task SaveCategoryTags(Category category, CategoryEditModel model)
        {
            var tagObjects = category.Tags.ToList();

            // Ensure the newTageNames are safe to access.
            var newTagNames = model.TagNames?.Where(x => !string.IsNullOrWhiteSpace(x)) ?? Enumerable.Empty<string>();
            var oldTagNames = tagObjects.Select(x => x.Name).ToList();

            var createTagNames = newTagNames.Except(oldTagNames);
            var deleteTagNames = oldTagNames.Except(newTagNames);
            bool tagPredicate(Tag tag, string slug, string name) => tag.Slug == slug || tag.Name == name;

            foreach (var tagName in deleteTagNames)
            {
                var tagSlug = SanitizerHelper.GenerateSlug(tagName);
                var tagObject = tagObjects.FirstOrDefault(x => tagPredicate(x, tagSlug, tagName));

                if (tagObject != null)
                {
                    tagObjects.Remove(tagObject);
                    await _tagService.DeleteAsync(tagObject);
                }
            }

            foreach (var tagName in createTagNames)
            {
                var tagSlug = SanitizerHelper.GenerateSlug(tagName);
               
                if (!tagObjects.Any(x => tagPredicate(x, tagSlug, tagName)))
                {
                    var tag = new Tag
                    {
                        Name = tagName,
                        CategoryId = category.Id,
                        Slug = tagSlug
                    };

                    tagObjects.Add(tag);
                    await _tagService.CreateAsync(tag);
                }
            }
        }
    }
}