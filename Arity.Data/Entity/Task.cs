
namespace Arity.Data.Entity
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Tasks
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int Status { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool Active { get; set; }
        public int? ClientId { get; set; }
        public int? Priorities { get; set; }
        public string Remarks { get; set; }
        public bool? IsChargeble { get; set; }
        public DateTime? CompletedOn { get; set; }
        public int AddedBy { get; set; }
        public decimal? ChargeAmount { get; set; }
    } 
}
