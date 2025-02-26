using System;

namespace Skincare.BusinessObjects.DTOs
{
    public class CreateVoucherDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsPercent { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal Value { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public bool? IsInfinity { get; set; }
        public int Quantity { get; set; }
        public int PointCost { get; set; }
    }
}
