
namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class RecieptDetail
    {
        [Key]
        public long Id { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string RecieptNo { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public string BankName { get; set; }
        public string ChequeNumber { get; set; }
        public Nullable<bool> Status { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public DateTime RecieptDate { get; set; }
        public int CreatedBy { get; set; }
        public string Remarks { get; set; }
        public int? AddedBy { get; set; }
        public long? companyId { get; set; }
        public long? clientId { get; set; }
    }
}
