using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Services;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public class UserListModel : PageableModel<UserModel, UserFilter>
    {
        public List<SelectListItem> StoreCategoryOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem> StoreSetupOptions { get; } = new List<SelectListItem>();

        protected override object GetFilterValues()
        {
            return TypeMerger.Merge(new
            {
                storeCategory = Filter.StoreCategory,
                storeSetup = Filter.StoreSetup,
                storeRegion = Filter.StoreRegion,
                storePlace = Filter.StorePlace,
                search = Filter.Search,
            }, base.GetFilterValues());
        }
    }

    public class UserModel
    {
        public User User { get; set; }

        public ReviewEvaluation ReviewEvaluation { get; set; }
    }
}
