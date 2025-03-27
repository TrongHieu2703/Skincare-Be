using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class SkinTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
    }
    
    public class SkinTypesListResponse
    {
        public string Message { get; set; }
        public IEnumerable<SkinTypeDto> Data { get; set; }
    }
} 