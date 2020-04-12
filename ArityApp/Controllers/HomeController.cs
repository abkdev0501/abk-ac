using Arity.Data.Dto;
using Arity.Service;
using Arity.Service.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using OfficeOpenXml;
using Arity.Data.Helpers;
using Arity.Data.Entity;
using System.Text;
using System.Drawing;

namespace ArityApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        #region Global Variables
        private IAccountService _accountService;
        private IInvoiceService _invoiceService;
        private IPaymentService _paymentService;
        private ITaskService _taskService;
        private IDocumentService _documentService;
        #endregion

        #region Invoice
        public ActionResult Index()
        {
            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View();
        }

        /// <summary>
        /// Load list of invoice by from and to date
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> LoadInvoiceHistory(DateTime from, DateTime to)
        {
            try
            {
                _invoiceService = new InvoiceService();
                to = to + new TimeSpan(23, 59, 59);
                from = from + new TimeSpan(00, 00, 1);
                var invoiceList = await _invoiceService.GetAllInvoice(from, to);
                return Json(new { data = invoiceList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<InvoiceEntry>() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get all particulars history by invoice id
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetInvoiceParticulars(int invoiceId)
        {
            try
            {
                _invoiceService = new InvoiceService();
                return Json(await _invoiceService.GetAllInvoiceParticulars(invoiceId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<InvoiceEntry>() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Add invoice entry
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddInvoiceEntry(int? id, int? invoiceId)
        {
            try
            {
                _invoiceService = new InvoiceService();
                var invoiceEntry = new InvoiceEntry();
                if (id != null)
                    invoiceEntry = await _invoiceService.GetInvoice(id ?? 0);
                invoiceEntry = invoiceEntry ?? new InvoiceEntry();

                if (invoiceId != null)
                {
                    var invoiceDetails = await _invoiceService.GetInvoiceSingle(invoiceId ?? 0);
                    invoiceEntry.InvoiceId = invoiceId ?? 0;
                    invoiceEntry.ClientId = invoiceDetails != null ? invoiceDetails.ClientId : 0;
                    invoiceEntry.CompanyId = invoiceDetails != null ? invoiceDetails.CompanyId : 0;
                    invoiceEntry.InvoiceDate = invoiceDetails != null ? invoiceDetails.InvoiceDate : invoiceEntry.InvoiceDate;
                }
                else
                    invoiceEntry.InvoiceId = default(int);
                ViewBag.Company = new SelectList(await _invoiceService.GetCompany(), "Id", "CompanyName", invoiceEntry.CompanyId);
                ViewBag.Client = new SelectList(await _invoiceService.GetClient(Convert.ToInt32(invoiceEntry.CompanyId)), "Id", "FullName", invoiceEntry.ClientId);
                ViewBag.Particular = new SelectList(await _invoiceService.GetParticular(), "Id", "ParticularFF", invoiceEntry.ParticularId);
                return PartialView("_InvoiceEntry", invoiceEntry);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Add invoice entry
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> AddInvoiceEntry(InvoiceEntry invoiceEntry)
        {
            try
            {
                _invoiceService = new InvoiceService();
                await _invoiceService.AddUpdateInvoiceEntry(Convert.ToInt32(SessionHelper.UserId), invoiceEntry);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Remove invoice entry
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> RemoveInvoiceEntry(int id)
        {
            try
            {
                _invoiceService = new InvoiceService();
                await _invoiceService.DeleteInvoiceParticularEntry(id);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get client by company
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetClientByCompany(int companyId)
        {
            try
            {
                _invoiceService = new InvoiceService();
                return Json(await _invoiceService.GetClient(companyId), JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get tracking information by invoice id
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetInvoiceTrackingInformation(int invoiceId)
        {
            try
            {
                _invoiceService = new InvoiceService();
                return Json(await _invoiceService.GetTrackingInformation(invoiceId), JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Add invoice tracking information 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> AddInvoiceTrackingInformation(int invoiceTrackingId, int invoiceId)
        {
            try
            {
                _invoiceService = new InvoiceService();
                var trackingDetails = await _invoiceService.GetTrackingInformationById(invoiceTrackingId);
                if (trackingDetails == null)
                    trackingDetails = new TrackingInformation { InvoiceId = invoiceId };
                return PartialView("_InvoiceTracking", trackingDetails);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Add invoice tracking information 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> AddInvoiceTrackingInformation(TrackingInformation trackingInformation)
        {
            try
            {
                _invoiceService = new InvoiceService();
                return Json(await _invoiceService.AddTrackingInformation(trackingInformation), JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Remove invoice tracking information 
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> RemoveInvoiceTracking(int trackingId)
        {
            try
            {
                _invoiceService = new InvoiceService();
                return Json(await _invoiceService.RemoveInvoiceTracking(trackingId), JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }
        public async Task<ActionResult> DeleteInvoice(int invoiceId)
        {
            try
            {
                _invoiceService = new InvoiceService();
             await _invoiceService.DeleteInvoiceById(invoiceId);
                return Json(true,JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw;
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Payment
        /// <summary>
        /// Landing page for payment history
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Payment()
        {
            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View();
        }

        /// <summary>
        /// Load payment list by from and to date
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> LoadPaymentHistory(DateTime from, DateTime to)
        {
            try
            {
                _paymentService = new PaymentService();
                to = to + new TimeSpan(23, 59, 59);
                from = from + new TimeSpan(00, 00, 1);
                var receiptList = await _paymentService.GetAllReceipts(from, to);
                return Json(new { data = receiptList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<ReceiptDto>() }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Add receipt entry
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddPaymentEntry(int? id)
        {
            try
            {
                _paymentService = new PaymentService();
                _invoiceService = new InvoiceService();
                var receiptDto = new ReceiptDto();
                if (id != null)
                    receiptDto = await _paymentService.GetReceipt(id ?? 0);
                receiptDto = receiptDto ?? new ReceiptDto();

                ViewBag.Company = new SelectList(await _invoiceService.GetCompany(), "Id", "CompanyName", receiptDto.CompanyId);
                ViewBag.Client = new SelectList(await _invoiceService.GetClient(Convert.ToInt32(receiptDto.CompanyId)), "Id", "FullName", receiptDto.ClientId);

                ViewBag.Invoice = new MultiSelectList(await _invoiceService.GetInvoiceByClientandCompany(Convert.ToInt32(receiptDto.CompanyId), Convert.ToInt32(receiptDto.ClientId)), "InvoiceId", "InvoiceNumber", receiptDto.InvoiceIds);
                return PartialView("_ReceiptEntry", receiptDto);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<JsonResult> GetInvoiceByClientAndCompany(int companyId, int clientId)
        {
            try
            {
                _invoiceService = new InvoiceService();
                return Json(await _invoiceService.GetInvoiceByClientandCompany(companyId, clientId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<JsonResult> GetTotalOfInvoice(List<long> invoices)
        {
            try
            {
                _invoiceService = new InvoiceService();
                return Json(await _invoiceService.GetInvoiceAmountTotal(invoices), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Add receipt entry
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> AddPaymentEntry(ReceiptDto receiptEntry)
        {
            try
            {
                _paymentService = new PaymentService();
                await _paymentService.AddUpdateReceiptEntry(receiptEntry);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Render ViewToString
        /// </summary>
        /// <param name="context"></param>
        /// <param name="viewPath"></param>
        ///<param name="model"></param>
        /// <returns></returns>
        public static String RenderViewToString(ControllerContext context, String viewPath, object model = null)
        {
            context.Controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindView(context, viewPath, null);
                var viewContext = new ViewContext(context, viewResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(context, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
        #endregion

        #region Download Invoice & Payment receipt
        /// <summary>
        /// Generate invoice entry
        /// </summary>
        /// <returns></returns>
        public async Task GenerateInvoice(string id)
        {
            _invoiceService = new InvoiceService();
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Invoice");
            int currentCell = 1;

            var invoiceIds = id.TrimEnd(',').Split(',').ToList<string>();
            foreach (var invoiceId in invoiceIds)
            {
                var invoicDetails = await _invoiceService.DownloadInvoice(Convert.ToInt32(invoiceId));
                var companyDetail = await _invoiceService.GetCompanyDetailById(Convert.ToInt32(invoicDetails.InvoiceEntry.CompanyId));

                #region Excel set
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Value = companyDetail.CompanyName;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Font.Size = 16;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);
                currentCell++;

                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Value = companyDetail.Type;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Font.Size = 12;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);
                currentCell++;

                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Value = companyDetail.Address;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("A{0}", currentCell)].Value = "Bill No.";
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("B{0}", currentCell)].Value = invoicDetails.InvoiceEntry.InvoiceNumber;
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;


                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Value = "INVOICE";
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Font.UnderLine = true;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Font.Size = 12;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);

                Sheet.Cells[string.Format("G{0}", currentCell)].Value = "Date:";
                Sheet.Cells[string.Format("H{0}", currentCell)].Value = invoicDetails.InvoiceEntry.InvoiceDate.ToString("MM/dd/yyyy");
                Sheet.Cells[string.Format("H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                currentCell++;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("A{0}", currentCell)].Value = "Name:";
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Value = invoicDetails.InvoiceEntry.FullName;
                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("A{0}", currentCell)].Value = "Address:";
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Value = invoicDetails.InvoiceEntry.Address;
                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Merge = true;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Value = "Particulars";
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Value = "Amount";
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                currentCell++;

                for (int i = currentCell; i < (invoicDetails.Particulars.Count() + currentCell); i++)
                {
                    Sheet.Cells[string.Format("A{0}:F{0}", i)].Value = string.Format("{0} Fees For F.Y.{1}", invoicDetails.Particulars[i - currentCell].FFParticulars, invoicDetails.Particulars[i - currentCell].Year);
                    Sheet.Cells[string.Format("A{0}:F{0}", i)].Merge = true;
                    Sheet.Cells[string.Format("A{0}:F{0}", i)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    Sheet.Cells[string.Format("A{0}:F{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    Sheet.Cells[string.Format("G{0}:H{0}", i)].Value = invoicDetails.Particulars[i - currentCell].Amount;
                    Sheet.Cells[string.Format("G{0}:H{0}", i)].Merge = true;
                    Sheet.Cells[string.Format("G{0}:H{0}", i)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    Sheet.Cells[string.Format("G{0}:H{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                }

                currentCell = (invoicDetails.Particulars.Count() + currentCell);
                for (int i = currentCell; i < currentCell + (invoicDetails.Particulars.Count() <= 5 ? 3 : 1); i++)
                {
                    Sheet.Cells[string.Format("A{0}:F{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    Sheet.Cells[string.Format("A{0}:F{0}", i)].Merge = true;
                    Sheet.Cells[string.Format("G{0}:H{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                    Sheet.Cells[string.Format("G{0}:H{0}", i)].Merge = true;
                }

                currentCell = currentCell + (invoicDetails.Particulars.Count() <= 5 ? 3 : 1);
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:E{0}", currentCell)].Value = "Amount Chargeable (in words)";
                Sheet.Cells[string.Format("A{0}:E{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:E{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                Sheet.Cells[string.Format("F{0}", currentCell)].Value = "Total";
                Sheet.Cells[string.Format("F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Value = invoicDetails.Particulars.Sum(_ => _.Amount);
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Value = "Rupees " + CurrencyConvertor.NumberToWords(Convert.ToInt32(invoicDetails.Particulars.Sum(_ => _.Amount))) + " Only";
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Value = "";
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                currentCell++;

                Sheet.Cells[string.Format("A{0}", currentCell)].Value = "For F.Y.";
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Value = invoicDetails.Particulars.FirstOrDefault().Year;
                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                currentCell += 5;

                Sheet.Cells[string.Format("F{0}:H{0}", currentCell)].Value = "Authorized Signatory";
                Sheet.Cells[string.Format("F{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("F{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                #endregion

                currentCell += 10;

            }

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=Invoice.xlsx");
                Ep.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

        /// <summary>
        /// Generate payment receipt
        /// </summary>
        /// <returns></returns>
        public async Task GenerateReciept(string id)
        {
            _invoiceService = new InvoiceService();
            _paymentService = new PaymentService();
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Invoice");
            int currentCell = 1;


            var receiptIds = id.TrimEnd(',').Split(',').ToList<string>();
            foreach (var receiptId in receiptIds)
            {
                var receiptDetails = await _paymentService.GetReceipt(Convert.ToInt32(receiptId));
                var companyDetail = await _invoiceService.GetCompanyDetailById(Convert.ToInt32(receiptDetails.Invoices.FirstOrDefault().CompanyId));

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Value = companyDetail.CompanyName;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Font.Size = 16;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Value = companyDetail.Type;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Font.Size = 12;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Value = companyDetail.Address;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("A{0}", currentCell)].Value = "Rpt No.";
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("B{0}", currentCell)].Value = receiptDetails.RecieptNo;
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;


                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Value = "RECEIPT";
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Font.Size = 12;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("D{0}:E{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);

                Sheet.Cells[string.Format("G{0}", currentCell)].Value = "Date:";
                Sheet.Cells[string.Format("G{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("H{0}", currentCell)].Value = receiptDetails.RecieptDate.ToString("MM/dd/yyyy");
                Sheet.Cells[string.Format("H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Value = "Particulars";
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Value = "Amount";
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                currentCell++;

                Sheet.Cells[string.Format(string.Format("A{0}:F{0}", currentCell), currentCell)].Value = receiptDetails.Invoices.FirstOrDefault().FullName;
                Sheet.Cells[string.Format(string.Format("A{0}:F{0}", currentCell), currentCell)].Merge = true;
                Sheet.Cells[string.Format(string.Format("A{0}:F{0}", currentCell), currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format(string.Format("A{0}:F{0}", currentCell), currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Value = receiptDetails.TotalAmount;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                currentCell++;

                Sheet.Cells[string.Format("A{0}", currentCell)].Value = "Against Ref.";
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Value = string.Join("&", receiptDetails.Invoices.Select(_ => _.InvoiceNumber));
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;

                currentCell++;

                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Value = receiptDetails.BankName + " " + receiptDetails.ChequeNumber;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Value = "Discount";
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                if (receiptDetails.Discount > 0)
                    Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Value = receiptDetails.Discount;
                else
                    Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Value = "";
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                #region Line Break
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;
                #endregion

                Sheet.Cells[string.Format("A{0}", currentCell)].Value = "Against F.Y.";
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("A{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Value = receiptDetails.Year;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:E{0}", currentCell)].Value = "Amount Received (in words)";
                Sheet.Cells[string.Format("A{0}:E{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("A{0}:E{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("A{0}:E{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:E{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:E{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("F{0}", currentCell)].Value = "Total";
                Sheet.Cells[string.Format("F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("F{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("F{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Value = receiptDetails.TotalAmount - receiptDetails.Discount;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}:H{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                currentCell++;

                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Value = "Rupees " + CurrencyConvertor.NumberToWords(Convert.ToInt32(receiptDetails.TotalAmount - receiptDetails.Discount)) + " Only";
                Sheet.Cells[string.Format("A{0}:H{0}", currentCell)].Merge = true;

                currentCell += 5;

                Sheet.Cells[string.Format("F{0}:H{0}", currentCell)].Value = "Authorized Signatory";
                Sheet.Cells[string.Format("F{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("F{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                currentCell += 10;
            }

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=Reciept.xlsx");
                Ep.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

        public async Task<ActionResult> GeneratePDF(string id)
        {
            _invoiceService = new InvoiceService();
            StringBuilder invoiceTemplate = new StringBuilder();
            var invoiceIds = id.TrimEnd(',').Split(',').ToList<string>();
            foreach (var invoiceId in invoiceIds)
            {
                var invoicDetails = await _invoiceService.DownloadInvoice(Convert.ToInt32(invoiceId));
                var companyDetail = await _invoiceService.GetCompanyDetailById(Convert.ToInt32(invoicDetails.InvoiceEntry.CompanyId));

                var html = GetInvoiceHTML();
                invoiceTemplate.Append(html.Replace("##COMPANY##", companyDetail.CompanyName)
                    .Replace("##TYPE##", companyDetail.Type)
                    .Replace("##ADDRESS##", companyDetail.Address)
                    .Replace("##COLOR##", GetColorString(companyDetail.PreferedColor ?? 0))
                    .Replace("##BILLNO##", invoicDetails.InvoiceEntry.InvoiceNumber)
                    .Replace("##DATE##", invoicDetails.InvoiceEntry.InvoiceDate.ToString("MM/dd/yyyy"))
                    .Replace("##NAME##", invoicDetails.InvoiceEntry.FullName)
                    .Replace("##CLIENTADDRESS##", invoicDetails.InvoiceEntry.Address)
                    .Replace("##PARTICULARS##", BindParticulars(invoicDetails.Particulars))
                    .Replace("##TOTAL##", invoicDetails.Particulars.Sum(_ => _.Amount).ToString())
                    .Replace("##INWORD##", "Rupees " + CurrencyConvertor.NumberToWords(Convert.ToInt32(invoicDetails.Particulars.Sum(_ => _.Amount))) + " Only")
                    .Replace("##YEAR##", invoicDetails.Particulars.FirstOrDefault().Year)
                    );
            }

            var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();

            var fileData = htmlToPdf.GeneratePdf(invoiceTemplate.ToString());
            return File(fileData, "application/octet-stream", "Invoide.pdf");
        }


        public async Task<ActionResult> GenerateReceiptPDF(string id)
        {
            _invoiceService = new InvoiceService();
            _paymentService = new PaymentService();
            StringBuilder invoiceTemplate = new StringBuilder();
            var receiptIds = id.TrimEnd(',').Split(',').ToList<string>();
            foreach (var receiptId in receiptIds)
            {
                var receiptDetails = await _paymentService.GetReceipt(Convert.ToInt32(receiptId));
                var companyDetail = await _invoiceService.GetCompanyDetailById(Convert.ToInt32(receiptDetails.Invoices.FirstOrDefault().CompanyId));


                var html = GetRecieptHTML();
                invoiceTemplate.Append(html.Replace("##COMPANY##", companyDetail.CompanyName)
                    .Replace("##TYPE##", companyDetail.Type)
                    .Replace("##ADDRESS##", companyDetail.Address)
                    .Replace("##COLOR##", GetColorString(companyDetail.PreferedColor ?? 0))
                    .Replace("##BILLNO##", receiptDetails.RecieptNo)
                    .Replace("##DATE##", receiptDetails.RecieptDate.ToString("MM/dd/yyyy"))
                    .Replace("##NAME##", receiptDetails.Invoices.FirstOrDefault().FullName)
                    .Replace("##RECEIPTNO##", string.Join(", ", receiptDetails.Invoices.Select(_ => _.InvoiceNumber)))
                    .Replace("##BANK##", receiptDetails.BankName + " " + receiptDetails.ChequeNumber)
                    .Replace("##TOTAL##", receiptDetails.TotalAmount.ToString())
                    .Replace("##FULLTOTAL##", (receiptDetails.TotalAmount - receiptDetails.Discount).ToString())
                    .Replace("##DISCOUNT##", (receiptDetails.Discount).ToString())
                    .Replace("##INWORD##", "Rupees " + CurrencyConvertor.NumberToWords(Convert.ToInt32(receiptDetails.TotalAmount - receiptDetails.Discount)) + " Only")
                    .Replace("##YEAR##", receiptDetails.Year)
                    );
            }

            var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();

            var fileData = htmlToPdf.GeneratePdf(invoiceTemplate.ToString());
            return File(fileData, "application/octet-stream", "Invoide.pdf");
        }

        #endregion

        #region Generate sample Invoice

        public async Task SampleInvoiceCreateFile()
        {
            _invoiceService = new InvoiceService();
            _invoiceService.GetAllCompanyWithClients();

            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Create Invoice");
            int currentCell = 1;

            //Instruction in excel file
            Sheet.Cells["A1"].Value = "**** Do not change anything in file Excepting filling data****";
            Sheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
            currentCell++;

            Sheet.Cells["A6"].Value = "**** Do not change anything in file Excepting filling data****";
            Sheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
            currentCell++;

            Sheet.Cells["B6"].Value = "**** Do not change anything in file Excepting filling data****";
            Sheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
            currentCell++;

            Sheet.Cells["C6"].Value = "**** Do not change anything in file Excepting filling data****";
            Sheet.Cells["A1"].Style.Font.Color.SetColor(Color.Red);
            currentCell++;


            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename= SampleCreateInvoice.xlsx");
                Ep.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
            
        }

        #endregion

        #region User
        public async Task<ActionResult> Users()
        {

            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View();
        }

        public async Task<JsonResult> LoadUsers(DateTime from, DateTime to)
        {
            try
            {
                _accountService = new AccountService();
                to = to + new TimeSpan(23, 59, 59);
                from = from + new TimeSpan(00, 00, 1);
                var users = await _accountService.GetAllUsers(from, to);
                return Json(new { data = users }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var tt = ex.InnerException;
                return Json(new { data = new List<UsersDto>() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Add User entry
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AddUpdateUser(int? id)
        {
            try
            {
                _invoiceService = new InvoiceService();
                _accountService = new AccountService();
                var userDto = new UsersDto();
                if (id != null)
                    userDto = await _accountService.GetUser(id ?? 0);
                userDto = userDto ?? new UsersDto();
                var userTypes = await _accountService.GetAllUserType();
                if (SessionHelper.UserTypeId != (int)Arity.Service.Core.UserType.MasterAdmin)
                {
                    userTypes.RemoveAt(0);
                    userTypes.RemoveAt(1);
                }
                else
                    userTypes.RemoveAt(2);

                ViewBag.UserType = new SelectList(userTypes, "Id", "UserTypeName", userDto.UserTypeId);
               // ViewBag.Companies = new MultiSelectList(await _invoiceService.GetCompany(), "Id", "CompanyName", userDto.CompanyIds);
                return PartialView("_UserWizard", userDto);
            }
            catch (Exception ex)
            {
                var tt = ex.InnerException;
                throw;
            }
        }

        /// <summary>
        /// Add User entry
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddUpdateUser(UsersDto user)
        {
            try
            {
                _accountService = new AccountService();
                await _accountService.AddUpadate(user);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var tt = ex.InnerException;
                throw;
            }
        }
        #endregion
         
        #region Dashboard
        /// <summary>
        /// Dashboard landing page
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Dashboard()
        {
            return View();
        }

        /// <summary>
        /// Get current user task
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetAllTask()
        {
            _taskService = new TaskService();
            var tasks = _taskService.GetAll(Convert.ToInt32(SessionHelper.UserId), Convert.ToInt32(SessionHelper.UserTypeId));
            return Json(tasks, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Fetch documents from db
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult>GetDocumentList()
        {
            _documentService = new DocumentService();
            var DocumentList = new List<DocumentMasterDto>();

            if (SessionHelper.UserTypeId == (int)EnumHelper.UserType.Master)
            {
                DocumentList = await _documentService.GetAllDocuments();
            }
            if (SessionHelper.UserTypeId == (int)EnumHelper.UserType.User)
            {
                DocumentList = await _documentService.GetDocumentByUserID((int)SessionHelper.UserId);
            }
            return Json(DocumentList, JsonRequestBehavior.AllowGet);
        }

        public FileResult DownloadDocument(int documentID)
        {
            try
            {
                string folderPath = Server.MapPath("~/Content/Documents/1002_mathOperation.PNG");
                byte[] fileBytes = System.IO.File.ReadAllBytes(@folderPath);
                string fileName = "1002_mathOperation.PNG";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion

        /// <summary>
        /// 404 page 
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> NotFound()
        {
            return View();
        }

        #region Private Method
        private System.Drawing.Color GetColor(int colorType)
        {
            if (colorType == (int)EnumHelper.CompanyColor.Aqua)
                return System.Drawing.Color.MediumAquamarine;

            if (colorType == (int)EnumHelper.CompanyColor.DarkOliveGreen)
                return System.Drawing.Color.DarkOliveGreen;

            return System.Drawing.Color.Green;
        }

        private string GetColorString(int colorType)
        {
            if (colorType == (int)EnumHelper.CompanyColor.Aqua)
                return "MediumAquamarine";

            if (colorType == (int)EnumHelper.CompanyColor.DarkOliveGreen)
                return "DarkOliveGreen";

            return "Green";
        }

        private string BindParticulars(List<InvoiceEntry> invoiceEntries)
        {
            StringBuilder sb = new StringBuilder();
            var template = @"<tr>
                     <td colspan = '6' align='left'>##PARTICULAR##</td>
                     <td align = 'right' class='border-left-light'>##AMOUNT##</td>
                     </tr>";

            foreach (var item in invoiceEntries)
            {
                sb.Append(
                    template.Replace("##PARTICULAR##", string.Format("{0} Fees For F.Y.{1}", item.FFParticulars, item.Year))
                    .Replace("##AMOUNT##", item.Amount.ToString())
                    );
            }

            for (int i = 1; i <= (invoiceEntries.Count() <= 5 ? 2 : 4); i++)
                sb.Append(
                template.Replace("##PARTICULAR##", "&nbsp;")
                .Replace("##AMOUNT##", "&nbsp;")
                );

            return sb.ToString();
        }

        private string GetInvoiceHTML()
        {
            return @"
    <head>
            <style>
                body {
                font - family: 'Segoe UI', Tahoma, Arial, Helvetica, sans - serif;
                font - size: .813em;
                color: #222;
    background - color: #fff;
	margin: 3 % 15 % 3 % 15 %;
            }

            h1, h2, h3, h4, h5 {
                /*font-family: 'Segoe UI',Tahoma,Arial,Helvetica,sans-serif;*/
                font - weight: 100;
            }

            h1 {
                color: #fff;
    margin: 15px 0 15px 0;
            }

            h2 {
                margin: 10px 5px 0 0;
            }

            h3 {
                color: #fff;
    margin: 5px 5px 0 0;
            }
.color{
    
	color:#fff;
    text-align:center;
}
.address{
 text-align:center;
}
.signature
{
    padding-top:10%;
    text-align:right;
}
.table table
{
    width:100%
}
.padding-bottom-20{
padding-bottom:20px;
}
.width-15
{
width:15%;
}
.border-top
{
border-top:2px solid;
}
.border-bottom
{
border-bottom:2px solid;
}
.border{
border:2px solid;
}
.border-left{
border-left:2px solid;
}
.border-top-light{
border-top:1px solid;
}

.border-left-light{
border-left:1px solid;
}
.invoice{
    font-weight: 600;
    text-decoration: underline;
}
        </style>
    </head>
    <body>
        <div class='color' style='background-color:##COLOR##;'>
            <h1>##COMPANY##</h1>
             <h3>##TYPE##</h3>
        </div>
        <div class='address'>
             <h4>##ADDRESS##</h4>
        </div>
         <div class='table padding-bottom-20'>
             <table>
                 <tr>
                     <td>Bill No.</td>
                     <td>##BILLNO##</td>
                     
                      <td>&nbsp;</td>
                     <td class='color' style='background-color:##COLOR##;'><span class='invoice'>INVOICE</span></td>
                     <td>&nbsp;</td>
                     
                      <td align='right'>Date:</td>
                     <td align='right'>##DATE##</td>
                 </tr>
             </table>
        </div>
        
         <div class='table padding-bottom-20'>
             <table>
                 <tr>
                     <td class='width-15'>Name:</td>
                     <td>##NAME##</td>
                     </tr>
                     <tr>
                     <td class='width-15'>Address:</td>
                     <td>##CLIENTADDRESS##</td>
                     </tr>
             </table>
        </div>
        
        <div class='table border'>
             <table>
                 <tr>
                     <td colspan = '6' align='center' class='border-bottom'>Particulars</td>
                     <td align = 'center' class='border-left border-bottom'>Amount</td>
                     </tr>
                    ##PARTICULARS##
                    
                 <tr>
                     <td colspan = '5' class='border-top-light'>Amount Chargeable(in words)</td>
                     <td align = 'right' class='border-top-light'>Total</td>
                     <td align = 'right' class='border-top-light border-left-light'>##TOTAL##</td>
                     </tr>
                   <tr>
                     <td colspan = '6' >##INWORD##</td>
					 <td class='border-left-light'>&nbsp;</td>
                     </tr>
                      <tr>
                     <td colspan = '7' class='border-top'>&nbsp;</td>
                     </tr>
                      <tr>
                     <td class='width-15'>For F.Y.</td>
                     <td align='left' >##YEAR##</td>
                     </tr >
             </table >
        </div >


        <div class='signature'>
            Authorised Signatory
            </div>
        
    </body>
</html>";
        }

        private string GetRecieptHTML()
        {
            return @"
    <head>
            <style>
                body {
                font - family: 'Segoe UI', Tahoma, Arial, Helvetica, sans - serif;
                font - size: .813em;
                color: #222;
    background - color: #fff;
	margin: 3 % 15 % 3 % 15 %;
            }

            h1, h2, h3, h4, h5 {
                /*font-family: 'Segoe UI',Tahoma,Arial,Helvetica,sans-serif;*/
                font - weight: 100;
            }

            h1 {
                color: #fff;
    margin: 15px 0 15px 0;
            }

            h2 {
                margin: 10px 5px 0 0;
            }

            h3 {
                color: #fff;
    margin: 5px 5px 0 0;
            }
.color{
    
	color:#fff;
    text-align:center;
}
.address{
 text-align:center;
}
.signature
{
    padding-top:10%;
    text-align:right;
}
.table table
{
    width:100%
}
.padding-bottom-20{
padding-bottom:20px;
}
.width-15
{
width:15%;
}
.border-top
{
border-top:2px solid;
}
.border-bottom
{
border-bottom:2px solid;
}
.border{
border:2px solid;
}
.border-left{
border-left:2px solid;
}
.border-top-light{
border-top:1px solid;
}

.border-left-light{
border-left:1px solid;
}
.invoice{
    font-weight: 600;
    text-decoration: underline;
}
        </style>
    </head>
    <body>
        <div class='color' >
            <div style='padding-left:22%;'>
            <h1 style='background-color:##COLOR##;width:70%;'>##COMPANY##</h1>
            </div>
        </div>
         <div style='color:black;text-align:center;'>
             <h3 style='color:#000'>##TYPE##</h3>
        </div>
       
        <div class='address'>
             <h4>##ADDRESS##</h4>
        </div>
         <div class='table padding-bottom-20'>
             <table>
                 <tr>
                     <td>Rpt No.</td>
                     <td>##BILLNO##</td>
                     
                      <td>&nbsp;</td>
                     <td class='color' style='background-color:##COLOR##;'><span class='invoice'>RECEIPT</span></td>
                     <td>&nbsp;</td>
                     
                      <td align='right'>Date:</td>
                     <td align='right'>##DATE##</td>
                 </tr>
             </table>
        </div>
        
         
        <div class='table border'>
             <table>
                 <tr>
                     <td colspan = '6' align='center' class='border-bottom'>Particulars</td>
                     <td align='center' class='border-left border-bottom'>Amount</td>
                     </tr>
                      <tr>
                        <td colspan='6'>&nbsp;</td>
                        <td class='border-left'>&nbsp;</td>
                    </tr>
                    <tr>
                          <td colspan='6'>##NAME##</td>
                        <td align='right' class='border-left'>##TOTAL##</td>
                    </tr>
                    
                     <tr>
                          <td colspan='2' style='font-weight:600'>Against Ref.</td>
                          <td align = 'left' colspan='4'>##RECEIPTNO##</td>
                        <td align='right' class='border-left'></td>
                    </tr>
                    
                      <tr>
                          <td colspan='6' >##BANK##</td>
                         <td align='right' class='border-left'></td>
                    </tr>
                     <tr>
                          <td colspan='6' ></td>
                         <td align = 'right' class='border-left'></td>
                    </tr>
                     <tr>
                          <td colspan='6' style='font-weight:600'>Discount</td>
                         <td align='right' class='border-left'>##DISCOUNT##</td>
                    </tr>
                     <tr>
                          <td colspan='6' ></td>
                         <td align = 'right' class='border-left'></td>
                    </tr>
                     <tr>
                          <td colspan='6' ></td>
                         <td align = 'right' class='border-left'></td>
                    </tr>
                    <tr>
                          <td colspan='2' style='font-weight:600'>Against F.Y.</td>
                          <td align = 'left' colspan='4'>##YEAR##</td>
                        <td align = 'right' class='border-left'></td>
                    </tr>
                      <tr>
                          <td colspan='6' ></td>
                         <td align = 'right' class='border-left'></td>
                    </tr>
                 <tr>
                     <td colspan = '5' class='border-top-light'>Amount Received(in words)</td>
                     <td align = 'right' class='border-top-light'>Total</td>
                     <td align = 'right' class='border-top-light border-left-light'>##FULLTOTAL##</td>
                 </tr>
                 <tr>
                     <td align='left' colspan = '7' class='border-top'>##INWORD##</td>
                     </tr >
             </table >
        </div >


        <div class='signature'>
            Authorised Signatory
            </div>
        
    </body>
</html>";
        }
        #endregion

    }
}