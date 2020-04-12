
namespace Arity.Data.Entity
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class DocumentMaster
    {
        [Key]
        public int DocumentId { get; set; }

        [Required(ErrorMessage = "Document Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Client Name is required")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Document Type is required")]
        public int DocumentType { get; set; }

        [Required(ErrorMessage = "Status must be specified")]
        public int Status { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        [Required(ErrorMessage = "Please select file to upload")]
        public string FileName { get; set; }
    }
}
