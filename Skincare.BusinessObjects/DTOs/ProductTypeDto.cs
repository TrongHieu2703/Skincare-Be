using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class ProductTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    public class ProductTypesListResponse
    {
        public string Message { get; set; }
        public IEnumerable<ProductTypeDto> Data { get; set; }
    }
} 