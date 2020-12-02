using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Neimart.Core.Services;

namespace Neimart.Core.Entities
{
    public class Order : IEntity, ICloneable
    {
        public virtual User Seller { get; set; }

        public long SellerId { get; set; }

        public virtual User Customer { get; set; }

        public long CustomerId { get; set; }

        public string OrderCode { get; set; }

        public string TrackingCode { get; set; }

        public long Id { get; set; }

        public OrderStatus Status { get; set; }

        public string ProcessingInfo { get; set; }

        public string DeliveryInfo { get; set; }

        public decimal DeliveryFee { get; set; }

        public DateTimeOffset DeliveringOn { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public DateTimeOffset CompletedOn { get; set; }

        public DateTimeOffset CancelledOn { get; set; }

        public string CancelReason { get; set; }

        public bool Updated { get; set; }

        public bool Deleted { get; set; }

        public bool Paid { get; set; }

        public bool Refunded { get; set; }

        public DateTimeOffset PaidOn { get; set; }

        public Address BillingAddress { get; set; }

        public Address DeliveryAddress { get; set; }

        public bool DeliveryRequired { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public int TotalQuantity => OrderItems.Select(x => x.Quantity).Sum();

        public decimal SubtotalAmount => OrderItems.Select(x => x.Amount).Sum();

        public decimal TotalAmount => SubtotalAmount + DeliveryFee;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Delivering,
        Complete,
        Cancelled
    }
}