using System;
using System.Collections.Generic;

namespace Inventory_Order.Models.Database;

public partial class OrderTb
{
    public int OrdersId { get; set; }

    public int CustomersId { get; set; }

    public decimal TotalAmount { get; set; }

    public string OrderStatus { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public virtual CustomerTb Customers { get; set; } = null!;

    public virtual ICollection<OrderItemTb> OrderItemTbs { get; set; } = new List<OrderItemTb>();
}
