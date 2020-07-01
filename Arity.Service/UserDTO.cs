using System;

namespace Arity.Service
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; }
        public DateTime LoginAt { get; set; }
        public string IpAddress { get; set; }
    }
}

