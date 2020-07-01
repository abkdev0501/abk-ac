using System;
using System.Collections.Generic;

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
        public List<long> InvoiceIds { get; set; }
        public long InvoiceId { get; set; }
        public long ReceiptMappingId { get; set; }
        public string CreatedDateString { get; set; }
        public string InvoiceNumbers { get; set; }
        public string Year { get; set; }

        public List<InvoiceEntry> Invoices { get; set; }
        public long CompanyId { get; set; }
        public long ClientId { get; set; }
        public DateTime RecieptDate { get; set; }
        public int CreatedBy { get; set; }
        public string Remarks { get; set; }
        public string CompanyName { get; set; }
        public string ClientName { get; set; }
        public string GroupName { get; set; }
        public string AddedBy { get; set; }
    }
}
