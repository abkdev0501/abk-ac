using Arity.Data.Dto;
using System.Collections.Generic;

namespace ArityApp.Models.Reports
{
    public class ViewLedgerReport
    {
        public List<LedgerReportDto> Data { get; set; }
        public string Period { get; set; }
        public int ClosingBalance { get; set; }
        public int TotalDebit { get; set; }
        public int TotalCredit { get; set; }
        public string ClientName { get; set; }
        public string GroupName { get; set; }
        public string GroupAddress { get; set; }
        public string ClientAddress { get; set; }
        public bool IsGroup { get; set; }
    }
}