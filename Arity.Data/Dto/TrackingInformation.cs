using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Data.Dto
{
   public class TrackingInformation
    {
        public int TrackingId { get; set; }
        public string Comment { get; set; }
        public string CreatedAt { get; set; }
        public int InvoiceId { get; set; }
    }
}
