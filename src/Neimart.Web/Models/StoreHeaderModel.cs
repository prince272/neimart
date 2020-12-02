using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;

namespace Neimart.Web.Models
{
    public class StoreHeaderModel
    {
        public AmountEvaluation CartListEvaluation { get; set; }

        public AmountEvaluation WishlistEvaluation { get; set; }

        public List<SelectListItem<Category>> CategoryOptions { get; } = new List<SelectListItem<Category>>();
    }
}
