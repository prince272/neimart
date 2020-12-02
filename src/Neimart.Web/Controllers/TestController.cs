using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Services;
using Neimart.Data;

namespace Neimart.Web.Controllers
{
    public class TestController : Controller
    {
        public TestController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}