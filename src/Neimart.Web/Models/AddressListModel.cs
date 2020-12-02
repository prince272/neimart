using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Web.Models
{
    public class AddressListModel : PageableModel<AddressModel, AddressFilter>
    {
        public AddressType? AddressType { get; set; }
    }

    public class AddressModel
    {
        public Address Address { get; set; }

        public AddressType? AddressType { get; set; }
    }
}
