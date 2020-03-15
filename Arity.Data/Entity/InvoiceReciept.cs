
namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class InvoiceReciept
    {
        [Key]
        public long id { get; set; }
        public Nullable<long> InvoiceId { get; set; }
        public Nullable<long> RecieptId { get; set; }
        public int CreatedBy { get; set; }
    }
}
