﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.Entities;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int ItemQuantity { get; set; }

    public virtual Order Order { get; set; }

    public virtual Product Product { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}