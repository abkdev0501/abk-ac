using Arity.Data.Dto;
using Arity.Data.Helpers;
using Arity.Service;
using Arity.Service.Contract;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ArityApp.Controllers
{
    [Authorize]
    public class MasterController : Controller
    {
        private IMasterService _masterService;
        private IInvoiceService _invoiceService;

        #region Company
        /// <summary>
        /// Landing page of company
        /// </summary>
        /// <returns></returns>
        public ActionResult Company()
        {
            return View();
        }

        /// <summary>
        /// Get all companies
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadCompany()
        {
            _masterService = new MasterService();
            var companies = await _masterService.GetAllCompany();
            return Json(new { data = companies }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add/Update company
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddCompany(int id)
        {
            _masterService = new MasterService();
            var company = await _masterService.GetCompanyById(id);
            if (company == null)
                company = new CompanyDto();

            ViewBag.Color = new SelectList(Enum.GetValues(typeof(EnumHelper.CompanyColor))
               .Cast<EnumHelper.CompanyColor>()
               .Select(t => new
               {
                   Id = ((int)t),
                   Name = t.ToString()
               }), "Id", "Name", company.PreferedColor);

            return PartialView("_CompanyWizard", company);
        }

        /// <summary>
        /// Add/Update company
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<ActionResult> AddCompany(CompanyDto company)
        {
            _masterService = new MasterService();
            await _masterService.AddUpdateCompany(company);

            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Group
        /// <summary>
        /// Landing page of group
        /// </summary>
        /// <returns></returns>
        public ActionResult GroupMaster()
        {
            return View();
        }

        /// <summary>
        /// Get all companies
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadGroup()
        {
            try
            {
                _masterService = new MasterService();
                var groups = await _masterService.GetAllGroup();
                return Json(new { data = groups }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Add/Update group
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddGroup(int id)
        {
            _masterService = new MasterService();
            var groupMaster = await _masterService.GetGroupById(id);
            if (groupMaster == null)
                groupMaster = new GroupMasterDTO();
            return PartialView("_GroupMasterWizard", groupMaster);
        }

        /// <summary>
        /// Add/Update group
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddGroup(GroupMasterDTO groupMaster)
        {
            _masterService = new MasterService();
            await _masterService.AddUpdateGroup(groupMaster);

            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Client Master
        /// <summary>
        /// Landing page of client master
        /// </summary>
        /// <returns></returns>
        public ActionResult ClientMaster()
        {
            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View();
        }

        /// <summary>
        /// Get all clients
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadClient(DateTime from, DateTime to)
        {
            try
            {
                _masterService = new MasterService();
                to = to + new TimeSpan(23, 59, 59);
                from = from + new TimeSpan(00, 00, 1);
                var users = await _masterService.GetAllClient(from, to);
                return Json(new { data = users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Add/Update group
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ManageClient(int? id)
        {
            _masterService = new MasterService();
            _invoiceService = new InvoiceService();
            var clientMaster = await _masterService.GetClientById(id ?? 0);
            if (clientMaster == null)
                clientMaster = new UsersDto();

            ViewBag.Companies = new MultiSelectList(await _invoiceService.GetCompany(), "Id", "CompanyName", clientMaster.CompanyIds);
            ViewBag.Consultant = new SelectList(await _masterService.GetAllConsultant(), "ConsultantId", "Name", clientMaster.ConsultantId);
            ViewBag.Groups = new SelectList(await _masterService.GetAllGroups(), "GroupId", "Name", clientMaster.GroupId);
            ViewBag.BusinessType = new SelectList(Enum.GetValues(typeof(EnumHelper.BusinessType))
               .Cast<EnumHelper.BusinessType>()
               .Select(t => new
               {
                   Id = ((int)t),
                   Name = t.ToString()
               }), "Id", "Name", clientMaster.BusinessType);
            return View(clientMaster);
        }

        /// <summary>
        /// Add/Update group
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ManageClient(UsersDto usersDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(usersDto.FullName)
                    && !string.IsNullOrEmpty(usersDto.Email)
                    && !string.IsNullOrEmpty(usersDto.Username)
                    && !string.IsNullOrEmpty(usersDto.Password)
                    && !string.IsNullOrEmpty(usersDto.Address)
                    && !string.IsNullOrEmpty(usersDto.City))
                {
                    _masterService = new MasterService();
                    await _masterService.AddUpdateClient(usersDto);
                    TempData["Success"] = usersDto.Id > 0 ? "Client updated successfully" : "Client added successfully";
                    return RedirectToAction("ClientMaster");
                }
            }
            catch (Exception ex)
            {
            }
            ViewBag.ErrorMsg = "client not added";
            return View(usersDto);
        }
        #endregion


    }
}