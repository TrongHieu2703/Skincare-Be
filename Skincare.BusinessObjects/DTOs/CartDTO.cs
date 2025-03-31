using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class CartDTO
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public List<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();
        public decimal TotalPrice { get; set; }
    }

    public class CartItemDTO
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
        public int? ProductStock { get; set; }
    }

    public class AddToCartDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartDTO
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
} 