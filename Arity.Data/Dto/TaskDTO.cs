using System;
using System.Threading.Tasks;

namespace Arity.Data.Dto
{
    public class TaskDTO
    {
        public int TaskId { get; set; }
        public int TaskUserId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public TaskStatus Status { get; set; }
        public int StatusId { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool Active { get; set; }
        public string UserComment { get; set; }
        public int UserId { get; set; }
        public DateTime? DueDate { get; set; }
        public string UserName { get; set; }
        public string CreatedOnString { get; set; }
        public string StatusString { get; set; }
        public string DueDateString { get; set; }
        public int? ClientId { get; set; }
        public int? Priorities { get; set; }
        public string Remarks { get; set; }
        public bool IsChargeble { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string CompletedOnString { get; set; }
        public int AddedBy { get; set; }
        public string AddedByName { get; set; }
        public string ClientName { get; set; }
    }
}
