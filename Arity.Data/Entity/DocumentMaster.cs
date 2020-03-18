
namespace Arity.Data.Entity
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class DocumentMaster
    {
        [Key]
        public int DocumentId { get; set; }
        public string Name { get; set; }
        public int ClientId { get; set; }
        public int DocumentType { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string FileName { get; set; }
    }
}
