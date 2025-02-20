using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.BusinessObjects.Entities
{
    public class OrderFeedback
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime FeedbackDate { get; set; }
    }

}
