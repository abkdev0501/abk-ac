using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Data.Dto
{
    public class LedgerReportDto
    {
        public int UserId { get; set; }
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime Date { get; set; }
        public string Particular { get; set; }
        public long InvoiceId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal closingBalance { get; set; }

        
    }
}
