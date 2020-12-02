using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neimart.Core.Services;

namespace Neimart.Web.Models
{
    public class DashboardModel
    {
        public AmountEvaluation ProductListEvaluation { get; set; }

        public CountEvaluation CategoryListEvaluation { get; set; }

        public CountEvaluation BannerListEvaluation { get; set; }

        public ReviewEvaluation ReviewListEvaluation { get; set; }
    }
}
