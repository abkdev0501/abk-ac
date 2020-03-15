namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Client_Particular_Mapping
    {
        [Key]
        public int Id { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<int> particularId { get; set; }
    }
}
