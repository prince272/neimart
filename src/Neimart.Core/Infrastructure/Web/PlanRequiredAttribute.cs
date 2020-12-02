using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Neimart.Core.Services;

namespace Neimart.Core.Infrastructure.Web
{
    public class PlanRequiredAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userSerivce = context.HttpContext.RequestServices.GetRequiredService<UserService>();
            var member = await context.HttpContext.GetMemberAsync();
            var memberIsNotInAdminRole = !(await userSerivce.IsInRoleAsync(member, RoleNames.Admin));
            var returnUrl = context.HttpContext.Request.RelativeUrl();

            if (memberIsNotInAdminRole && member.StorePlanEnded)
            {
                context.Result = new RedirectToActionResult("PlanEnded", "Account", new { area = string.Empty, returnUrl  });
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
