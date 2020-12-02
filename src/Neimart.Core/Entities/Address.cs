using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neimart.Core.Entities
{
    public class Address : IEntity
    {
        public long Id { get; set; }

        public string FullName => $"{FirstName?.Trim()} {LastName?.Trim()}".Trim();

        public string FullAddress => string.Join(", ", new[] { Region, Place, Street, Postal }
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim(',', ' ')));

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Region { get; set; }

        public string Place { get; set; }

        public string Street { get; set; }

        public string Postal { get; set; }

        public string Organization { get; set; }

        public virtual User Customer { get; set; }

        public long CustomerId { get; set; }

        public ICollection<AddressType> AddressTypes { get; set; } = new List<AddressType>();
    }

    public enum AddressType
    {
        Delivery,
        Billing
    }
}
