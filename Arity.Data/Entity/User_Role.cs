
namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class User_Role
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
    }
}
