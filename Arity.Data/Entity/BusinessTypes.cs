namespace Arity.Data.Entity
{
    using System.ComponentModel.DataAnnotations;

    public partial class BusinessTypes
    {
        [Key]
        public int BusinessTypeId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
