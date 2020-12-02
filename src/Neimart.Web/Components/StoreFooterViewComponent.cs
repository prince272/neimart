using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neimart.Web.Models;

namespace Neimart.Web.Components
{
    public class StoreFooterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new StoreFooterModel();
            return View(model);
        }
    }
}
