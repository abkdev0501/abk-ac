using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Data.Dto
{
    public class ClientParticularMappingDto
    {
        public int Id { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<int> particularId { get; set; }
    }
}
