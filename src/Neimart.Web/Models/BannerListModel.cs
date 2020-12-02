using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public class BannerListModel : PageableModel<BannerModel, BannerFilter>
    {
        public bool SuggestItems { get; set; }

        public ViewType ViewType { get; set; }

        public int ImageHeight { get; set; }

        public int ImageWidth { get; set; }

        public BannerSize ImageSize { get; set; }

        public List<SelectListItem> SizeOptions { get; } = new List<SelectListItem>();

        protected override object GetFilterValues()
        {
            return TypeMerger.Merge(new
            {
                search = Filter.Search,
                size = Filter.Size
            }, base.GetFilterValues());
        }
    }

    public class BannerModel
    {
        public Banner Banner { get; set; }
    }
}
