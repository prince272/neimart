using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neimart.Core.Entities;
using Neimart.Core.Filters;

namespace Neimart.Web.Models
{
    public class OrderItemListModel : PageableModel<OrderItemModel, OrderItemFilter>
    {
    }
}
