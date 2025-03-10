using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.BusinessObjects.DTOs
{
    public class CartDTO
    {
        public int CartId { get; set; }
        public int AccountId { get; set; }
        public decimal SubTotal { get; set; }
        public DateTime AddedDate { get; set; }
        public List<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();
    }

    // Thêm DTO cho thêm sản phẩm vào giỏ hàng
    public class AddToCartDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // Thêm DTO cho cập nhật số lượng sản phẩm
    public class UpdateCartItemDTO
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
