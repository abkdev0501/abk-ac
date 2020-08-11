namespace Arity.Data.Entity
{
    using System.ComponentModel.DataAnnotations;

    public partial class BusinessStatus
    {
        [Key]
        public int BusinessStatusId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
