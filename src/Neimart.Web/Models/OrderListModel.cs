using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Services;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public class OrderListModel : PageableModel<OrderModel, OrderFilter>
    {
        public List<SelectListItem> StatusOptions { get; } = new List<SelectListItem>();

        protected override object GetFilterValues()
        {
            return TypeMerger.Merge(new
            {
                orderCode = Filter.OrderCode,
                trackingCode = Filter.TrackingCode,
                status = Filter.Status,
                search = Filter.Search,
            }, base.GetFilterValues());
        }
    }

    public class OrderModel
    {
        public Order Order { get; set; }

        public List<(User User, string Role)> UsersWithRoles { get; } = new List<(User, string)>();

        public List<(Address Address, AddressType AddressType)> AddressesWithTypes { get; } = new List<(Address, AddressType)>();

        public List<OrderItemModel> OrderItemModels { get; } = new List<OrderItemModel>();

        public List<(bool CanChangeStatus, OrderStatus Status)> StatusActions { get; } = new List<(bool, OrderStatus)>();
    }

    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderModel>();
        }
    }

    public class OrderItemModel
    {
        public OrderItem OrderItem { get; set; }
    }

    public class OrderItemProfile : Profile
    {
        public OrderItemProfile()
        {
            CreateMap<OrderItem, OrderItemModel>();
        }
    }
}
