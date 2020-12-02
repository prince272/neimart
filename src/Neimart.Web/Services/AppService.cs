using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Caching;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Models;
using Neimart.Web.Models.Account;
using Newtonsoft.Json;

namespace Neimart.Web.Services
{
    public class AppService
    {
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICacheManager _cacheManager;
        private readonly IRazorViewRenderer _razorViewRenderer;
        private readonly ProductService _productService;
        private readonly TagService _tagService;
        private readonly CategoryService _categoryService;
        private readonly BannerService _bannerService;
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly OrderItemService _orderItemService;
        private readonly ReviewService _reviewService;

        public AppService(IServiceProvider services)
        {
            _mapper = services.GetRequiredService<IMapper>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
            _webHostEnvironment = services.GetRequiredService<IWebHostEnvironment>();
            _razorViewRenderer = services.GetRequiredService<IRazorViewRenderer>();
            _cacheManager = services.GetRequiredService<ICacheManager>();
            _productService = services.GetRequiredService<ProductService>();
            _tagService = services.GetRequiredService<TagService>();
            _categoryService = services.GetRequiredService<CategoryService>();
            _bannerService = services.GetRequiredService<BannerService>();
            _cartService = services.GetRequiredService<CartService>();
            _orderService = services.GetRequiredService<OrderService>();
            _orderItemService = services.GetRequiredService<OrderItemService>();
            _reviewService = services.GetRequiredService<ReviewService>();
        }

        public Task PrepareModelAsync(TransactionListModel model, IPageable<Transaction> transactions)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (transactions == null) throw new ArgumentNullException(nameof(transactions));

            foreach (var transaction in transactions)
            {
                var transactionModel = new TransactionModel
                {
                    Transaction = transaction
                };
                model.Items.Add(transactionModel);
            }

            model.ModeOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.Filter.Mode, defaultText: "All"));
            model.StatusOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.Filter.Status, defaultText: "All"));
            model.ProcessorOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.Filter.Processor, defaultText: "All"));
            model.TypeOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.Filter.Type, defaultText: "All"));

            model.Page = transactions.Page;
            model.PageSize = transactions.PageSize;
            model.PageFrom = transactions.PageFrom;
            model.PageTo = transactions.PageTo;
            model.TotalPages = transactions.TotalPages;
            model.TotalItems = transactions.TotalItems;

            return Task.CompletedTask;
        }

        public Task PrepareModelAsync(TransactionModel model, Transaction transaction)
        {
            model.Transaction = transaction;
            return Task.CompletedTask;
        }

        public Task PrepareAddressAsync(Address address, AddressEditModel model)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (model == null) throw new ArgumentNullException(nameof(model));

            address = _mapper.Map(model, address);

            return Task.CompletedTask;
        }

        public async Task PrepareModelAsync(UserListModel model, IPageable<User> users, Func<UserModel, User, Task> prepareChildModel = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (users == null) throw new ArgumentNullException(nameof(users));

            foreach (var user in users)
            {
                var userModel = new UserModel
                {
                    User = user
                };

                if (prepareChildModel != null)
                    await prepareChildModel(userModel, user);

                model.Items.Add(userModel);
            }

            model.StoreCategoryOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.Filter.StoreCategory, defaultText: "All"));
            model.StoreSetupOptions.AddRange(SelectListHelper.GetBoolSelectList("Completed", "Not Completed", selectedBool: model.Filter.StoreSetup, defaultText: "Any"));

            model.Page = users.Page;
            model.PageSize = users.PageSize;
            model.PageFrom = users.PageFrom;
            model.PageTo = users.PageTo;
            model.TotalPages = users.TotalPages;
            model.TotalItems = users.TotalItems;
        }

        public Task PrepareModelAsync(UserModel model, User user)
        {
            model.User = user;
            return Task.CompletedTask;
        }

        public Task PrepareReviewAsync(Review review, ReviewEditModel model)
        {
            if (review == null) throw new ArgumentNullException(nameof(review));
            if (model == null) throw new ArgumentNullException(nameof(model));

            review = _mapper.Map(model, review);

            return Task.CompletedTask;
        }

        public Task PrepareProductAsync(Product product, ProductEditModel model)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (model == null) throw new ArgumentNullException(nameof(model));

            product = _mapper.Map(model, product);
            product.Description = SanitizerHelper.SanitizeHtml(model.Description);

            return Task.CompletedTask;
        }

        public Task PrepareCategoryAsync(Category category, CategoryEditModel model)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            if (model == null) throw new ArgumentNullException(nameof(model));

            category = _mapper.Map(model, category);

            return Task.CompletedTask;
        }

        public Task PrepareBannerAsync(Banner banner, BannerEditModel model)
        {
            if (banner == null) throw new ArgumentNullException(nameof(banner));
            if (model == null) throw new ArgumentNullException(nameof(model));

            banner = _mapper.Map(model, banner);
            banner.Permalink = SanitizerHelper.AppendUrlPath(model.Permalink);

            return Task.CompletedTask;
        }



        public async Task PrepareModelAsync(ProductListModel model, IPageable<Product> products, User seller, User customer = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (products == null) throw new ArgumentNullException(nameof(products));

            foreach (var product in products)
            {
                var productModel = new ProductModel();

                await PrepareModelAsync(productModel, product);

                if (customer != null)
                {
                    productModel.IsAddedToCart = await _cartService.GetQuery(new CartFilter { CustomerId = customer.Id, SellerId = seller.Id, ProductId = product.Id, Type = CartType.Cart }).AnyAsync();
                    productModel.IsAddedToWishlist = await _cartService.GetQuery(new CartFilter { CustomerId = customer.Id, SellerId = seller.Id, ProductId = product.Id, Type = CartType.Wishlist }).AnyAsync();
                }

                // NOTE: "StoreController.cs" with the line number 299 for code changes.
                productModel.ReviewEvaluation = await _reviewService.EvaluateAsync(new ReviewFilter
                {
                    Approved = true,
                    SellerId = seller.Id,
                    ProductId = product.Id
                });

                model.Items.Add(productModel);
            }

            model.LowestMinPrice = 0;
            model.HighestMaxPrice = (int)Math.Min(_appSettings.CurrencyMaxValue, int.MaxValue);

            model.SortOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.Filter.Sort, defaultText: "Any"));
            model.StockOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.Filter.Stock, defaultText: "All"));
            model.PublishedOptions.AddRange(SelectListHelper.GetBoolSelectList("Published", "Unpublished", selectedBool: model.Filter.Published, defaultText: "All"));

            model.RatingOptions.AddRange(SelectListHelper.GetSelectList(Enumerable.Range(1, 4).Reverse(),
                (x) => new SelectListItem<int>(text: $"{"star".ToQuantity(x)} or more", value: x.ToString(), selected: x == model.Filter.Rating)));

            model.DiscountOptions.AddRange(SelectListHelper.GetSelectList(Enumerable.Range(1, 4).Select(x => x * 20).Reverse(),
                (x) => new SelectListItem<int>(text: $"{x}% or more", value: x.ToString(), selected: x == model.Filter.Discount)));

            var categories = await _categoryService.ListAsync(new CategoryFilter() { SellerId = seller.Id, Published = true });
            model.CategoryOptions.AddRange(SelectListHelper.GetSelectList(categories, x => new SelectListItem<Category>(text: x.Name, value: x.Slug, x.Slug == model.Filter.Search), defaultText: "All"));

            model.Page = products.Page;
            model.PageSize = products.PageSize;
            model.PageFrom = products.PageFrom;
            model.PageTo = products.PageTo;
            model.TotalPages = products.TotalPages;
            model.TotalItems = products.TotalItems;
        }

        public async Task PrepareModelAsync(CartListModel model, IPageable<Cart> carts)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (carts == null) throw new ArgumentNullException(nameof(carts));

            foreach (var cart in carts)
            {
                var cartModel = _mapper.Map(cart, new CartModel());

                cartModel.ProductModel = new ProductModel();
                await PrepareModelAsync(cartModel.ProductModel, cart.Product);

                cartModel.Evaluation = await _cartService.EvaluateAsync(cart);

                int minQuantity = 1;
                int maxQuantity = _appSettings.QuantityMaxValue;
                int existingQuantity = cart.Quantity;

                var quantities = Enumerable.Range(minQuantity, maxQuantity);

                cartModel.QuantityOptions.AddRange(SelectListHelper.GetSelectList(quantities, (newQuantity) =>
                {
                    int remainingQuantity = newQuantity - existingQuantity;
                    bool selectedQuantity = (newQuantity == existingQuantity);
                    return new SelectListItem<int>(text: newQuantity.ToString(), value: remainingQuantity.ToString(), selected: selectedQuantity);
                }));

                model.Items.Add(cartModel);
            }

            model.CartListEvaluation = await _cartService.EvaluateAsync(carts);

            model.Page = carts.Page;
            model.PageSize = carts.PageSize;
            model.PageFrom = carts.PageFrom;
            model.PageTo = carts.PageTo;
            model.TotalPages = carts.TotalPages;
            model.TotalItems = carts.TotalItems;
        }

        public async Task PrepareModelAsync(OrderListModel model, IPageable<Order> orders)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (orders == null) throw new ArgumentNullException(nameof(orders));

            foreach (var order in orders)
            {
                var orderModel = new OrderModel();
                await PrepareModelAsync(orderModel, order);

                model.Items.Add(orderModel);
            }

            model.StatusOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.Filter.Status, defaultText: "All"));

            model.Page = orders.Page;
            model.PageSize = orders.PageSize;
            model.PageFrom = orders.PageFrom;
            model.PageTo = orders.PageTo;
            model.TotalPages = orders.TotalPages;
            model.TotalItems = orders.TotalItems;
        }

        public Task PrepareModelAsync(CategoryListModel model, IPageable<Category> categories)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (categories == null) throw new ArgumentNullException(nameof(categories));

            foreach (var category in categories)
            {
                var categoryModel = new CategoryModel
                {
                    Category = category
                };
                model.Items.Add(categoryModel);
            }

            model.Page = categories.Page;
            model.PageSize = categories.PageSize;
            model.PageFrom = categories.PageFrom;
            model.PageTo = categories.PageTo;
            model.TotalPages = categories.TotalPages;
            model.TotalItems = categories.TotalItems;

            return Task.CompletedTask;
        }

        public Task PrepareModelAsync(BannerListModel model, IPageable<Banner> banners)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (banners == null) throw new ArgumentNullException(nameof(banners));

            foreach (var banner in banners)
            {
                var bannerModel = new BannerModel
                {
                    Banner = banner
                };
                model.Items.Add(bannerModel);
            }

            var imageSize = model.Filter.Size.GetValueOrDefault();
            var imageSizeInfo = AttributeHelper.GetMemberAttribute<ImageSizeAttribute>(typeof(BannerSize).GetMember(imageSize.ToString())[0]);

            model.ImageWidth += imageSizeInfo.Width;
            model.ImageHeight += imageSizeInfo.Height;

            model.SizeOptions.AddRange(SelectListHelper.GetEnumSelectList(selectedEnum: model.Filter.Size, defaultText: "All"));

            model.Page = banners.Page;
            model.PageSize = banners.PageSize;
            model.PageFrom = banners.PageFrom;
            model.PageTo = banners.PageTo;
            model.TotalPages = banners.TotalPages;
            model.TotalItems = banners.TotalItems;

            return Task.CompletedTask;
        }

        public Task PrepareModelAsync(ReviewListModel model, IPageable<Review> reviews)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (reviews == null) throw new ArgumentNullException(nameof(reviews));

            foreach (var review in reviews)
            {
                var reviewModel = new ReviewModel
                {
                    Review = review
                };
                model.Items.Add(reviewModel);
            }

            model.ApprovedOptions.AddRange(SelectListHelper.GetBoolSelectList("Approved", "Rejected", selectedBool: model.Filter.Approved, defaultText: "All"));
            model.RatingOptions.AddRange(SelectListHelper.GetSelectList(Enumerable.Range(1, 5).Reverse(),
                (x) => new SelectListItem<int>(text: $"{"star".ToQuantity(x)}", value: x.ToString(), selected: x == model.Filter.Rating), defaultText: "All"));

            model.Page = reviews.Page;
            model.PageSize = reviews.PageSize;
            model.PageFrom = reviews.PageFrom;
            model.PageTo = reviews.PageTo;
            model.TotalPages = reviews.TotalPages;
            model.TotalItems = reviews.TotalItems;

            return Task.CompletedTask;
        }

        public Task PrepareModelAsync(AddressListModel model, IPageable<Address> addresses)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (addresses == null) throw new ArgumentNullException(nameof(addresses));

            foreach (var address in addresses)
            {
                var addressModel = new AddressModel
                {
                    Address = address,
                    AddressType = model.AddressType
                };

                model.Items.Add(addressModel);
            }

            model.Page = addresses.Page;
            model.PageSize = addresses.PageSize;
            model.PageFrom = addresses.PageFrom;
            model.PageTo = addresses.PageTo;
            model.TotalPages = addresses.TotalPages;
            model.TotalItems = addresses.TotalItems;

            return Task.CompletedTask;

        }

        public async Task PrepareModelAsync(OrderItemListModel model, IPageable<OrderItem> orderItems)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (orderItems == null) throw new ArgumentNullException(nameof(orderItems));

            foreach (var orderItem in orderItems)
            {
                var orderItemModel = new OrderItemModel();
                await PrepareModelAsync(orderItemModel, orderItem);
                model.Items.Add(orderItemModel);
            }

            model.Page = orderItems.Page;
            model.PageSize = orderItems.PageSize;
            model.PageFrom = orderItems.PageFrom;
            model.PageTo = orderItems.PageTo;
            model.TotalPages = orderItems.TotalPages;
            model.TotalItems = orderItems.TotalItems;
        }



        public Task PrepareModelAsync(ProductModel model, Product product)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (product == null) throw new ArgumentNullException(nameof(product));

            model.Product = product;
            return Task.CompletedTask;
        }

        public async Task PrepareModelAsync(OrderModel model, Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            model.Order = order;
            model.AddressesWithTypes.Add((order.BillingAddress, AddressType.Billing));
            model.AddressesWithTypes.Add((order.DeliveryAddress, AddressType.Delivery));

            model.UsersWithRoles.Add((order.Seller, RoleNames.Seller));
            model.UsersWithRoles.Add((order.Customer, RoleNames.Customer));

            foreach (var status in EnumHelper.GetEnumValues<OrderStatus>())
            {
                var canChangeStatus = (await _orderService.CanChangeStatusAsync(order, status)).Success;
                model.StatusActions.Add((canChangeStatus, status));
            }

            foreach (var orderItem in order.OrderItems)
            {
                var orderItemModel = new OrderItemModel();
                await PrepareModelAsync(orderItemModel, orderItem);

                model.OrderItemModels.Add(orderItemModel);
            }
        }

        public Task PrepareModelAsync(OrderItemModel model, OrderItem orderItem)
        {
            model.OrderItem = orderItem;
            return Task.CompletedTask;
        }


        public async Task PrepareModelAsync(ProductEditModel model, Product product, User seller)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (seller == null)
                throw new ArgumentNullException(nameof(seller));

            if (product != null)
            {
                model = _mapper.Map(product, model);
                model.Images = product.Images.ToList();
                model.TagNames.AddRange(product.Tags.Select(x => x.Name));
            }
            else
            {
                model.Published = true;
                model.Stock = ProductStock.InStock;
            }

            model.StockOptions.AddRange(SelectListHelper.GetEnumSelectList<ProductStock>(selectedEnum: model.Stock));
        
            var categoryNames = (await _categoryService.ListAsync(new CategoryFilter { SellerId = seller.Id })).Select(x => x.Name).Distinct();
            var tagNames = (await _tagService.ListAsync(new TagFilter { SellerId = seller.Id })).Select(x => x.Name).Distinct();
            tagNames = categoryNames.Concat(tagNames).Distinct(StringComparer.InvariantCultureIgnoreCase);

            model.TagOptions.AddRange(SelectListHelper.GetSelectList(tagNames, x => new SelectListItem<string>(text: x, value: x)));
        }

        public async Task PrepareModelAsync(CategoryEditModel model, Category category, User seller)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (category != null)
            {
                model = _mapper.Map(category, model);
                model.TagNames.AddRange(category.Tags.Select(x => x.Name));
            }
            else
            {
                model.Published = true;
            }

            var tagNames = (await _tagService.ListAsync(new TagFilter() { SellerId = seller.Id })).Select(x => x.Name).Distinct();

            model.TagOptions.AddRange(SelectListHelper.GetSelectList(tagNames, x => new SelectListItem<string>(text: x, value: x)));

            var icons = await GenerateFa5IconsAsync();
            model.IconOptions.AddRange(SelectListHelper.GetSelectList(icons, x => new SelectListItem<string>(text: x.Humanize(LetterCasing.Title), value: x, selected: x == model.Icon), defaultText: "No Icon"));
        }

        public Task PrepareModelAsync(BannerEditModel model, Banner banner)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (banner != null)
            {
                model = _mapper.Map(banner, model);
            }
            else
            {
                model.Published = true;
            }

            return Task.CompletedTask;
        }

        public Task PrepareModelAsync(ReviewEditModel model, Review review)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (review != null)
            {
                model = _mapper.Map(review, model);
            }
            else
            {

            }

            return Task.CompletedTask;
        }

        public Task PrepareModelAsync(AddressEditModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (address != null)
            {
                model = _mapper.Map(address, model);
            }
            else
            {

            }

            return Task.CompletedTask;
        }

        public Task PrepareModelAsync(ProfileEditModel model, User user)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (user == null) throw new ArgumentNullException(nameof(user));

            model = _mapper.Map(user, model);

            return Task.CompletedTask;
        }

        public Task PrepareModelAsync(StoreEditModel model, User user)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (user == null) throw new ArgumentNullException(nameof(user));

            model = _mapper.Map(user, model);

            model.StoreDeliveryRequiredOptions.AddRange(SelectListHelper.GetBoolSelectList("Required", "Not Required", selectedBool: model.StoreDeliveryRequired));
            model.StoreStatusOptions.AddRange(SelectListHelper.GetEnumSelectList<StoreStatus>(selectedEnum: model.StoreStatus));

            model.StoreThemeModeOptions.AddRange(SelectListHelper.GetEnumSelectList<ThemeMode>(selectedEnum: model.StoreThemeMode));
            model.StoreThemeStyleOptions.AddRange(SelectListHelper.GetEnumSelectList<ThemeStyle>(selectedEnum: model.StoreThemeStyle));

            model.StoreCategorySelections.AddRange(user.StoreCategory.ExpandEnum());
            model.StoreCategoryOptions.AddRange(SelectListHelper.GetEnumSelectList<StoreCategory>(selectedEnum: user.StoreCategory));

            return Task.CompletedTask;
        }


        public Task PrepareModelAsync(OrderStatusModel model, Order order)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            model = _mapper.Map(order, model);

            return Task.CompletedTask;
        }



        public Task PrepareStoreAsync(User user, StoreEditModel model)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (model == null) throw new ArgumentNullException(nameof(model));

            user = _mapper.Map(model, user);

            user.StoreCategory = model.StoreCategorySelections.MergeEnums();

            user.StoreSlug = SanitizerHelper.GenerateSlug(model.StoreSlug);

            user.AboutNote = SanitizerHelper.SanitizeHtml(model.AboutNote);
            user.TermsNote = SanitizerHelper.SanitizeHtml(model.TermsNote);
            user.PrivacyNote = SanitizerHelper.SanitizeHtml(model.PrivacyNote);
            user.ReturnsNote = SanitizerHelper.SanitizeHtml(model.ReturnsNote);
            user.ReviewsNote = SanitizerHelper.SanitizeHtml(model.ReviewsNote);


            user.FacebookLink = SanitizerHelper.AppendUrlScheme(model.FacebookLink);
            user.TwitterLink = SanitizerHelper.AppendUrlScheme(model.TwitterLink);
            user.YoutubeLink = SanitizerHelper.AppendUrlScheme(model.YoutubeLink);
            user.InstagramLink = SanitizerHelper.AppendUrlScheme(model.InstagramLink);
            user.LinkedInLink = SanitizerHelper.AppendUrlScheme(model.LinkedInLink);
            user.PinterestLink = SanitizerHelper.AppendUrlScheme(model.PinterestLink);
            user.WhatsAppNumber = SanitizerHelper.ExtractPhoneNumber(model.WhatsAppNumber);
            user.MapLink = SanitizerHelper.AppendUrlScheme(model.MapLink);

            return Task.CompletedTask;
        }

        public Task PrepareProfileAsync(User user, ProfileEditModel model)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (model == null) throw new ArgumentNullException(nameof(model));

            user = _mapper.Map(model, user);

            return Task.CompletedTask;
        }

        public async Task PrepareMediaAsync<TMedia>(IEnumerable<TMedia> existingMedias, IEnumerable<TMedia> currentMedias, Func<TMedia, Task> createMedia, Func<TMedia, Task> updateMedia, Func<TMedia, Task> deleteMedia)
             where TMedia : Media
        {
            currentMedias = currentMedias.Where(x => x != null);
            existingMedias = existingMedias.Where(x => x != null);

            var currentFileNames = currentMedias.Select(x => x.FileName).ToList();
            var existingFileNames = existingMedias.Select(x => x.FileName).ToList();

            var addFileNames = currentFileNames.Except(existingFileNames);
            var removeFileNames = existingFileNames.Except(currentFileNames);

            var mediasToCreate = currentMedias.Where(x => addFileNames.Contains(x.FileName)).ToList();
            var mediasToDelete = existingMedias.Where(x => removeFileNames.Contains(x.FileName)).ToList();
            var mediasToUpdate = existingMedias.ToList();

            foreach (var media in mediasToDelete)
            {
                await deleteMedia(media);
                mediasToUpdate.Remove(media);
            }

            foreach (var media in mediasToCreate)
            {
                await createMedia(media);
                mediasToUpdate.Add(media);
            }

            mediasToUpdate = mediasToUpdate.OrderBy(x => currentFileNames.IndexOf(x.FileName)).ToList();

            foreach (var media in mediasToUpdate)
            {
                media.Position = mediasToUpdate.IndexOf(media) + 1;
                await updateMedia(media);
            }
        }

        public async Task<string> GenerateNoteAsync(User user, NoteTemplateType noteTemplateType)
        {
            string content = await _razorViewRenderer.RenderViewToStringAsync($"~/Views/Shared/Templates/Note/{noteTemplateType}.cshtml", (user, (object)null));
            return content;
        }

        public async Task<List<string>> GenerateFa5IconsAsync()
        {
            var fa5fileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo("/assets/font-awesome-5-icons.json");
            var fa5Icons = (await fa5fileInfo.CreateReadStream().ReadAllTextAsync()).ToJsonObject<List<string>>();
            return fa5Icons;
        }
    }
}
