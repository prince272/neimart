using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public class CategoryListModel : PageableModel<CategoryModel, CategoryFilter>
    {
        public bool SuggestItems { get; set; }

        public ViewType ViewType { get; set; }

        protected override object GetFilterValues()
        {
            return TypeMerger.Merge(new
            {
                search = Filter.Search,
            }, base.GetFilterValues());
        }
    }

    public class CategoryModel
    {
        public Category Category { get; set; }
    }
}
