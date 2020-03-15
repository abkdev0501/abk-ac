
namespace Arity.Data.Entity
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class UserTask
    {
        [Key]
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int  UserId{ get; set; }
        public string Comment{ get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public int Status { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool Active { get; set; }
        public int AddedBy { get; set; }
    }
}
