using System;
using System.Collections.Generic;

namespace Inventory_Order.Models.Database;

public partial class CustomerTb
{
    public int CustomerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<OrderTb> OrderTbs { get; set; } = new List<OrderTb>();
}
