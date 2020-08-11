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
    public class MasterController : Controller
    {
        private readonly IMasterService _masterService;
        private readonly IInvoiceService _invoiceService;

        public MasterController(IMasterService masterService,
            IInvoiceService invoiceService)
        {
            _masterService = masterService;
            _invoiceService = invoiceService;
        }

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
            var companies = await _masterService.GetAllCompany();
            return Json(new { data = companies }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add/Update company
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddCompany(int id)
        {
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
            await _masterService.AddUpdateCompany(company);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Delete company
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<ActionResult> DeleteCompany(int companyId)
        {
            try
            {
                await _masterService.DeleteCompany(companyId);

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
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

                var groups = await _masterService.GetAllGroup();
                return Json(new { data = groups }, JsonRequestBehavior.AllowGet);
            }
            catch
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

            await _masterService.AddUpdateGroup(groupMaster);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Delete group
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> DeleteGroup(int groupId)
        {

            await _masterService.DeleteGroup(groupId);

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
            return View();
        }

        /// <summary>
        /// Get all clients
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadClient(string from, string to)
        {
            try
            {
                var users = await _masterService.GetAllClient();
                return Json(new { data = users }, JsonRequestBehavior.AllowGet);
            }
            catch
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

            var clientMaster = await _masterService.GetClientById(id ?? 0);
            if (clientMaster == null)
                clientMaster = new UsersDto();
            var companies = await _invoiceService.GetCompany();
            var particulars = await _invoiceService.GetParticular();
            clientMaster.Companies = companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName
            }).ToList();

            clientMaster.Particulars = particulars.Select(p => new ParticularDto
            {
                Id = Convert.ToInt32(p.Id),
                ParticularFF = p.ParticularFF
            }).ToList();

            ViewBag.Companies = new MultiSelectList(companies, "Id", "CompanyName", clientMaster.CompanyIds);
            ViewBag.Consultant = new SelectList(await _masterService.GetAllConsultant(), "ConsultantId", "Name", clientMaster.ConsultantId);
            ViewBag.Groups = new SelectList(await _masterService.GetAllGroups(), "GroupId", "Name", clientMaster.GroupId);
            ViewBag.BusinessType = new SelectList(await _masterService.GetAllBusinessType(), "BusinessTypeId", "Name", clientMaster?.BusinessType);
            ViewBag.BusinessStatus = new SelectList(await _masterService.GetAllBusinessStatus(), "BusinessStatusId", "Name", clientMaster?.BusinessStatus);

            return View(clientMaster);
        }

        /// <summary>
        /// Add/Update group
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ManageClient(UsersDto usersDto, FormCollection fn)
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

                    var services = new List<ServiceTypes>();
                    var bankDetails = new List<BankDetail>();
                    var additionalDetails = new List<GodownDetail>();
                    int retryCount = 0;
                    for (int i = 0; i <= 9999; i++)
                    {

                        if (fn["particularId" + i] == null || fn["companyId" + i] == "")
                            retryCount += 1;
                        else
                            retryCount = 0;

                        if (retryCount > 5)
                            break;

                        if (retryCount == 0)
                        {
                            int.TryParse(fn["particularId" + i], out int ParticularId);
                            int.TryParse(fn["companyId" + i], out int CompanyId);
                            int.TryParse(fn["txtRate" + i], out int Rate);
                            services.Add(new ServiceTypes
                            {
                                Id = i,
                                ParticularId = ParticularId,
                                CompanyId = CompanyId,
                                Rate = Rate
                            });
                        }
                    }
                    retryCount = 0;
                    for (int i = 0; i <= 9999; i++)
                    {

                        if (fn["txtAName" + i] == null || fn["txtAName" + i] == "")
                            retryCount += 1;
                        else
                            retryCount = 0;

                        if (retryCount > 5)
                            break;

                        if (retryCount == 0)
                        {
                            DateTime.TryParse(fn["txtAOpen" + i], out DateTime OpenDate);
                            DateTime.TryParse(fn["txtAOpen" + i], out DateTime CloseDate);
                            additionalDetails.Add(new GodownDetail
                            {
                                Id = i,
                                Name = fn["txtAName" + i],
                                Address = fn["txtAAddress" + i],
                                Open = OpenDate != DateTime.MinValue ? OpenDate : (DateTime?)null,
                                Close = CloseDate != DateTime.MinValue ? CloseDate : (DateTime?)null
                            });
                        }
                    }
                    retryCount = 0;
                    for (int i = 0; i <= 9999; i++)
                    {

                        if (fn["txtBName" + i] == null || fn["txtBName" + i] == "")
                            retryCount += 1;
                        else
                            retryCount = 0;

                        if (retryCount > 5)
                            break;

                        if (retryCount == 0)
                        {
                            DateTime.TryParse(fn["txtBOpen" + i], out DateTime OpenDate);
                            DateTime.TryParse(fn["txtBClose" + i], out DateTime CloseDate);
                            bankDetails.Add(new BankDetail
                            {
                                Id = i,
                                Name = fn["txtBName" + i],
                                Address = fn["txtBAddress" + i],
                                AccountNo = fn["txtBAccountNo" + i],
                                Open = OpenDate != DateTime.MinValue ? OpenDate : (DateTime?)null,
                                Close = CloseDate != DateTime.MinValue ? CloseDate : (DateTime?)null
                            });
                        }
                    }

                    usersDto.Services = services.Count() > 0 ? services : null;
                    usersDto.BankDetails = bankDetails.Count() > 0 ? bankDetails : null;
                    usersDto.AdditionlPlaces = additionalDetails.Count() > 0 ? additionalDetails : null;


                    await _masterService.AddUpdateClient(usersDto);
                    TempData["Success"] = usersDto.Id > 0 ? "Client updated successfully" : "Client added successfully";
                    return RedirectToAction("ClientMaster");
                }
            }
            catch
            {
            }
            ViewBag.ErrorMsg = "client not added";
            return View(usersDto);
        }
        #endregion

        #region Notification
        /// <summary>
        /// Landing page of Notification
        /// </summary>
        /// <returns></returns>
        public ActionResult NotificationMaster()
        {
            return View();
        }

        /// <summary>
        /// Get all notifications
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadNotification()
        {
            try
            {

                var notifications = await _masterService.GetAllNotification();
                return Json(new { data = notifications }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Add/Update notification
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddNotification(int id)
        {

            var notificationMaster = await _masterService.GetNotificationById(id);
            if (notificationMaster == null)
                notificationMaster = new NotificationDTO { OnBroadcastDateTime = DateTime.Now, OffBroadcastDateTime = DateTime.Now };
            var clients = await _masterService.GetAllClient();
            clients.Add(new UsersDto { Id = 0, FullName = "All" });
            ViewBag.Clients = new SelectList(clients, "Id", "FullName", notificationMaster.ClientId);
            return PartialView("_NotificationMasterWizard", notificationMaster);
        }

        /// <summary>
        /// Add/Update Notification
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddNotification(NotificationDTO notification)
        {

            await _masterService.AddUpdateNotification(notification);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Delete Notification
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> DeleteNotification(int notificationId)
        {

            await _masterService.DeleteNotification(notificationId);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Landing page of Notes
        /// </summary>
        /// <returns></returns>
        public ActionResult NotesMaster()
        {
            return View();
        }

        /// <summary>
        /// Get all notes
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadNotes()
        {
            try
            {

                var notes = await _masterService.GetAllNotes(Convert.ToInt32(SessionHelper.UserId), Convert.ToInt32(SessionHelper.UserTypeId));
                return Json(new { data = notes }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Commodity Master
        /// <summary>
        /// Landing page of Commodity master
        /// </summary>
        /// <returns></returns>
        public ActionResult CommodityMaster()
        {
            return View();
        }

        /// <summary>
        /// Get all CommodityMaster
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadCommodityMaster()
        {

            var commodities = await _masterService.GetAllCommodities();
            return Json(new { data = commodities }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add/Update CommodityMaster
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddCommodityMaster(int id)
        {

            var commodity = await _masterService.GetCommodityById(id);
            if (commodity == null)
                commodity = new Arity.Data.Entity.CommodityMaster() { EFDate = DateTime.Now };

            return PartialView("_CommodityWizard", commodity);
        }

        /// <summary>
        /// Add/Update Commodity
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<ActionResult> AddCommodityMaster(Arity.Data.Entity.CommodityMaster commodity)
        {

            await _masterService.AddUpdateCommodity(commodity);

            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Consultants
        /// <summary>
        /// Landing page of Consultants master
        /// </summary>
        /// <returns></returns>
        public ActionResult Consultants()
        {
            return View();
        }

        /// <summary>
        /// Get all Consultants
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadConsultants()
        {

            var consultants = await _masterService.GetAllConsultants();
            return Json(new { data = consultants }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add/Update CommodityMaster
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddConsultant(int id)
        {

            var consultant = await _masterService.GetConsultantById(id);
            if (consultant == null)
                consultant = new ConsultantDTO();

            return PartialView("_ConsultantWizard", consultant);
        }

        /// <summary>
        /// Add/Update consultant
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddConsultant(ConsultantDTO consultant)
        {

            await _masterService.AddUpdateConsultant(consultant);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Remove consultant
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> DeleteConsultant(int id)
        {
            try
            {

                await _masterService.RemoveConsultant(id);

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {

                throw;
            }
        }

        /// <summary>
        /// Select Commodities for client
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> SelectCommodities()
        {

            var commodities = await _masterService.GetAllCommodities();
            ViewBag.Commodities = new SelectList(commodities, "Id", "Name");
            return PartialView("_SelectConsultantwizard", commodities);
        }

        #endregion

        #region Document Types
        /// <summary>
        /// Landing page of Document Types
        /// </summary>
        /// <returns></returns>
        public ActionResult DocumentType()
        {
            return View();
        }

        /// <summary>
        /// Get all document type
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadDocumentType()
        {
            var documentTypes = await _masterService.GetAllDocumentTypes();
            return Json(new { data = documentTypes }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add/Update document type
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddDocumentType(int id)
        {
            var documentType = await _masterService.GetDocumentTypeById(id);
            if (documentType == null)
                documentType = new DocumentTypes();

            return PartialView("_DocumentTypeWizard", documentType);
        }

        /// <summary>
        /// Add/Update document type
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<ActionResult> AddDocumentType(DocumentTypes documentType)
        {
            await _masterService.AddUpdateDocumentType(documentType);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Delete document type
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<ActionResult> DeleteDocumentType(int documentTypeId)
        {
            try
            {
                await _masterService.DeleteDocumentType(documentTypeId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        #endregion


        #region Business Status
        /// <summary>
        /// Landing page of Business status
        /// </summary>
        /// <returns></returns>
        public ActionResult BusinessStatus()
        {
            return View();
        }

        /// <summary>
        /// Get all business status
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadBusinessStatus()
        {
            var businessStatus = await _masterService.GetAllBusinessStatus();
            return Json(new { data = businessStatus }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add/Update business status
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddBusinessStatus(int id)
        {
            var businessStatus = await _masterService.GetBusinessStatusById(id);
            if (businessStatus == null)
                businessStatus = new BusinessStatus();

            return PartialView("_BusinessStatusWizard", businessStatus);
        }

        /// <summary>
        /// Add/Update business status
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<ActionResult> AddBusinessStatus(BusinessStatus businessStatus)
        {
            await _masterService.AddUpdateBusinessStatus(businessStatus);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Delete business status
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<ActionResult> DeleteBusinessStatus(int businessStatusId)
        {
            try
            {
                await _masterService.DeleteBusinessStatus(businessStatusId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Business Type
        /// <summary>
        /// Landing page of business type
        /// </summary>
        /// <returns></returns>
        public ActionResult BusinessType()
        {
            return View();
        }

        /// <summary>
        /// Get all business type
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LoadBusinessType()
        {
            var businessType = await _masterService.GetAllBusinessType();
            return Json(new { data = businessType }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add/Update business type
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddBusinessType(int id)
        {
            var businessType = await _masterService.GetBusinessTypeById(id);
            if (businessType == null)
                businessType = new BusinessTypes();

            return PartialView("_BusinessTypeWizard", businessType);
        }

        /// <summary>
        /// Add/Update business type
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<ActionResult> AddBusinessTypes(BusinessTypes businessTypes)
        {
            await _masterService.AddUpdateBusinessTypes(businessTypes);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Delete business type
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<ActionResult> DeleteBusinessTypes(int businessTypesId)
        {
            try
            {
                await _masterService.DeleteBusinessTypes(businessTypesId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}