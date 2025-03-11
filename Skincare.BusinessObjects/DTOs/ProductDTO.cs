using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public bool IsAvailable { get; set; }
        public string ProductTypeName { get; set; }
        public string ProductBrandName { get; set; }
        public List<string> SkinTypes { get; set; }
    }
}
