
namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class InvoiceDetail
    {
        [Key]
        public long Id { get; set; }
        public string Invoice_Number { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public long ClientId { get; set; }
        public long CompanyId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
}
