using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int? Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CustomerName { get; set; }
    }

    // public class CreateReviewDto
    // {
    //     public int OrderDetailId { get; set; }
    //     public int CustomerId { get; set; }
    //     public int ProductId { get; set; }
    //     public int Rating { get; set; }
    //     public string Comment { get; set; }
    // }

    // public class UpdateReviewDto
    // {
    //     public int? Rating { get; set; }
    //     public string Comment { get; set; }
    // }
}
