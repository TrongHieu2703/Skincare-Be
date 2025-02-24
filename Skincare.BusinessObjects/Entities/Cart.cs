using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.Entities;

public partial class Cart
{
    public int CartId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    public DateTime? AddedDate { get; set; } = DateTime.Now;

    public virtual Product Product { get; set; }

    public virtual Account User { get; set; }
}