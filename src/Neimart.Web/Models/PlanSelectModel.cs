using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Infrastructure.Web;

namespace Neimart.Web.Models
{
    public class PlanSelectModel
    {
        public PlanType? PlanType { get; set; }

        public List<SelectListItem> PlanTypeOptions { get; } = new List<SelectListItem>();

        public PlanPeriod? PlanPeriod { get; set; }

        public List<SelectListItem<PlanPeriod>> PlanPeriodOptions { get; } = new List<SelectListItem<PlanPeriod>>();
    }

    public class PlanSelectValidator : AbstractValidator<PlanSelectModel>
    {
        public PlanSelectValidator()
        {
            RuleFor(x => x.PlanType).NotNull();
            RuleFor(x => x.PlanPeriod).NotNull();
        }
    }
}
