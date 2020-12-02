using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
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
using Serilog;

namespace Neimart.Web.Areas.Portal.Controllers
{
    [Area("Portal")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Seller)]
    [PlanRequired]
    public class ProductsController : Controller
    {
        private readonly AppService _appService;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly TagService _tagService;
        private readonly MediaService _mediaService;
        private readonly AppSettings _appSettings;

        public ProductsController(IServiceProvider services)
        {
            _appService = services.GetRequiredService<AppService>();
            _productService = services.GetRequiredService<ProductService>();
            _categoryService = services.GetRequiredService<CategoryService>();
            _tagService = services.GetRequiredService<TagService>();
            _mediaService = services.GetRequiredService<MediaService>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
        }


        [HttpGet]
        public async Task<IActionResult> Index(int page, ProductFilter filter)
        {
            var seller = await HttpContext.GetMemberAsync();

            filter.SellerId = seller.Id;

            var products = await _productService.GetQuery(filter).ToPageableAsync(page, _appSettings.PageDefaultSize);

            var model = new ProductListModel
            {
                Filter = filter
            };

            await _appService.PrepareModelAsync(model, products, seller);

            return View(model);
        }

        [HttpGet, ModelState]
        public async Task<IActionResult> Add()
        {
            var seller = await HttpContext.GetMemberAsync();
            var model = new ProductEditModel();

            await _appService.PrepareModelAsync(model, null, seller);
            return View(nameof(Edit), model);
        }

        [HttpPost, ModelState]
        public async Task<IActionResult> Add(ProductEditModel model)
        {
            if (ModelState.IsValid)
            {
                var seller = await HttpContext.GetMemberAsync();
                var product = new Product();
                await _appService.PrepareProductAsync(product, model);

                product.SellerId = seller.Id;
                await _productService.CreateAsync(product);

                await SaveProductImages(product, model);
                await SaveProductDocument(product, model);
                await SaveProductTags(product, model);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{product.Name}\" product was added.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet, ModelState]
        public async Task<IActionResult> Edit(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var product = await _productService.GetAsync(new ProductFilter() { SellerId = seller.Id, ProductId = id });

            if (product != null)
            {
                var model = new ProductEditModel();

                await _appService.PrepareModelAsync(model, product, seller);
                return View(nameof(Edit), model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ModelState]
        public async Task<IActionResult> Edit(ProductEditModel model)
        {
            if (ModelState.IsValid)
            {
                var seller = await HttpContext.GetMemberAsync();
                var product = await _productService.GetAsync(new ProductFilter() { SellerId = seller.Id, ProductId = model.Id });

                if (product != null)
                {
                    await _appService.PrepareProductAsync(product, model);

                    product.SellerId = seller.Id;
                    await _productService.UpdateAsync(product);

                    await SaveProductImages(product, model);
                    await SaveProductDocument(product, model);
                    await SaveProductTags(product, model);

                    TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{product.Name}\" product was updated.");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var product = await _productService.GetAsync(new ProductFilter() { SellerId = seller.Id, ProductId = id });

            if (product != null)
            {
                await _productService.DeleteAsync(product);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{product.Name}\" product was deleted.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Product does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Preview(long id)
        {
            var seller = await HttpContext.GetMemberAsync();
            var product = await _productService.GetAsync(new ProductFilter() { SellerId = seller.Id, ProductId = id });

            if (product != null)
            {
                return RedirectToAction("Products", "Store", new { area = string.Empty, storeSlug = seller.StoreSlug, slug = product.Slug });
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Product does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Publish(long id, bool toggle)
        {
            var seller = await HttpContext.GetMemberAsync();
            var product = await _productService.GetAsync(new ProductFilter() { SellerId = seller.Id, ProductId = id });
            var toggleName = toggle ? "Published" : "Unpublished";

            if (product != null)
            {
                product.Published = toggle;
                await _productService.UpdateAsync(product);

                TempData.AddAlert(AlertMode.Notify, AlertType.Success, $"\"{product.Name}\" product was {toggleName.ToLowerInvariant()}.");
            }
            else
            {
                TempData.AddAlert(AlertMode.Notify, AlertType.Error, $"Product does not exist.");
            }

            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        private async Task SaveProductImages(Product product, ProductEditModel model)
        {
            var fileClient = HttpContext.RequestServices.GetRequiredService<IFileClient>();
            var imageProcessor = HttpContext.RequestServices.GetRequiredService<IImageProcessor>();

            await _appService.PrepareMediaAsync(product.Images, model.Images,
                createMedia: async (image) =>
                {
                    image.DirectoryName = product.SellerId.ToString();


                    var source = await fileClient.GetAsync(image.DirectoryName, image.FileName);

                    if (source != null)
                    {
                        var (imageWidth, imageHeight) = await imageProcessor.GetImageSizeAsync(source);

                        image.ProductId = product.Id;
                        image.Width = imageWidth;
                        image.Height = imageHeight;
                        image.FileSize = source.Length;

                        await _mediaService.CreateAsync(image);
                    }
                },
                updateMedia: async (image) =>
                {
                    await _mediaService.UpdateAsync(image);
                },
                deleteMedia: async (image) =>
                {
                    await _mediaService.DeleteAsync(image);
                });
        }

        [NonAction]
        private async Task SaveProductDocument(Product product, ProductEditModel model)
        {
            var fileClient = HttpContext.RequestServices.GetRequiredService<IFileClient>();
            var imageProcessor = HttpContext.RequestServices.GetRequiredService<IImageProcessor>();

            await _appService.PrepareMediaAsync(new[] { product.Document }, new[] { model.Document },
                createMedia: async (document) =>
                {
                    document.DirectoryName = product.SellerId.ToString();

                    var source = await fileClient.GetAsync(document.DirectoryName, document.FileName);

                    if (source != null)
                    {
                        document.FileSize = source.Length;

                        await _mediaService.CreateAsync(document);

                        product.DocumentId = document.Id;
                        await _productService.UpdateAsync(product);
                    }
                },
                updateMedia: async (document) =>
                {
                    await _mediaService.UpdateAsync(document);
                },
                deleteMedia: async (document) =>
                {
                    product.DocumentId = null;
                    product.Document = null;

                    await _productService.UpdateAsync(product);
                    await _mediaService.DeleteAsync(document);
                });
        }

        [NonAction]
        private async Task SaveProductTags(Product product, ProductEditModel model)
        {
            var tagObjects = product.Tags.ToList();

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
                    var tagObject = new Tag
                    {
                        Name = tagName,
                        ProductId = product.Id,
                        Slug = tagSlug
                    };

                    tagObjects.Add(tagObject);
                    await _tagService.CreateAsync(tagObject);
                }
            }
        }
    }
}