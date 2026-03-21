using System;
using System.Collections.Generic;

namespace Inventory_Order.Models.Database;

public partial class UserTb
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? CustomerId { get; set; }

    public bool IsActive { get; set; }

    public virtual CustomerTb? Customer { get; set; }
}
