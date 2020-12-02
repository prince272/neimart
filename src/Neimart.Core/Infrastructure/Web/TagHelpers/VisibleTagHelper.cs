using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Neimart.Core.Infrastructure.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "visible")]
    public class VisibleTagHelper : TagHelper
    {
        public bool Visible { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (!Visible) output.SuppressOutput();
        }
    }
}
