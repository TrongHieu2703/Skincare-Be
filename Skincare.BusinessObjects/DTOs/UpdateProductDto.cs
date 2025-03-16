using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class UpdateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Image { get; set; }
        public bool? IsAvailable { get; set; }
        public int? ProductTypeId { get; set; }
        public int? ProductBrandId { get; set; }
    }
}
