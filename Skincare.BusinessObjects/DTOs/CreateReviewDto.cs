using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid product ID")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Customer ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid customer ID")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Review title is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Review title must be between 5 and 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Review content is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review content must be between 10 and 1000 characters")]
        public string Content { get; set; }

        [StringLength(500, ErrorMessage = "Review images cannot exceed 500 characters")]
        public string? Images { get; set; }

        [StringLength(200, ErrorMessage = "Pros cannot exceed 200 characters")]
        public string? Pros { get; set; }

        [StringLength(200, ErrorMessage = "Cons cannot exceed 200 characters")]
        public string? Cons { get; set; }

        [Range(0, 12, ErrorMessage = "Usage duration must be between 0 and 12 months")]
        public int? UsageDuration { get; set; }

        [RegularExpression(@"^(verified|unverified)$", ErrorMessage = "Verification status must be verified or unverified")]
        public string VerificationStatus { get; set; } = "unverified";

        public bool IsAnonymous { get; set; } = false;
    }
}
