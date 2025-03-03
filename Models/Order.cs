﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment_Webshop.Models;

internal class Order
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public int UserId { get; set; }
    public int ShippingId { get; set; }
    public DateTime OrderDate { get; set; }

    public virtual User User { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}
