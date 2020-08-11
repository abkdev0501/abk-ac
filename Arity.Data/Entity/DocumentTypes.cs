namespace Arity.Data.Entity
{
    using System.ComponentModel.DataAnnotations;

    public partial class DocumentTypes
    {
        [Key]
        public int DocumnetTypeId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
