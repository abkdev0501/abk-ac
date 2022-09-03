using Arity.Data.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Arity.Data.Dto
{
    public class TaskDTO
    {
        public int TaskId { get; set; }
        public int TaskUserId { get; set; }
        [DataType(DataType.MultilineText)]
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
        public string CreatedOnString { get { return CreatedOn != null ? CreatedOn.ToString("dd/MM/yyyy") : ""; } }
        public string StatusString { get { return StatusId > 0 ? Enum.GetName(typeof(EnumHelper.TaskStatus), StatusId) : string.Empty; } }
        public string DueDateString { get { return DueDate.HasValue ? DueDate.Value.ToString("dd/MM/yyyy") : ""; } }
        public int? ClientId { get; set; }
        public int? Priorities { get; set; }
        public string Remarks { get; set; }
        public bool IsChargeble { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string CompletedOnString { get; set; }
        public int AddedBy { get; set; }
        public string AddedByName { get; set; }
        public string ClientName { get; set; }
        public string CreatedByString { get; set; }
        public decimal? ChargeAmount { get; set; }
        public string PrioritiesString { get { return Priorities != null ? Enum.GetName(typeof(EnumHelper.TaskPrioritis), Priorities) : string.Empty; } }
        public int TotalRecords { get;set; }
    }
}
