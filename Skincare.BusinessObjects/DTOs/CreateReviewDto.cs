using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class CreateReviewDto
    {
        [Required]
        public int OrderDetailId { get; set; }
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public int ProductId { get; set; }
        public int? Rating { get; set; }
        [Required]
        public string Comment { get; set; }
    }
}
