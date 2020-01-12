using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Data.Dto
{
   public class CompanyDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyBanner { get; set; }
        public string Address { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}
