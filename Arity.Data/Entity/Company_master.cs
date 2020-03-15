
namespace Arity.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Company_master
    {
        [Key]
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyBanner { get; set; }
        public string Address { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string Type { get; set; }
        public Nullable<int> PreferedColor { get; set; }
        public string Prefix { get; set; }
    }
}
