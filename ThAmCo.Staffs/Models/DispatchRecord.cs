using System;
using ThAmCo.Customers.Models; // Reference for Order model

namespace ThAmCo.Staffs.Models
{
    public class DispatchRecord
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public bool IsDispatched { get; set; } = false;
        public DateTime? DispatchedAt { get; set; }
    }
}

