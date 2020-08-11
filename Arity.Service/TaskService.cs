using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service.Contract;

namespace Arity.Service
{
    public class TaskService : ITaskService
    {
        private readonly RMNEntities _dbContext;
        private readonly IInvoiceService _invoiceService;

        public TaskService(RMNEntities rmnEntities, IInvoiceService invoiceService)
        {
            _dbContext = rmnEntities;
            _invoiceService = invoiceService;
        }

        public async Task<List<TaskDTO>> GetAll(DateTime fromDate, DateTime toDate)
        {
            var users = _dbContext.Users.ToList();
            if (SessionHelper.UserTypeId == (int)Core.UserType.User)
            {
                return (from task in _dbContext.Tasks.ToList()
                        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                        join assignedTo in _dbContext.Users.ToList() on task.ClientId ?? 0 equals assignedTo.Id
                        where userTask.CreatedOn >= fromDate && userTask.CreatedOn <= toDate && assignedTo.Id == SessionHelper.UserId
                        select new TaskDTO
                        {
                            TaskId = task.Id,
                            TaskUserId = userTask.Id,
                            UserComment = userTask.Comment,
                            UserId = userTask.UserId,
                            Description = task.Description,
                            TaskName = task.Name,
                            DueDateString = userTask.DueDate.HasValue ? userTask.DueDate.Value.ToString("dd/MM/yyyy") : "",
                            CreatedBy = userTask.CreatedBy,
                            CreatedByString = users.FirstOrDefault(_ => _.Id == userTask.AddedBy)?.FullName ?? string.Empty,
                            CreatedOnString = userTask.CreatedOn.ToString("dd/MM/yyyy"),
                            UserName = users.FirstOrDefault(_ => _.Id == userTask.UserId)?.FullName ?? string.Empty,
                            StatusString = Enum.GetName(typeof(EnumHelper.TaskStatus), userTask.Status),
                            PriorityString = Enum.GetName(typeof(EnumHelper.TaskPrioritis), task.Priorities),
                            StatusId = userTask.Status,
                            ClientName = assignedTo.FullName,
                            IsChargeble = task.IsChargeble ?? false,
                            ChargeAmount = task.ChargeAmount
                        }).OrderBy(_ => _.StatusId).ToList();
            }
            else
            {

                return (from task in _dbContext.Tasks.ToList()
                        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                        where userTask.CreatedOn >= fromDate && userTask.CreatedOn <= toDate
                        select new TaskDTO
                        {
                            TaskId = task.Id,
                            TaskUserId = userTask.Id,
                            UserComment = userTask.Comment,
                            UserId = userTask.UserId,
                            Description = task.Description,
                            TaskName = task.Name,
                            DueDateString = userTask.DueDate.HasValue ? userTask.DueDate.Value.ToString("dd/MM/yyyy") : "",
                            CreatedBy = userTask.CreatedBy,
                            CreatedByString = users.FirstOrDefault(_ => _.Id == userTask.AddedBy)?.FullName??string.Empty,
                            CreatedOnString = userTask.CreatedOn.ToString("dd/MM/yyyy"),
                            UserName = users.FirstOrDefault(_ => _.Id == userTask.UserId)?.FullName ?? string.Empty,
                            StatusString = Enum.GetName(typeof(EnumHelper.TaskStatus), userTask.Status),
                            PriorityString = Enum.GetName(typeof(EnumHelper.TaskPrioritis), task.Priorities),
                            StatusId = userTask.Status,
                            ClientName = users.FirstOrDefault(_ => _.Id == (task.ClientId ?? 0))?.FullName ?? string.Empty,
                            IsChargeble = task.IsChargeble ?? false,
                            ChargeAmount = task.ChargeAmount
                        }).OrderBy(_ => _.StatusId).ToList();
            }
        }

        public async Task<TaskDTO> GetTask(int id)
        {
            return (from task in _dbContext.Tasks.ToList()
                    join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                    join user in _dbContext.Users.ToList() on userTask.UserId equals user.Id
                    where task.Id == id
                    select new TaskDTO
                    {
                        TaskId = task.Id,
                        TaskUserId = userTask.Id,
                        UserComment = userTask.Comment,
                        UserId = userTask.UserId,
                        Description = task.Description,
                        TaskName = task.Name,
                        DueDate = userTask.DueDate,
                        CreatedBy = userTask.CreatedBy,
                        CreatedOn = userTask.CreatedOn,
                        StatusId = userTask.Status,
                        Active = task.Active,
                        ClientId = task.ClientId,
                        CompletedOn = task.CompletedOn,
                        Priorities = task.Priorities,
                        Remarks = task.Remarks,
                        IsChargeble = task.IsChargeble ?? false,
                        ChargeAmount = task.ChargeAmount
                    }).FirstOrDefault();
        }

        public async Task AddUpdateTask(TaskDTO task)
        {
            if (task.TaskId > 0)
            {

                var existingTask = await _dbContext.Tasks.FirstOrDefaultAsync(_ => _.Id == task.TaskId);
                existingTask.Name = task.TaskName;
                existingTask.Active = task.Active;
                existingTask.Status = task.StatusId;
                existingTask.Description = task.Description;
                existingTask.ClientId = task.ClientId;
                existingTask.Priorities = task.Priorities;
                existingTask.Remarks = task.Remarks;
                existingTask.IsChargeble = task.IsChargeble;
                existingTask.CompletedOn = task.CompletedOn;
                existingTask.ModifiedOn = DateTime.Now;
                existingTask.ChargeAmount = task.ChargeAmount;


                var existingUserTask = await _dbContext.UserTasks.FirstOrDefaultAsync(_ => _.Id == task.TaskUserId);
                existingUserTask.Status = task.StatusId;
                existingUserTask.Comment = task.UserComment;
                existingUserTask.UserId = task.UserId;
                existingUserTask.ModifiedOn = DateTime.Now;
                existingUserTask.DueDate = task.DueDate;

                if (task.IsChargeble && existingUserTask.Active != task.Active && task.Active)
                    await CreateInvoice(task);

                await _dbContext.SaveChangesAsync();

            }
            else
            {
                var taskDetail = new Tasks();
                taskDetail.CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId);
                taskDetail.AddedBy = Convert.ToInt32(SessionHelper.UserId);
                taskDetail.Name = task.TaskName;
                taskDetail.Description = task.Description;
                taskDetail.Status = task.StatusId;
                taskDetail.Active = task.Active;
                taskDetail.CreatedOn = DateTime.Now;
                taskDetail.ClientId = task.ClientId;
                taskDetail.Priorities = task.Priorities;
                taskDetail.Remarks = task.Remarks;
                taskDetail.IsChargeble = task.IsChargeble;
                taskDetail.CompletedOn = task.CompletedOn;
                taskDetail.ChargeAmount = task.ChargeAmount;
                _dbContext.Tasks.Add(taskDetail);
                await _dbContext.SaveChangesAsync();

                _dbContext.UserTasks.Add(new UserTask
                {
                    Active = task.Active,
                    Comment = task.UserComment,
                    TaskId = taskDetail.Id,
                    UserId = task.UserId,
                    Status = task.StatusId,
                    DueDate = task.DueDate,
                    CreatedOn = DateTime.Now,
                    CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId),
                    AddedBy = Convert.ToInt32(SessionHelper.UserId)
                });
                await _dbContext.SaveChangesAsync();

                if (task.IsChargeble && task.Active)
                    await CreateInvoice(task);
            }
        }

        /// <summary>
        /// Create invoice based on task completion
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async Task CreateInvoice(TaskDTO task)
        {
            try
            {
                var yearStartAt = Convert.ToDateTime(ConfigurationManager.AppSettings["YearStart"].Replace("XXXX", ((task.CompletedOn ?? DateTime.Now).Month >= 4 ? (task.CompletedOn ?? DateTime.Now).Year : ((task.CompletedOn ?? DateTime.Now).Year - 1)).ToString()));
                var yearEndedAt = Convert.ToDateTime(ConfigurationManager.AppSettings["YearEnd"].Replace("XXXX", ((task.CompletedOn ?? DateTime.Now).Month > 3 ? ((task.CompletedOn ?? DateTime.Now).Year + 1) : (task.CompletedOn ?? DateTime.Now).Year).ToString()));

                var companyId = await _invoiceService.GetCompanyByClientId(task.ClientId ?? 0);

                await _invoiceService.AddUpdateInvoiceEntry(companyId, new InvoiceEntry
                {
                    ClientId = Convert.ToInt64(task.ClientId ?? 0),
                    Amount = task.ChargeAmount ?? 0,
                    Remarks = task.Remarks,
                    CompanyId = companyId,
                    InvoiceDate = task.CompletedOn ?? DateTime.Now,
                    Year = yearStartAt.Year.ToString().Substring(2, 2) + "/" + yearEndedAt.Year.ToString().Substring(2, 2),
                    ParticularId = 1
                });
            }
            catch
            {
            }
        }

        public async Task<List<TaskDTO>> GetAll(int userId, int typeId)
        {
            if (typeId == (int)Arity.Service.Core.UserType.User)
                return (from task in _dbContext.Tasks.ToList()
                        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                        join user in _dbContext.Users.ToList() on userTask.UserId equals user.Id
                        join client in _dbContext.Users.ToList() on (task.ClientId ?? 0) equals client.Id
                        where task.ClientId == userId
                        && task.Status != (int)EnumHelper.TaskStatus.OnHold
                        && task.Status != (int)EnumHelper.TaskStatus.Cancel
                        select new TaskDTO
                        {
                            TaskId = task.Id,
                            TaskUserId = userTask.Id,
                            UserComment = userTask.Comment,
                            UserId = userTask.UserId,
                            Description = task.Description,
                            TaskName = task.Name,
                            DueDateString = userTask.DueDate.HasValue ? userTask.DueDate.Value.ToString("dd/MM/yyyy") : "",
                            CreatedBy = userTask.CreatedBy,
                            CreatedOnString = userTask.CreatedOn.ToString("dd/MM/yyyy"),
                            UserName = user.FullName,
                            ClientName = client.FullName,
                            Remarks = task.Remarks,
                            StatusString = Enum.GetName(typeof(EnumHelper.TaskStatus), userTask.Status),
                            StatusId = userTask.Status,
                            ChargeAmount = task.ChargeAmount
                        }).OrderBy(_ => _.StatusId).ToList();
            else
                return (from task in _dbContext.Tasks.ToList()
                        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                        join user in _dbContext.Users.ToList() on userTask.UserId equals user.Id
                        join client in _dbContext.Users.ToList() on (task.ClientId ?? 0) equals client.Id
                        where userTask.UserId == userId
                        && task.Status != (int)EnumHelper.TaskStatus.Complete
                        && task.Status != (int)EnumHelper.TaskStatus.Cancel
                        && task.Status != (int)EnumHelper.TaskStatus.OnHold
                        select new TaskDTO
                        {
                            TaskId = task.Id,
                            TaskUserId = userTask.Id,
                            UserComment = userTask.Comment,
                            UserId = userTask.UserId,
                            Description = task.Description,
                            TaskName = task.Name,
                            DueDateString = userTask.DueDate.HasValue ? userTask.DueDate.Value.ToString("dd/MM/yyyy") : "",
                            CreatedBy = userTask.CreatedBy,
                            CreatedOnString = userTask.CreatedOn.ToString("dd/MM/yyyy"),
                            UserName = user.FullName,
                            ClientName = client.FullName,
                            Remarks = task.Remarks,
                            StatusString = Enum.GetName(typeof(EnumHelper.TaskStatus), userTask.Status),
                            StatusId = userTask.Status,
                            ChargeAmount = task.ChargeAmount
                        }).OrderBy(_ => _.StatusId).ToList();
        }

        public async Task DeleteTask(int taskId)
        {
            _dbContext.UserTasks.RemoveRange(_dbContext.UserTasks.Where(_ => _.TaskId == taskId).ToList());
            _dbContext.Tasks.RemoveRange(_dbContext.Tasks.Where(_ => _.Id == taskId).ToList());
            await _dbContext.SaveChangesAsync();
        }
    }
}

