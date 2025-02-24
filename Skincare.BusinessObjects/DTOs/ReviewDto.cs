﻿using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int OrderDetailId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int? Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
