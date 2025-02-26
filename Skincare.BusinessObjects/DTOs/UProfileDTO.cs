using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.BusinessObjects.DTOs
{
    public class UProfileDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public string PhoneNumber { get; set; }

        // 🔥 Thêm cho Admin nhưng không ảnh hưởng đến Customer
        public string Status { get; set; }     // (Active, Inactive, etc.)
        public string Role { get; set; }       // (User, Admin)
        public DateTime? CreatedAt { get; set; } // Ngày tạo tài khoản
    }


}
