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
        public int GroupId { get; set; }
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public DateTime Date { get; set; }
        public string Particular { get; set; }
        public string InvoiceId { get; set; }
        public int? Debit { get; set; }
        public int? Credit { get; set; }
        public string CompanyName { get; set; }
        public string Year { get; set; }
        public string GroupName { get; set; }
    }
}
