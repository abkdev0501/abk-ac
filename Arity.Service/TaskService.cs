using System;
using System.Collections.Generic;
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
        public TaskService()
        {
            _dbContext = new RMNEntities();
        }

        public async Task<List<TaskDTO>> GetAll(DateTime fromDate, DateTime toDate)
        {
            if (SessionHelper.UserTypeId != (int)Arity.Service.Core.UserType.MasterAdmin)
            {
                return (from task in _dbContext.Tasks.ToList()
                        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                        join user in _dbContext.Users.ToList() on userTask.UserId equals user.Id
                        join assignedTo in _dbContext.Users.ToList() on task.ClientId ?? 0 equals assignedTo.Id
                        where userTask.CreatedOn >= fromDate && userTask.CreatedOn <= toDate && user.Id == SessionHelper.UserId
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
                            StatusString = Enum.GetName(typeof(EnumHelper.TaskStatus), userTask.Status),
                            StatusId = userTask.Status,
                            ClientName = assignedTo.FullName,
                            IsChargeble = task.IsChargeble ?? false
                        }).OrderBy(_ => _.StatusId).ToList();
            }
            else
            {

                return (from task in _dbContext.Tasks.ToList()
                        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                        join user in _dbContext.Users.ToList() on userTask.UserId equals user.Id
                        join assignedTo in _dbContext.Users.ToList() on task.ClientId ?? 0 equals assignedTo.Id
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
                            CreatedOnString = userTask.CreatedOn.ToString("dd/MM/yyyy"),
                            UserName = user.FullName,
                            StatusString = Enum.GetName(typeof(EnumHelper.TaskStatus), userTask.Status),
                            StatusId = userTask.Status,
                            ClientName = assignedTo.FullName,
                            IsChargeble = task.IsChargeble ?? false
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
                        IsChargeble = task.IsChargeble ?? false
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

                var existingUserTask = await _dbContext.UserTasks.FirstOrDefaultAsync(_ => _.Id == task.TaskUserId);
                existingUserTask.Status = task.StatusId;
                existingUserTask.Comment = task.UserComment;
                existingUserTask.UserId = task.UserId;
                existingUserTask.ModifiedOn = DateTime.Now;
                existingUserTask.DueDate = task.DueDate;

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
            }
        }

        public async Task<List<TaskDTO>> GetAll(int userId, int typeId)
        {
            if (typeId == (int)Arity.Service.Core.UserType.MasterAdmin)
                return (from task in _dbContext.Tasks.ToList()
                        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                        join user in _dbContext.Users.ToList() on userTask.UserId equals user.Id
                        join client in _dbContext.Users.ToList() on (task.ClientId ?? 0) equals client.Id
                        where userTask.CreatedOn >= Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01)) && task.Active == true
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
                            StatusId = userTask.Status
                        }).OrderBy(_ => _.StatusId).ToList();
            else if (typeId == (int)Arity.Service.Core.UserType.User)
                return (from task in _dbContext.Tasks.ToList()
                        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                        join user in _dbContext.Users.ToList() on userTask.UserId equals user.Id
                        join client in _dbContext.Users.ToList() on (task.ClientId ?? 0) equals client.Id
                        where task.ClientId == userId  && task.Active == true
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
                            StatusId = userTask.Status
                        }).OrderBy(_ => _.StatusId).ToList();
            else
                return (from task in _dbContext.Tasks.ToList()
                        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
                        join user in _dbContext.Users.ToList() on userTask.UserId equals user.Id
                        join client in _dbContext.Users.ToList() on (task.ClientId ?? 0) equals client.Id
                        where userTask.UserId == userId && task.Active == true && userTask.CreatedOn >= Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01))
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
                            StatusId = userTask.Status
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

