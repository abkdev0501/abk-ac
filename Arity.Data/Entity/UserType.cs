
namespace Arity.Data.Entity
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class UserType
    {
        [Key]
        public long Id { get; set; }
        public string UserTypeName { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
    }
}
