using System;
using System.Collections.Generic;

namespace Inventory_Order.Models.Database;

public partial class OrderItemTb
{
    public int OrderItemId { get; set; }

    public int OrdersId { get; set; }

    public int ProductsId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public virtual OrderTb Orders { get; set; } = null!;

    public virtual ProductTb Products { get; set; } = null!;
}
