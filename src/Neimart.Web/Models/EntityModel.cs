using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public abstract class EntityModel
    {
        protected virtual object GetFilterValues() => new object();

        public object GetRequestValues(object values = null)
        {
            if (values != null)
                return TypeMerger.Merge(GetFilterValues(), values);
            else
                return GetFilterValues();
        }

        public bool HasRequestValues(object values = null)
        {
            var filterObject = GetRequestValues(values);
            var hasFilterObject = filterObject.GetType().GetProperties().Any(x => x.GetValue(filterObject) != null);
            return hasFilterObject;
        }
    }

    public abstract class PageableModel<TItem, TFilter> : EntityModel
    {
        public ICollection<TItem> Items { get; } = new List<TItem>();

        public TFilter Filter { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int PageFrom { get; set; }

        public int PageTo { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }

        protected override object GetFilterValues()
        {
            return TypeMerger.Merge(new
            {
                page = Page == 1 ? (int?)null : Page,

            }, base.GetFilterValues());
        }
    }
}