using System;

namespace Arity.Data.Dto
{
    public class InvoiceEntry
    {
        public long InvoiceId { get; set; }
        public long InvoiceParticularId { get; set; }
        public long ClientId { get; set; }
        public long CompanyId { get; set; }
        public long ParticularId { get; set; }
        public string Year { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string CreatedDateString { get; set; }
        public string SFParticulars { get; set; }
        public string FFParticulars { get; set; }
        public int CreatedBy { get; set; }
        public DateTime InvoiceDate{ get; set; }
        public bool IsExclude { get; set; }
        public string CompanyName { get; set; }
        public string CreatedByString { get; set; }
        public string GroupName { get; set; }
        public int AddedBy { get; set; }
        public string Remarks { get; set; }
    }
}
