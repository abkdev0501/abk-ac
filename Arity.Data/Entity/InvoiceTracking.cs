
namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class InvoiceTracking
    {
        [Key]
        public int id { get; set; }
        public int InvoiceId { get; set; }
        public Nullable<int> UserId { get; set; }
        public string Comment { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}
