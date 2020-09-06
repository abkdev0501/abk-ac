using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service;
using Arity.Service.Contract;
using Arity.Web.Extensions;
using Arity.Web.Models.AuxiliaryModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ArityApp.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IAccountService _accountService;

        public TaskController(ITaskService taskService,
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
                var searchBy = dtParameters.Search?.Value;

                // if we have an empty search then just order the results by Id ascending
                var orderCriteria = "TaskId";
                var orderAscendingDirection = false;

                if (dtParameters.Order != null)
                {
                    // in this example we just default sort on the 1st column
                    orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                    orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
                }

                var result = await _taskService.GetAll();
                var totalResultsCount = result.Count();
                if (!string.IsNullOrEmpty(searchBy))
                {
                    result = result.Where(r => r.UserComment != null && r.UserComment.ToUpper().Contains(searchBy.ToUpper()) ||
                                               r.Description != null && r.Description.ToUpper().Contains(searchBy.ToUpper()) ||
                                               r.TaskName != null && r.TaskName.ToUpper().Contains(searchBy.ToUpper()) ||
                                               //r.DueDate != null && r.DueDate.Value.ToString("dd/MM/yyyy").Contains(searchBy.ToUpper()) ||
                                               //r.CreatedBy != null && r.CreatedByString.ToUpper().Contains(searchBy.ToUpper()) ||
                                               //r.CreatedOn != null && r.CreatedOn.ToString("dd/MM/yyyy").Contains(searchBy.ToUpper()) ||
                                               r.UserName != null && r.UserName.ToUpper().Contains(searchBy.ToUpper()) ||
                                               //r.PrioritiesString != null && r.PrioritiesString.ToUpper().Contains(searchBy.ToUpper()) ||
                                               r.ClientName != null && r.ClientName.ToUpper().Contains(searchBy.ToUpper()) ||
                                               r.ChargeAmount != null && r.ChargeAmount.Value.ToString().Contains(searchBy.ToUpper()) ||
                                               r.IsChargeble.ToString().Contains(searchBy.ToUpper())
                                               //r.StatusString != null && r.StatusString.ToUpper().Contains(searchBy.ToUpper())
                                               );
                }

                result = orderAscendingDirection ? result.OrderByDynamic(orderCriteria.Replace("String",""), DtOrderDir.Asc) : result.OrderByDynamic(orderCriteria.Replace("String", ""), DtOrderDir.Desc);

                // now just get the count of items (without the skip and take) - eg how many could be returned with filtering
                var filteredResultsCount = result.Count();


                return Json(new
                {
                    draw = dtParameters.Draw,
                    recordsTotal = totalResultsCount,
                    recordsFiltered = filteredResultsCount,
                    data = result
                        .Skip(dtParameters.Start)
                        .Take(dtParameters.Length)
                        .ToList()
                });

                //return Json(new DtResult<TaskDTO>
                //{
                //    Draw = dtParameters.Draw,
                //    RecordsTotal = totalResultsCount,
                //    RecordsFiltered = filteredResultsCount,
                //    Data = result
                //        .Skip(dtParameters.Start)
                //        .Take(dtParameters.Length)
                //        .ToList()
                //});
            }
            catch (Exception ex)
            {
                return Json(new { draw = 0, recordsTotal = 0, recordsFiltered = 0, data = new List<Tasks>() }, JsonRequestBehavior.AllowGet);
            }
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