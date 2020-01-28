using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Data.Dto
{
   public class ReceiptDto
    {
        public long ReceiptId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string RecieptNo { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public string BankName { get; set; }
        public string ChequeNumber { get; set; }
        public bool Status { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public List<long?> InvoiceIds { get; set; }
        public long InvoiceId { get; set; }
        public long ReceiptMappingId { get; set; }
        public string CreatedDateString { get; set; }
        public string InvoiceNumbers { get; set; }
        public int CompanyId { get; set; }
        public int ClientId { get; set; }
    }
}
