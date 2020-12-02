using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public class ProductListModel : PageableModel<ProductModel, ProductFilter>
    {
        public List<SelectListItem> StockOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem> PublishedOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem> SortOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem<Category>> CategoryOptions { get; } = new List<SelectListItem<Category>>();

        public bool SuggestItems { get; set; }

        public ViewType ViewType { get; set; }

        public Category Category { get; set; }

        public int LowestMinPrice { get; set; }

        public int HighestMaxPrice { get; set; }

        public List<SelectListItem> RatingOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem> DiscountOptions { get; } = new List<SelectListItem>();

        protected override object GetFilterValues()
        {
            return TypeMerger.Merge(new
            {
                stock = Filter.Stock,
                search = Filter.Search,
                minPrice = Filter.MinPrice,
                maxPrice = Filter.MaxPrice,
                rating = Filter.Rating,
                sort = Filter.Sort,
                discount = Filter.Discount
            }, base.GetFilterValues());
        }
    }

    public class ProductModel
    {
        public Product Product { get; set; }

        public ReviewEvaluation ReviewEvaluation { get; set; }

        public ReviewListModel ReviewListModel { get; set; }

        public bool IsAddedToWishlist { get; set; }

        public bool IsAddedToCart { get; set; }
    }

    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductModel>();
        }
    }
}
