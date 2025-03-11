using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    public class CompareRequestDto
    {
        [Required]
        public List<int> ProductIds { get; set; }
    }
}
