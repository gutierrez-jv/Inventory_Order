using System;
using System.Collections.Generic;

namespace Inventory_Order.Models.Database;

public partial class ProductTb
{
    public int ProductsId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int Quantity { get; set; }

    public int Price { get; set; }

    public bool Stock { get; set; }

    public string Barcode { get; set; } = null!;

    public virtual ICollection<OrderTb> OrderTbs { get; set; } = new List<OrderTb>();
}
