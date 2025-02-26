using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.BusinessObjects.DTOs
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }        
        public string OtpCode { get; set; }      
        public string NewPassword { get; set; }  
    }

}
