
namespace Arity.Data.Entity
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class CommodityMaster
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string HSN { get; set; }
        public int GST_Rate { get; set; }
        public DateTime EFDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
