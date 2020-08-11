namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Role
    {
        [Key]
        public long Id { get; set; }
        public string RoleName { get; set; }
    } 
}
