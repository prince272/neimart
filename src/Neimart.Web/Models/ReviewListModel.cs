using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Services;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public class ReviewListModel : PageableModel<ReviewModel, ReviewFilter>
    {
        public ReviewEvaluation Evaluation { get; set; }

        public List<SelectListItem> ApprovedOptions { get; } = new List<SelectListItem>();

        public List<SelectListItem> RatingOptions { get; } = new List<SelectListItem>();

        protected override object GetFilterValues()
        {
            return TypeMerger.Merge(new
            {
                title = Filter.Title,
                rating = Filter.Rating,
                approved = Filter.Approved
            }, base.GetFilterValues());
        }
    }

    public class ReviewModel
    {
        public Review Review { get; set; }
    }
}
