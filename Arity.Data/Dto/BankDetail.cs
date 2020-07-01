using System;

namespace Arity.Data.Dto
{
    public class BankDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string AccountNo { get; set; }
        public DateTime? Open { get; set; }
        public DateTime? Close { get; set; }
    }
}
