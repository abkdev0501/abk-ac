using Arity.Data.Dto;
using Arity.Service;
using Arity.Service.Contract;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using Arity.Data.Helpers;
using Arity.Data;
using ArityApp.App_Start;

namespace ArityApp.Controllers
{
    [Authorize]
    //[IsAuthorize]
    public class HomeController : Controller
    {
        #region Global Variables
        private IAccountService _accountService;
        private IInvoiceService _invoiceService;
        private IPaymentService _paymentService;
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
                }
                else
                    invoiceEntry.InvoiceId = default(int);
                ViewBag.Company = new SelectList(await _invoiceService.GetCompany(), "Id", "CompanyName", invoiceEntry.CompanyId);
                ViewBag.Client = new SelectList(await _invoiceService.GetClient(Convert.ToInt32(invoiceEntry.CompanyId)), "Id", "FullName", invoiceEntry.ClientId);
                ViewBag.Particular = new SelectList(await _invoiceService.GetParticular(), "Id", "ParticularSF", invoiceEntry.ParticularId);
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
                var tt = ex.InnerException;
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
                var receiptDto = new ReceiptDto();
                if (id != null)
                    receiptDto = await _paymentService.GetReceipt(id ?? 0);
                receiptDto = receiptDto ?? new ReceiptDto();

                ViewBag.Invoice = new MultiSelectList(await _paymentService.GetAllInvoice(), "InvoiceId", "InvoiceNumber", receiptDto.InvoiceIds);
                return PartialView("_ReceiptEntry", receiptDto);
            }
            catch (Exception ex)
            {
                var tt = ex.InnerException;
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
        /// Generate Meeting by State Report Excel
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async Task GenerateInvoice(int id)
        {
            _invoiceService = new InvoiceService();
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Invoice");

            var invoicDetails = await _invoiceService.DownloadInvoice(id);
            var companyDetail = await _invoiceService.GetCompanyDetailById(Convert.ToInt32(invoicDetails.InvoiceEntry.CompanyId));

            Sheet.Cells["A1:H1"].Style.Font.Bold = true;
            Sheet.Cells["A1:H1"].Value = companyDetail.CompanyName;
            Sheet.Cells["A1:H1"].Merge = true;
            Sheet.Cells["A1:H1"].Style.Font.Size = 16;
            Sheet.Cells["A1:H1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A1:H1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            Sheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
            Sheet.Cells["A1:H1"].Style.Font.Color.SetColor(System.Drawing.Color.White);

            Sheet.Cells["A2:H2"].Style.Font.Bold = true;
            Sheet.Cells["A2:H2"].Value = companyDetail.Type;
            Sheet.Cells["A2:H2"].Merge = true;
            Sheet.Cells["A2:H2"].Style.Font.Size = 12;
            Sheet.Cells["A2:H2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A2:H2"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            Sheet.Cells["A2:H2"].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
            Sheet.Cells["A2:H2"].Style.Font.Color.SetColor(System.Drawing.Color.White);

            Sheet.Cells["A3:H3"].Value = companyDetail.Address;
            Sheet.Cells["A3:H3"].Merge = true;
            Sheet.Cells["A3:H3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells["A5"].Value = "Bill No.";
            Sheet.Cells["A5"].Style.Font.Bold = true;
            Sheet.Cells["B5"].Value = invoicDetails.InvoiceEntry.InvoiceNumber;
            Sheet.Cells["B5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;


            Sheet.Cells["D5:E5"].Value = "INVOICE";
            Sheet.Cells["D5:E5"].Style.Font.Bold = true;
            Sheet.Cells["D5:E5"].Style.Font.UnderLine = true;
            Sheet.Cells["D5:E5"].Merge = true;
            Sheet.Cells["D5:E5"].Style.Font.Size = 12;
            Sheet.Cells["D5:E5"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            Sheet.Cells["D5:E5"].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
            Sheet.Cells["D5:E5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["D5:E5"].Style.Font.Color.SetColor(System.Drawing.Color.White);

            Sheet.Cells["G5"].Value = "Date:";
            Sheet.Cells["H5"].Value = invoicDetails.InvoiceEntry.CreatedDate.Value.ToString("MM/dd/yyyy");
            Sheet.Cells["H5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;


            Sheet.Cells["A8"].Value = "Name:";
            Sheet.Cells["A8"].Style.Font.Bold = true;
            Sheet.Cells["A8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            Sheet.Cells["B8:H8"].Value = invoicDetails.InvoiceEntry.FullName;
            Sheet.Cells["B8:H8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["B8:H8"].Merge = true;

            Sheet.Cells["A9"].Value = "Address:";
            Sheet.Cells["A9"].Style.Font.Bold = true;
            Sheet.Cells["A9"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

            Sheet.Cells["B9:H9"].Value = invoicDetails.InvoiceEntry.Address;
            Sheet.Cells["B9:H9"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["B9:H9"].Merge = true;


            Sheet.Cells["A11:F11"].Value = "Particulars";
            Sheet.Cells["A11:F11"].Merge = true;
            Sheet.Cells["A11:F11"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A11:F11"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            Sheet.Cells["A11:F11"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A11:F11"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            Sheet.Cells["G11:H11"].Value = "Amount";
            Sheet.Cells["G11:H11"].Merge = true;
            Sheet.Cells["G11:H11"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["G11:H11"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            Sheet.Cells["G11:H11"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["G11:H11"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

            int currentCell = 12;
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
            for (int i = currentCell; i < currentCell + 5; i++)
            {
                Sheet.Cells[string.Format("A{0}:F{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("A{0}:F{0}", i)].Merge = true;
                Sheet.Cells[string.Format("G{0}:H{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("G{0}:H{0}", i)].Merge = true;
            }

            currentCell = currentCell + 5;
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

            Sheet.Cells[string.Format("A{0}", currentCell)].Value = "For F.Y.";
            Sheet.Cells[string.Format("A{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            Sheet.Cells[string.Format("A{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

            Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Value = invoicDetails.Particulars.FirstOrDefault().Year;
            Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Merge = true;
            Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            Sheet.Cells[string.Format("B{0}:H{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
            currentCell += 5;

            Sheet.Cells[string.Format("F{0}:H{0}", currentCell)].Value = "Authorized Signatory";
            Sheet.Cells[string.Format("F{0}:H{0}", currentCell)].Merge = true;
            Sheet.Cells[string.Format("F{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=" + invoicDetails.InvoiceEntry.InvoiceNumber + "_Invoice.xlsx");
                Ep.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }

        /// <summary>
        /// Generate Meeting by State Report Excel
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async Task GenerateReciept(int id)
        {
            _invoiceService = new InvoiceService();
            _paymentService = new PaymentService();
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Invoice");

            var receiptDetails = await _paymentService.GetReceipt(id);
            var companyDetail = await _invoiceService.GetCompanyDetailById(Convert.ToInt32(receiptDetails.Invoices.FirstOrDefault().CompanyId));

            Sheet.Cells["B1:G1"].Style.Font.Bold = true;
            Sheet.Cells["B1:G1"].Value = companyDetail.CompanyName;
            Sheet.Cells["B1:G1"].Merge = true;
            Sheet.Cells["B1:G1"].Style.Font.Size = 16;
            Sheet.Cells["B1:G1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["B1:G1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            Sheet.Cells["B1:G1"].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
            Sheet.Cells["B1:G1"].Style.Font.Color.SetColor(System.Drawing.Color.White);

            Sheet.Cells["B2:G2"].Style.Font.Bold = true;
            Sheet.Cells["B2:G2"].Value = companyDetail.Type;
            Sheet.Cells["B2:G2"].Merge = true;
            Sheet.Cells["B2:G2"].Style.Font.Size = 12;
            Sheet.Cells["B2:G2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["B2:G2"].Style.Font.Color.SetColor(System.Drawing.Color.White);

            Sheet.Cells["B3:G3"].Value = companyDetail.Address;
            Sheet.Cells["B3:G3"].Merge = true;
            Sheet.Cells["B3:G3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Sheet.Cells["A5"].Value = "Rpt No.";
            Sheet.Cells["A5"].Style.Font.Bold = true;
            Sheet.Cells["B5"].Value = receiptDetails.RecieptNo;
            Sheet.Cells["B5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;


            Sheet.Cells["D5:E5"].Value = "RECEIPT";
            Sheet.Cells["D5:E5"].Style.Font.Bold = true;
            Sheet.Cells["D5:E5"].Merge = true;
            Sheet.Cells["D5:E5"].Style.Font.Size = 12;
            Sheet.Cells["D5:E5"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            Sheet.Cells["D5:E5"].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
            Sheet.Cells["D5:E5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["D5:E5"].Style.Font.Color.SetColor(System.Drawing.Color.White);

            Sheet.Cells["G5"].Value = "Date:";
            Sheet.Cells["G5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells["H5"].Value = receiptDetails.CreatedDate.ToString("MM/dd/yyyy");
            Sheet.Cells["H5"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

            Sheet.Cells["A7:F7"].Value = "Particulars";
            Sheet.Cells["A7:F7"].Merge = true;
            Sheet.Cells["A7:F7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["A7:F7"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A7:F7"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["A7:F7"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            Sheet.Cells["G7:H7"].Value = "Amount";
            Sheet.Cells["G7:H7"].Merge = true;
            Sheet.Cells["G7:H7"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            Sheet.Cells["G7:H7"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["G7:H7"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            Sheet.Cells["G7:H7"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

            int currentCell = 8;

            Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Value = receiptDetails.Invoices.FirstOrDefault().FullName;
            Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Merge = true;
            Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            Sheet.Cells[string.Format("A{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

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

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=" + receiptDetails.RecieptNo + "_Reciept.xlsx");
                Ep.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }
        #endregion

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
                _accountService = new AccountService();
                var userDto = new UsersDto();
                if (id != null)
                    userDto = await _accountService.GetUser(id ?? 0);
                userDto = userDto ?? new UsersDto();

                ViewBag.UserType = new SelectList(await _accountService.GetAllUserType(), "Id", "UserTypeName", userDto.UserTypeId);
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
        public async Task<ActionResult> AddUpdateUser(User user)
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
        #endregion

    }
}