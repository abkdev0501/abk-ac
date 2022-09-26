using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Extensions;
using Arity.Data.Helpers;
using Arity.Data.Models.AuxiliaryModels;
using Arity.Infra.Interface;
using Arity.Service.Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Arity.Service
{
    public class TaskService : ITaskService
    {
        private readonly RMNEntities _dbContext;
        private readonly IInvoiceService _invoiceService;
        private readonly ITaskRepository _taskRepository;

        public TaskService(RMNEntities rmnEntities, IInvoiceService invoiceService, ITaskRepository taskRepository)
        {
            _dbContext = rmnEntities;
            _invoiceService = invoiceService;
            _taskRepository = taskRepository;
        }


        public async Task<List<TaskDTO>> GetAllTaskDetail(DtParameters dtParameters)
        {
            var sortColumn = string.Empty;
            var sortOrder = string.Empty;

            if (dtParameters.Order != null)
            {
                // in this example we just default sort on the 1st column
                sortColumn = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                sortOrder = dtParameters.Order[0].Dir.ToString();
            }

            Dictionary<string, object> filterParams;
            var columnFilter = dtParameters.Columns.Where(x => !string.IsNullOrWhiteSpace(x.Search.Value)).ToDictionary(x => x.Name, x => x.Search.Value);
            if (columnFilter.Count > 0)
            {
                filterParams = GetDynamicParamsForFilter(columnFilter);
            }
            else
            {
                var fromDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Date;
                var toDate = fromDate.AddDays(1).AddMilliseconds(-1);
                filterParams = new Dictionary<string, object>();
                filterParams.Add("DueDateFrom", fromDate);
                filterParams.Add("DueDateTo", toDate);
            }

            return await _taskRepository.GetAllTaskDetail(SessionHelper.UserId, SessionHelper.UserTypeId, dtParameters.Start, dtParameters.Length, sortColumn, sortOrder, filterParams);
        }

        public async Task<TaskDTO> GetTask(int id)
        {
            return await _taskRepository.GetTask(id);
            //return (from task in _dbContext.Tasks.ToList()
            //        join userTask in _dbContext.UserTasks.ToList() on task.Id equals userTask.TaskId
            //        join user in _dbContext.Users.ToList() on userTask.UserId equals user.Id
            //        where task.Id == id
            //        select new TaskDTO
            //        {
            //            TaskId = task.Id,
            //            TaskUserId = userTask.Id,
            //            UserComment = userTask.Comment,
            //            UserId = userTask.UserId,
            //            Description = task.Description,
            //            TaskName = task.Name,
            //            DueDate = userTask.DueDate,
            //            CreatedBy = userTask.CreatedBy,
            //            CreatedOn = userTask.CreatedOn,
            //            StatusId = userTask.Status,
            //            Active = task.Active,
            //            ClientId = task.ClientId,
            //            CompletedOn = task.CompletedOn,
            //            Priorities = task.Priorities,
            //            Remarks = task.Remarks,
            //            IsChargeble = task.IsChargeble ?? false,
            //            ChargeAmount = task.ChargeAmount
            //        }).FirstOrDefault();
        }

        public async Task AddUpdateTask(TaskDTO task)
        {
            if (task.TaskId > 0)
            {
                var updateResult = await _taskRepository.UpdateTask(task);
                if (updateResult.InvoiceCreate)
                {
                    await CreateInvoice(task);
                }

                //var existingTask = await _dbContext.Tasks.FirstOrDefaultAsync(_ => _.Id == task.TaskId);
                //existingTask.Name = task.TaskName;
                //existingTask.Active = task.Active;
                //existingTask.Status = task.StatusId;
                //existingTask.Description = task.Description;
                //existingTask.ClientId = task.ClientId;
                //existingTask.Priorities = task.Priorities;
                //existingTask.Remarks = task.Remarks;
                //existingTask.IsChargeble = task.IsChargeble;
                //existingTask.CompletedOn = task.CompletedOn;
                //existingTask.ModifiedOn = DateTime.Now;
                //existingTask.ChargeAmount = task.ChargeAmount;


                //var existingUserTask = await _dbContext.UserTasks.FirstOrDefaultAsync(_ => _.Id == task.TaskUserId);
                //existingUserTask.Status = task.StatusId;
                //existingUserTask.Comment = task.UserComment;
                //existingUserTask.UserId = task.UserId;
                //existingUserTask.ModifiedOn = DateTime.Now;
                //existingUserTask.DueDate = task.DueDate;

                //if (task.IsChargeble && existingUserTask.Active != task.Active && task.Active)
                //    await CreateInvoice(task);

                //await _dbContext.SaveChangesAsync();

            }
            else
            {
                var createResult = await _taskRepository.CreateTask(Convert.ToInt32(SessionHelper.UserId), Convert.ToInt32(SessionHelper.UserTypeId), task);

                if (createResult.InvoiceCreate)
                {
                    await CreateInvoice(task);
                }

                //var taskDetail = new Tasks();
                //taskDetail.CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId);
                //taskDetail.AddedBy = Convert.ToInt32(SessionHelper.UserId);
                //taskDetail.Name = task.TaskName;
                //taskDetail.Description = task.Description;
                //taskDetail.Status = task.StatusId;
                //taskDetail.Active = task.Active;
                //taskDetail.CreatedOn = DateTime.Now;
                //taskDetail.ClientId = task.ClientId;
                //taskDetail.Priorities = task.Priorities;
                //taskDetail.Remarks = task.Remarks;
                //taskDetail.IsChargeble = task.IsChargeble;
                //taskDetail.CompletedOn = task.CompletedOn;
                //taskDetail.ChargeAmount = task.ChargeAmount;
                //_dbContext.Tasks.Add(taskDetail);
                //await _dbContext.SaveChangesAsync();

                //_dbContext.UserTasks.Add(new UserTask
                //{
                //    Active = task.Active,
                //    Comment = task.UserComment,
                //    TaskId = taskDetail.Id,
                //    UserId = task.UserId,
                //    Status = task.StatusId,
                //    DueDate = task.DueDate,
                //    CreatedOn = DateTime.Now,
                //    CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId),
                //    AddedBy = Convert.ToInt32(SessionHelper.UserId)
                //});
                //await _dbContext.SaveChangesAsync();

                //if (task.IsChargeble && task.Active)
                //    await CreateInvoice(task);
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
            return await _taskRepository.GetAll(userId, typeId);
        }

        public async Task DeleteTask(int taskId)
        {
            await _taskRepository.DeleteTask(taskId);
            //_dbContext.UserTasks.RemoveRange(_dbContext.UserTasks.Where(_ => _.TaskId == taskId).ToList());
            //_dbContext.Tasks.RemoveRange(_dbContext.Tasks.Where(_ => _.Id == taskId).ToList());
            //await _dbContext.SaveChangesAsync();
        }

        #region Private Methods

        private Dictionary<string, object> GetDynamicParamsForFilter(Dictionary<string, string> columnFilter)
        {
            Dictionary<string, object> filterParams = new Dictionary<string, object>();

            if (columnFilter.ContainsKey("TaskName") && !string.IsNullOrWhiteSpace(columnFilter["TaskName"]))
                filterParams.Add("TaskName", columnFilter["TaskName"]);

            if (columnFilter.ContainsKey("UserName") && !string.IsNullOrWhiteSpace(columnFilter["UserName"]))
                filterParams.Add("UserName", columnFilter["UserName"]);

            if (columnFilter.ContainsKey("ClientName") && !string.IsNullOrWhiteSpace(columnFilter["ClientName"]))
                filterParams.Add("ClientName", columnFilter["ClientName"]);

            if (columnFilter.ContainsKey("Description") && !string.IsNullOrWhiteSpace(columnFilter["Description"]))
                filterParams.Add("Description", columnFilter["Description"]);

            if (columnFilter.ContainsKey("Status") && !string.IsNullOrWhiteSpace(columnFilter["Status"]) && columnFilter["Status"] != "null")
                filterParams.Add("Status", columnFilter["Status"]);

            if (columnFilter.ContainsKey("UserComment") && !string.IsNullOrWhiteSpace(columnFilter["UserComment"]))
                filterParams.Add("UserComment", columnFilter["UserComment"]);

            if (columnFilter.ContainsKey("IsChargeble") && bool.TryParse(columnFilter["IsChargeble"], out var isChargable))
                filterParams.Add("IsChargeble", isChargable);

            if (columnFilter.ContainsKey("Priorities") && !string.IsNullOrWhiteSpace(columnFilter["Priorities"]) && columnFilter["Priorities"] != "null")
                filterParams.Add("Priorities", columnFilter["Priorities"]);

            if (columnFilter.ContainsKey("CreatedBy") && !string.IsNullOrWhiteSpace(columnFilter["CreatedBy"]))
                filterParams.Add("CreatedBy", columnFilter["CreatedBy"]);

            if (columnFilter.ContainsKey("CreatedOn") && !string.IsNullOrWhiteSpace(columnFilter["CreatedOn"]))
            {
                var dates = columnFilter["CreatedOn"].GetDatesFromRange();
                if (dates.Item1 != null)
                    filterParams.Add("CreatedOnFrom", dates.Item1);

                if (dates.Item2 != null)
                    filterParams.Add("CreatedOnTo", dates.Item2);
            }

            if (columnFilter.ContainsKey("DueDate") && !string.IsNullOrWhiteSpace(columnFilter["DueDate"]))
            {
                var dates = columnFilter["DueDate"].GetDatesFromRange();
                if (dates.Item1 != null)
                    filterParams.Add("DueDateFrom", dates.Item1);

                if (dates.Item2 != null)
                    filterParams.Add("DueDateTo", dates.Item2);
            }

            return filterParams;
        }

        #endregion
    }
}

