using System;
using System.Collections.Generic;

namespace Inventory_Order.Models.Database;

public partial class OrderTb
{
    public int OrdersId { get; set; }

    public int ProductsId { get; set; }

    public int CustomersId { get; set; }

    public int Quantity { get; set; }

    public int Amount { get; set; }

    public virtual CustomerTb Customers { get; set; } = null!;

    public virtual ProductTb Products { get; set; } = null!;
}
