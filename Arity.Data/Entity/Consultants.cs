
namespace Arity.Data.Entity
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Consultants
    {
        [Key]
        public int ConsultantId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Mobile { get; set; }
        public string Notes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
