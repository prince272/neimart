using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Utilities.Helpers;
using UAParser;

namespace Neimart.Web.Models
{
    public class TransactionListModel : PageableModel<TransactionModel, TransactionFilter>
    {
        public List<SelectListItem> StatusOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem> ProcessorOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem> ModeOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem> TypeOptions { get; } = new List<SelectListItem>();

        protected override object GetFilterValues()
        {
            return TypeMerger.Merge(new
            {
                status = Filter.Status,
                processor = Filter.Processor,
                type = Filter.Type,
                mode = Filter.Mode,
                search = Filter.Search,
            }, base.GetFilterValues());
        }
    }

    public class TransactionModel
    {
        public Transaction Transaction { get; set; }
    }
}
