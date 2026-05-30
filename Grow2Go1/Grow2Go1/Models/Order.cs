using System;

namespace Grow2Go1.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int FarmId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? EstimatedDelivery { get; set; }

        // Joined from users table
        public string CustomerName { get; set; }
        public string CustomerProfilePicPath { get; set; }
        public string FarmName { get; set; }
        public string DeliveryMode { get; set; }
        public string PaymentMethod { get; set; }
    }
}