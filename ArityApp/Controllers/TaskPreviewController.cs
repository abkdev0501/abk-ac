using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Data.Models.AuxiliaryModels;
using Arity.Service.Contract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ArityApp.Controllers
{
    [Authorize]
    public class TaskPreviewController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IAccountService _accountService;

        public TaskPreviewController(ITaskService taskService,
           IAccountService accountService)
        {
            _taskService = taskService;
            _accountService = accountService;
        }

        public ActionResult Index()
        {
            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year - 1, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View();
        }

        /// <summary>
        /// Load list of task by from and to date of user and all
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> LoadTask(DtParameters dtParameters)
        {
            try
            {
                var result = await _taskService.GetAllTaskDetail(dtParameters);

                var totalResultsCount = result.FirstOrDefault()?.TotalRecords ?? 0;

                // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
                var filteredResultsCount = result.Count();

                var jsonResult = Json(new
                {
                    draw = dtParameters.Draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = filteredResultsCount,
                    data = dtParameters.Length == -1 ? result.ToList() : result
                        .Skip(dtParameters.Start)
                        .Take(dtParameters.Length)
                        .ToList()
                });

                jsonResult.MaxJsonLength = int.MaxValue;

                return jsonResult;
            }
            catch (Exception)
            {
                return Json(new { draw = 0, recordsTotal = 0, recordsFiltered = 0, data = new List<Tasks>() }, JsonRequestBehavior.AllowGet);
            }
        }

      
        public IQueryable<TaskDTO> ApplyFilter(Dictionary<string, string> columnFilter, IQueryable<TaskDTO> taskDTOs)
        {
            if (columnFilter.ContainsKey("TaskName") && !string.IsNullOrWhiteSpace(columnFilter["TaskName"]))
            {
                var taskName = columnFilter["TaskName"].ToUpper();
                taskDTOs = taskDTOs.Where(x => x.TaskName.ToUpper().Contains(taskName));
            }

            if (columnFilter.ContainsKey("UserName") && !string.IsNullOrWhiteSpace(columnFilter["UserName"]))
            {
                var userName = columnFilter["UserName"].ToUpper();
                taskDTOs = taskDTOs.Where(x => x.UserName.ToUpper().Contains(userName));
            }

            if (columnFilter.ContainsKey("ClientName") && !string.IsNullOrWhiteSpace(columnFilter["ClientName"]))
            {
                var clientName = columnFilter["ClientName"].ToUpper();
                taskDTOs = taskDTOs.Where(x => x.ClientName.ToUpper().Contains(clientName));
            }

            if (columnFilter.ContainsKey("Description") && !string.IsNullOrWhiteSpace(columnFilter["Description"]))
            {
                var description = columnFilter["Description"].ToUpper();
                taskDTOs = taskDTOs.Where(x => x.Description.ToUpper().Contains(description));
            }

            if (columnFilter.ContainsKey("Status") && !string.IsNullOrWhiteSpace(columnFilter["Status"]) && columnFilter["Status"] != "null")
            {
                var status = columnFilter["Status"].Split(',').Select(x => int.Parse(x)).ToArray();
                taskDTOs = taskDTOs.Where(x => status.Contains(x.StatusId));
            }

            if (columnFilter.ContainsKey("UserComment") && !string.IsNullOrWhiteSpace(columnFilter["UserComment"]))
            {
                var userComment = columnFilter["UserComment"].ToUpper();
                taskDTOs = taskDTOs.Where(x => x.UserComment.ToUpper().Contains(userComment));
            }

            if (columnFilter.ContainsKey("IsChargeble") && bool.TryParse(columnFilter["IsChargeble"], out var isChargable))
            {
                taskDTOs = taskDTOs.Where(x => x.IsChargeble == isChargable);
            }

            if (columnFilter.ContainsKey("Priorities") && !string.IsNullOrWhiteSpace(columnFilter["Priorities"]) && columnFilter["Priorities"] != "null")
            {
                var priorities = columnFilter["Priorities"].Split(',').Select(x => int.Parse(x)).ToArray();
                taskDTOs = taskDTOs.Where(x => priorities.Contains(x.Priorities ?? 0));
            }

            if (columnFilter.ContainsKey("CreatedBy") && !string.IsNullOrWhiteSpace(columnFilter["CreatedBy"]))
            {
                var createdBy = columnFilter["CreatedBy"].ToUpper();
                taskDTOs = taskDTOs.Where(x => x.CreatedByString.ToUpper().Contains(createdBy));
            }

            if (columnFilter.ContainsKey("CreatedOn") && !string.IsNullOrWhiteSpace(columnFilter["CreatedOn"]))
            {
                var (fromDate, toDate) = GetDatesFromRange(columnFilter["CreatedOn"]);
                if (fromDate != null)
                {
                    taskDTOs = taskDTOs.Where(x => x.CreatedOn == null || x.CreatedOn >= fromDate);
                }

                if (toDate != null)
                {
                    taskDTOs = taskDTOs.Where(x => x.CreatedOn == null || x.CreatedOn <= toDate);
                }
            }

            if (columnFilter.ContainsKey("DueDate") && !string.IsNullOrWhiteSpace(columnFilter["DueDate"]))
            {
                var (fromDate, toDate) = GetDatesFromRange(columnFilter["DueDate"]);
                if (fromDate != null)
                {
                    taskDTOs = taskDTOs.Where(x => x.DueDate == null || x.DueDate >= fromDate);
                }

                if (toDate != null)
                {
                    taskDTOs = taskDTOs.Where(x => x.DueDate == null || x.DueDate <= toDate);
                }
            }

            return taskDTOs;
        }

        private (DateTime? fromDate, DateTime? toDate) GetDatesFromRange(string range)
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!string.IsNullOrWhiteSpace(range))
            {
                var dates = range.Split('-');
                if (!string.IsNullOrWhiteSpace(dates[0]))
                {
                    fromDate = DateTime.ParseExact(dates[0], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrWhiteSpace(dates[1]))
                {
                    toDate = DateTime.ParseExact(dates[1], "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1).AddMilliseconds(-1);
                }
            }
            return (fromDate, toDate);
        }

        public async Task<ActionResult> AddUpdateTask(int id)
        {
            try
            {
                var users = await _accountService.GetAllUsers();
                var task = await _taskService.GetTask(id);
                if (task == null)
                    task = new TaskDTO();
                ViewBag.Users = new SelectList(users.Where(_ => _.UserTypeId != (int)Arity.Service.Core.UserType.User), "Id", "FullName", task.UserId);
                ViewBag.Clients = new SelectList(users.Where(_ => _.UserTypeId == (int)Arity.Service.Core.UserType.User), "Id", "FullName", task.ClientId);
                ViewBag.Status = new SelectList(Enum.GetValues(typeof(EnumHelper.TaskStatus))
               .Cast<EnumHelper.TaskStatus>()
               .Select(t => new
               {
                   Id = ((int)t),
                   Name = t.ToString()
               }), "Id", "Name", task.StatusId);

                ViewBag.Prioritis = new SelectList(Enum.GetValues(typeof(EnumHelper.TaskPrioritis))
               .Cast<EnumHelper.TaskPrioritis>()
               .Select(t => new
               {
                   Id = ((int)t),
                   Name = t.ToString()
               }), "Id", "Name", task.Priorities);
                return PartialView("_TaskWizard", task);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddUpdateTask(TaskDTO task)
        {
            try
            {
                await _taskService.AddUpdateTask(task);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteTask(int taskId)
        {
            try
            {
                await _taskService.DeleteTask(taskId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }
    }
}