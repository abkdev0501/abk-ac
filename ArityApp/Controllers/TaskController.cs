using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service;
using Arity.Service.Contract;
using System;
using System.Collections.Generic;
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
           IAccountService accountService )
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
        public async Task<JsonResult> LoadTask(string from, string to)
        {
            try
            {
                DateTime fromDate = Convert.ToDateTime(from);
                DateTime toDate = Convert.ToDateTime(to);

                toDate = toDate + new TimeSpan(23, 59, 59);
                fromDate = fromDate + new TimeSpan(00, 00, 1);
                var invoiceList = await _taskService.GetAll();
                return Json(new { data = invoiceList }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { data = new List<Tasks>() }, JsonRequestBehavior.AllowGet);
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