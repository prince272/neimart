using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Services;

namespace Neimart.Web.Models
{
    public class CartListModel : PageableModel<CartModel, CartFilter>
    {
        public CartType CartType { get; set; }

        public CartType OtherCartType => CartType == CartType.Cart ? CartType.Wishlist : CartType.Cart;


        public AmountEvaluation CartListEvaluation { get; set; }
    }

    public class CartModel
    {
        public CartType Type { get; set; }

        public int Quantity { get; set; }

        public List<SelectListItem> QuantityOptions { get; } = new List<SelectListItem>();

        public ProductModel ProductModel { get; set; }

        public AmountEvaluation Evaluation { get; set; }
    }

    public class CartProfile : Profile
    {
        public CartProfile()
        {
            CreateMap<Cart, CartModel>();
        }
    }
}
