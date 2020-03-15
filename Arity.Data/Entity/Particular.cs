
namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Particular
    {
        [Key]
        public long Id { get; set; }
        public string ParticularSF { get; set; }
        public string ParticularFF { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public Nullable<bool> IsExclude { get; set; }
        public int CreatedBy { get; set; }

    }
}
