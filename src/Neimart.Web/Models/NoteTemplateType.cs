using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Neimart.Web.Models
{
    public enum NoteTemplateType
    {
        [Display(Name = "About Store")]
        About,
        [Display(Name = "Terms of Service")]
        Terms,
        [Display(Name = "Privacy Policy")]
        Privacy,
        [Display(Name = "Return Policy")]
        Returns,
        [Display(Name = "Review Policy")]
        Reviews
    }
}
