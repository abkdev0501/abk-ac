
namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class InvoiceParticular
    {
        [Key]
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public long ParticularId { get; set; }
        public decimal Amount { get; set; }
        public string year { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public int CreatedBy { get; set; }
    }
}
