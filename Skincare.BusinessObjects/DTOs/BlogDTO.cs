using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.BusinessObjects.DTOs
{
    public class BlogDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Img { get; set; }
        public int BlogOwnerId { get; set; }
        public int BlogCategoryId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsVisible { get; set; }
        public string BlogCategory { get; set; }
        public string BlogOwner { get; set; }
    }
}
