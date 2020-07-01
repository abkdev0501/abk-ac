using System;

namespace Arity.Data.Dto
{
    public class GodownDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime? Open { get; set; }
        public DateTime? Close { get; set; }
    }
}
