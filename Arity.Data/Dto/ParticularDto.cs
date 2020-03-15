using System;

namespace Arity.Data.Dto
{
    public class ParticularDto
    {
        public int Id { get; set; }

        public string ParticularSF { get; set; }

        public string ParticularFF { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedDateString { get; set; }

        public DateTime UpdatedDate { get; set; }
        public bool IsExclude { get; set; }
    }
}
