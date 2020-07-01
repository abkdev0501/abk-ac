using System;

namespace Arity.Data.Dto
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyBanner { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public string Type { get; set; }
        public int? PreferedColor { get; set; }
        public string Prefix { get; set; }
        public bool IsAvailableForDelete { get; set; }
    }
}
