using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Data.Dto
{
    public class UsersDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string Pincode { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDated { get; set; }
        public long UserTypeId { get; set; }
        public long Id { get; set; }
        public string UserType { get; set; }
    }
}
