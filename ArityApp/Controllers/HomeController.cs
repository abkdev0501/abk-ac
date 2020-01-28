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
using System.Xml.Linq;

namespace ArityApp.Controllers
{
    //[Authorize]
    // [IsAuthorize]
    public class HomeController : Controller
    {
        #region Global Variables
        private IVendorServices _vendorServices;
        private IInvoiceService _invoiceService;
        private IPaymentService _paymentService;
        #endregion

        public ActionResult Index()
        {
            ViewBag.FromDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01));
            ViewBag.ToDate = Convert.ToDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
            return View();
        }

        #region Invoice
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
                await _invoiceService.AddUpdateInvoiceEntry(Convert.ToInt32(Session["UserID"]), invoiceEntry);
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
        /// Download Invoice
        /// </summary>
        /// <param name="id">Invoiceid</param>
        /// <returns></returns>
        public async Task<ActionResult> DownloadInvoice(int id)
        {
            try
            {
                _invoiceService = new InvoiceService();
                var downloadModel = await _invoiceService.DownloadInvoice(id);

                string filePath = "~/Views/Partial/_Invoice.cshtml";
                var hTMLViewStr = RenderViewToString(ControllerContext, filePath, downloadModel);

                using (MemoryStream stream = new System.IO.MemoryStream())
                {
                    StringReader sr = new StringReader(hTMLViewStr);
                    Document pdfDoc = new Document(PageSize.A4);

                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                    pdfDoc.Close();
                    return File(stream.ToArray(), "application/pdf", downloadModel.InvoiceEntry.InvoiceNumber + ".pdf");
                }
                // return File(document.ByteArray, document.ContentType, document.DocumentName + "." + document.ContentType);
            }
            catch (Exception)
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
            _invoiceService = new InvoiceService(); 
            return Json(await _invoiceService.GetInvoiceAmountTotal(invoices),JsonRequestBehavior.AllowGet);
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

                ViewBag.Invoice = new MultiSelectList(await _invoiceService.GetAllInvoice(), "InvoiceId", "InvoiceNumber", receiptDto.InvoiceIds);
                return PartialView("_ReceiptEntry", receiptDto);
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
        /// <summary>
        /// 404 page 
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> NotFound()
        {
            return View();
        }

    }
}