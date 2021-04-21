using System;

namespace Arity.Data.Dto
{
    public class GroupMasterDTO
    {
        public int GroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public int AddedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AddedByName { get; set; }
        public bool IsAsociatedWithClient { get; set; }
    }

    public class GroupMasterLookup
    {
        public int GroupId { get; set; }
        public string Name { get; set; }
    }
}
