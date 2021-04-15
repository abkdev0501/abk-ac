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
using System.Web;
using ExcelDataReader;
using System.Data;

namespace ArityApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        #region Global Variables
        private readonly IAccountService _accountService;
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;
        private readonly ITaskService _taskService;
        private readonly IDocumentService _documentService;
        private readonly IMasterService _masterService;

        #endregion

        public HomeController(IAccountService accountService,
           IInvoiceService invoiceService,
           IPaymentService paymentService,
           ITaskService taskService,
           IDocumentService documentService,
           IMasterService masterService)
        {
            _accountService = accountService;
            _invoiceService = invoiceService;
            _paymentService = paymentService;
            _taskService = taskService;
            _documentService = documentService;
            _masterService = masterService;
        }

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
        public async Task<JsonResult> LoadInvoiceHistory(string from, string to)
        {
            try
            {
                DateTime toDate = Convert.ToDateTime(to);
                DateTime fromDate = Convert.ToDateTime(from);
                toDate = toDate + new TimeSpan(23, 59, 59);
                fromDate = fromDate + new TimeSpan(00, 00, 1);
                var invoiceList = await _invoiceService.GetAllInvoice(fromDate, toDate);
                return Json(new { data = invoiceList }, JsonRequestBehavior.AllowGet);
            }
            catch
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
                return Json(await _invoiceService.GetAllInvoiceParticulars(invoiceId), JsonRequestBehavior.AllowGet);
            }
            catch
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
                    invoiceEntry.InvoiceDate = invoiceDetails != null && invoiceDetails.InvoiceDate != default ? invoiceDetails.InvoiceDate : DateTime.Now;
                    invoiceEntry.Remarks = invoiceDetails != null ? invoiceDetails.Remarks : string.Empty;
                }
                else
                    invoiceEntry.InvoiceId = default(int);
                ViewBag.Company = new SelectList(await _invoiceService.GetCompany(), "Id", "CompanyName", invoiceEntry.CompanyId);
                ViewBag.Client = new SelectList(await _invoiceService.GetClient(Convert.ToInt32(invoiceEntry.CompanyId)), "Id", "FullName", invoiceEntry.ClientId);
                ViewBag.Particular = new SelectList(await _invoiceService.GetParticular(), "Id", "ParticularFF", invoiceEntry.ParticularId);
                ViewBag.Year = new SelectList(GetYear(), "Key", "Value", invoiceEntry.Year);
                return PartialView("_InvoiceEntry", invoiceEntry);
            }
            catch
            {
                throw;
            }
        }

        private Dictionary<string, string> GetYear()
        {
            var years = new Dictionary<string, string>();
            for (int i = 5; i > 0; i--)
            {
                years.Add(
                    Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i + "/" + ((Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i) + 1),
                    Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i + "/" + ((Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i) + 1));
            }

            for (int i = 0; i < 5; i++)
            {
                if (!years.ContainsKey(Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i + "/" + ((Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i) + 1)))
                    years.Add(
                        Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i + "/" + ((Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i) + 1),
                        Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i + "/" + ((Convert.ToInt32(DateTime.Now.Year.ToString().Substring(1)) - i) + 1));
            }
            return years;
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
                var id = await _invoiceService.AddUpdateInvoiceEntry(Convert.ToInt32(SessionHelper.UserId), invoiceEntry);
                var invoice = await _invoiceService.GetInvoiceSingle(id);
                return Json(invoice, JsonRequestBehavior.AllowGet);
            }
            catch
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
                await _invoiceService.DeleteInvoiceParticularEntry(id);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
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
                var trackingDetails = await _invoiceService.GetTrackingInformationById(invoiceTrackingId);
                if (trackingDetails == null)
                    trackingDetails = new TrackingInformation { InvoiceId = invoiceId };
                return PartialView("_InvoiceTracking", trackingDetails);
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
        [HttpPost]
        public async Task<JsonResult> AddInvoiceTrackingInformation(TrackingInformation trackingInformation)
        {
            try
            {
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
                await _invoiceService.DeleteInvoiceById(invoiceId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {

                throw;
            }
        }

        /// <summary>
        /// Import invoice in bulk
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ImportInvoice()
        {
            try
            {
                List<InvoiceEntry> invoices = new List<InvoiceEntry>();
                List<InvoiceEntry> erroredInvoices = new List<InvoiceEntry>();
                List<ImportInvoices> invoiceParticulars = new List<ImportInvoices>();
                List<ImportInvoices> erroredParticulars = new List<ImportInvoices>();
                StringBuilder errors = new StringBuilder();
                HttpPostedFileBase importedFile = Request.Files[0];
                var allowedExtension = new string[] { ".xls", ".xlsx" };
                var extension = Path.GetExtension(importedFile.FileName);
                if (importedFile != null && importedFile.ContentType.Length > 0 && allowedExtension.Contains(extension)) /// Check if files fallin under selected extention or not
                {
                    var companies = await _masterService.GetAllCompany();
                    var clients = await _masterService.GetAllClient();
                    var particulars = await _invoiceService.GetParticular();

                    using (Stream inputStream = importedFile.InputStream)
                    {
                        //convert uploaded file data into bite 
                        MemoryStream memoryStream = inputStream as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            inputStream.CopyTo(memoryStream);
                        }

                        #region Fetch each sheets of uploaded file
                        IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(inputStream);
                        DataSet result = excelReader.AsDataSet();
                        for (int i = 4; i < result.Tables[0].Rows.Count; i++)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(result.Tables[0].Rows[i].ItemArray[0].ToString()))
                                {
                                    invoices.Add(new InvoiceEntry
                                    {
                                        Row = i + 1,
                                        CompanyName = result.Tables[0].Rows[i].ItemArray[4].ToString(),
                                        FullName = result.Tables[0].Rows[i].ItemArray[1].ToString(),
                                        CompanyId = companies.FirstOrDefault(_ => _.CompanyName.ToLower() == result.Tables[0].Rows[i].ItemArray[4].ToString().Replace("Receipt", "").Trim().ToLower())?.Id ?? 0,
                                        ClientId = clients.FirstOrDefault(_ => _.FullName.ToLower() == result.Tables[0].Rows[i].ItemArray[1].ToString().Trim().ToLower())?.Id ?? 0,
                                        InvoiceDate = Convert.ToDateTime(result.Tables[0].Rows[i].ItemArray[0]),
                                        Remarks = result.Tables[0].Rows[i].ItemArray[5] != null
                                                                        ? "Tally Invoice# " + result.Tables[0].Rows[i].ItemArray[5].ToString()
                                                                        : string.Empty
                                    });
                                }
                                else
                                {
                                    var particularName = result.Tables[0].Rows[i].ItemArray[1].ToString().Contains("Fees For")
                                                            ? result.Tables[0].Rows[i].ItemArray[1].ToString().Replace("Fees For F.Y.", "#").Split('#')[0].ToString().Trim()
                                                            : result.Tables[0].Rows[i].ItemArray[1].ToString().Replace("Fees F.Y.", "#").Split('#')[0].ToString().Trim();
                                    var particularYear = FormateYear(result.Tables[0].Rows[i].ItemArray[1].ToString());

                                    invoices.LastOrDefault().ImportInvoices.Add(new ImportInvoices
                                    {
                                        ParticularId = particulars.FirstOrDefault(_ => _.ParticularFF.ToLower() == particularName.ToLower())?.Id ?? 0,
                                        Amount = invoices.LastOrDefault().CompanyName.Contains("Receipt")
                                                                         ? Convert.ToDecimal(result.Tables[0].Rows[i].ItemArray[6])
                                                                         : Convert.ToDecimal(result.Tables[0].Rows[i].ItemArray[7]),
                                        Year = particularYear,
                                        ParticularName = particularName,
                                        Row = i + 1
                                    });
                                }
                            }
                            catch
                            {
                            }

                        }

                        // Add errors list if company, client, particulars not found for any invoice
                        // Remove those invoices and particulars from list
                        foreach (var invoice in invoices)
                        {
                            if (invoice.ClientId == 0 || invoice.CompanyId == 0)
                                errors.Append(string.Format("<br />Row# {0}, Errors : {1}", invoice.Row, (
                                                                                                    invoice.ClientId == 0
                                                                                                            ? "Client " + invoice.FullName + " is missing"
                                                                                                            : "") + " " + (invoice.CompanyId == 0
                                                                                                                                    ? "Company " + invoice.CompanyName + " is missing"
                                                                                                                                    : "")));
                            foreach (var particular in invoice.ImportInvoices)
                            {
                                if (particular.ParticularId == 0 && !invoice.CompanyName.Contains("Receipt"))
                                {
                                    errors.Append(string.Format("<br />Row# {0}, Errors : {1}", particular.Row, "Particular " + particular.ParticularName + " is missing"));
                                    erroredParticulars.Add(particular);
                                }
                            }
                            invoice.ImportInvoices.RemoveAll(_ => erroredParticulars.Any(x => x.Row == _.Row));

                            if (invoice.ClientId == 0 || invoice.CompanyId == 0)
                                erroredInvoices.Add(invoice);
                        };

                        invoices.RemoveAll(_ => erroredInvoices.Any(x => x.Row == _.Row));

                        // Import invoices and receipt if there are no any errors in uploaded sheet
                        if (!string.IsNullOrEmpty(errors.ToString()))
                        {
                            // Importing all receipt informations
                            foreach (var reciept in invoices.Where(_ => _.CompanyName.Contains("Receipt")))
                            {

                                try
                                {
                                    await _paymentService.AddUpdateReceiptEntry(new ReceiptDto
                                    {
                                        CompanyId = reciept.CompanyId,
                                        ClientId = reciept.ClientId,
                                        Discount = reciept.ImportInvoices?.FirstOrDefault(_ => _.ParticularName == "Discount")?.Amount ?? 0,
                                        TotalAmount = reciept.ImportInvoices?.FirstOrDefault(_ => _.ParticularName != "Discount")?.Amount ?? 0,
                                        RecieptDate = reciept.InvoiceDate,
                                        BankName = reciept.ImportInvoices?.FirstOrDefault(_ => _.ParticularName != "Discount")?.ParticularName ?? string.Empty,
                                        Status = true,
                                        Remarks = reciept.Remarks.Replace("Invoice", "Receipt"),
                                    });
                                }
                                catch
                                {
                                }
                            }

                            // Importing all invoices information
                            foreach (var invoice in invoices.Where(_ => !_.CompanyName.Contains("Receipt")))
                            {
                                try
                                {
                                    if (invoice.ImportInvoices?.Count > 0)
                                    {
                                        invoice.ParticularId = invoice.ImportInvoices.FirstOrDefault().ParticularId;
                                        invoice.Amount = invoice.ImportInvoices.FirstOrDefault().Amount;
                                        invoice.Year = invoice.ImportInvoices.FirstOrDefault().Year;
                                        var addedInvoiceId = await _invoiceService.AddUpdateInvoiceEntry(Convert.ToInt32(SessionHelper.UserId), invoice);
                                        if (addedInvoiceId > 0)
                                            for (int i = 1; i < invoice.ImportInvoices.Count; i++)
                                            {
                                                invoice.ParticularId = invoice.ImportInvoices[i].ParticularId;
                                                invoice.Amount = invoice.ImportInvoices[i].Amount;
                                                invoice.Year = invoice.ImportInvoices[i].Year;
                                                invoice.InvoiceId = addedInvoiceId;
                                                await _invoiceService.AddUpdateInvoiceEntry(Convert.ToInt32(SessionHelper.UserId), invoice);
                                            }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                        #endregion
                    }

                    return Content("success$" + errors.ToString());
                }
            }
            catch
            {
                throw;
            }
            return Content("error");
        }

        /// <summary>
        /// Get formated year by string i.e. 18/19, 19/20 etc..
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        private string FormateYear(string year)
        {
            try
            {
                var particularYear = "";
                year = year.Contains("Fees For")
                                            ? (year.Replace("Fees For F.Y.", "#").Split('#').Length > 1
                                                    ? year.Replace("Fees For F.Y.", "#").Split('#')[1].ToString().Trim()
                                                    : "")
                                            : (year.Replace("Fees F.Y.", "#").Split('#').Length > 1
                                                    ? year.Replace("Fees F.Y.", "#").Split('#')[1].ToString().Trim()
                                                    : "");
                var splitYear = year.Split('/');
                if (splitYear.Length > 1)
                {
                    particularYear = (splitYear[0].Length > 3
                                            ? splitYear[0].Substring(2, 2)
                                            : splitYear[0])
                                            + "/" + (splitYear[1].Length > 3
                                                             ? splitYear[1].Substring(2, 2)
                                                             : splitYear[1]);
                }
                return particularYear;
            }
            catch
            {
                return string.Empty;
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
        public async Task<JsonResult> LoadPaymentHistory(string from, string to)
        {
            try
            {
                DateTime fromDate = Convert.ToDateTime(from);
                DateTime toDate = Convert.ToDateTime(to);

                toDate = toDate + new TimeSpan(23, 59, 59);
                fromDate = fromDate + new TimeSpan(00, 00, 1);
                var receiptList = await _paymentService.GetAllReceipts(fromDate, toDate);
                return Json(new { data = receiptList }, JsonRequestBehavior.AllowGet);
            }
            catch
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
                var receiptDto = new ReceiptDto();
                if (id != null)
                    receiptDto = await _paymentService.GetReceipt(id ?? 0);
                receiptDto = receiptDto ?? new ReceiptDto();

                receiptDto.RecieptDate = receiptDto.RecieptDate != DateTime.MinValue ? receiptDto.RecieptDate : DateTime.Now;

                ViewBag.Company = new SelectList(await _invoiceService.GetCompany(), "Id", "CompanyName", receiptDto.CompanyId);
                ViewBag.Client = new SelectList(await _invoiceService.GetClient(Convert.ToInt32(receiptDto.CompanyId)), "Id", "FullName", receiptDto.ClientId);

                ViewBag.Invoice = new MultiSelectList(await _invoiceService.GetInvoiceByClientandCompany(Convert.ToInt32(receiptDto.CompanyId), Convert.ToInt32(receiptDto.ClientId), id), "InvoiceId", "InvoiceNumber", receiptDto.InvoiceIds);
                return PartialView("_ReceiptEntry", receiptDto);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get invoice by client and company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetInvoiceByClientAndCompany(int companyId, int clientId)
        {
            try
            {
                return Json(await _invoiceService.GetInvoiceByClientandCompany(companyId, clientId, null), JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get total amount for all invoices
        /// </summary>
        /// <param name="invoices"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetTotalOfInvoice(List<long> invoices)
        {
            try
            {
                return Json(await _invoiceService.GetInvoiceAmountTotal(invoices), JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(0, JsonRequestBehavior.AllowGet);
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
                await _paymentService.AddUpdateReceiptEntry(receiptEntry);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Remove receipt from db
        /// </summary>
        /// <param name="receiptId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> DeleteReceipt(int receiptId)
        {
            try
            {
                await _paymentService.DeleteReceipt(receiptId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Import receipt in bulk
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ImportReceipt()
        {
            try
            {
                List<ReceiptDto> receipts = new List<ReceiptDto>();
                HttpPostedFileBase importedFile = Request.Files[0];
                var allowedExtension = new string[] { ".xlx", ".xlsx" };
                var extension = Path.GetExtension(importedFile.FileName);
                if (importedFile != null && importedFile.ContentType.Length > 0 && allowedExtension.Contains(extension)) /// Check if files fallin under selected extention or not
                {
                    var companies = await _masterService.GetAllCompany();
                    var clients = await _masterService.GetAllClient();
                    var allInvoices = await _invoiceService.GetAllInvoice();

                    using (Stream inputStream = importedFile.InputStream)
                    {
                        //convert uploaded file data into bite 
                        MemoryStream memoryStream = inputStream as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            inputStream.CopyTo(memoryStream);
                        }

                        #region Fetch each sheets of uploaded file
                        ExcelPackage excelPackage = new ExcelPackage(inputStream);
                        bool isValidaSheet = true;
                        foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                        {
                            if (isValidaSheet)
                            {
                                var sheetDatas = worksheet.Cells.ToList();
                                for (int i = 2; i <= (sheetDatas.Count() / 7); i++)
                                {
                                    int companyId = companies.FirstOrDefault(_ => _.CompanyName == worksheet.Cells[i, 1].Value.ToString())?.Id ?? 0;
                                    long clientId = clients.FirstOrDefault(_ => _.Username == worksheet.Cells[i, 2].Value.ToString())?.Id ?? 0;
                                    List<long> invoiceIds = new List<long>();
                                    if (worksheet.Cells[i, 3].Value != null)
                                    {
                                        var invoiceNumbers = worksheet.Cells[i, 3].Value.ToString().Split(',').ToArray();
                                        invoiceIds = allInvoices.Where(_ => invoiceNumbers.Contains(_.InvoiceNumber) && _.CompanyId == companyId && _.ClientId == clientId).Select(_ => _.InvoiceId).ToList();
                                    }

                                    receipts.Add(new ReceiptDto
                                    {
                                        CompanyId = companyId,
                                        ClientId = clientId,
                                        InvoiceIds = invoiceIds,
                                        Discount = Convert.ToDecimal(worksheet.Cells[i, 4].Value.ToString()),
                                        TotalAmount = worksheet.Cells[i, 5].Value != null ? Convert.ToDecimal(worksheet.Cells[i, 5].Value.ToString()) : 0,
                                        RecieptDate = Convert.ToDateTime(worksheet.Cells[i, 6].Value.ToString()),
                                        BankName = worksheet.Cells[i, 7].Value != null ? worksheet.Cells[i, 7].Value.ToString() : string.Empty,
                                        ChequeNumber = worksheet.Cells[i, 8].Value != null ? worksheet.Cells[i, 8].Value.ToString() : string.Empty,
                                        Status = worksheet.Cells[i, 9].Value != null ? Convert.ToBoolean(Convert.ToInt32(worksheet.Cells[i, 9].Value.ToString())) : false,
                                        Remarks = worksheet.Cells[i, 10].Value != null ? worksheet.Cells[i, 10].Value.ToString() : string.Empty,
                                    });
                                }
                                isValidaSheet = false;
                            }
                        }

                        foreach (var receipt in receipts)
                        {
                            await _paymentService.AddUpdateReceiptEntry(receipt);
                        };
                        #endregion
                    }

                    return Content("success");
                }
            }
            catch (Exception ex)
            {
            }
            return Content("error");
        }
        #endregion

        #region Download Invoice & Payment receipt
        /// <summary>
        /// Generate invoice entry
        /// </summary>
        /// <returns></returns>
        public async Task GenerateInvoice(string id)
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Invoice");
            int currentCell = 1;

            var invoiceIds = id.TrimEnd(',').Split(',').ToList<string>();
            foreach (var invoiceId in invoiceIds)
            {
                var invoicDetails = await _invoiceService.DownloadInvoice(Convert.ToInt32(invoiceId));
                var companyDetail = await _invoiceService.GetCompanyDetailById(Convert.ToInt32(invoicDetails.InvoiceEntry.CompanyId));

                #region Excel set
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Value = companyDetail.CompanyName;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Font.Size = 16;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);
                currentCell++;

                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Value = companyDetail.Type;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Font.Size = 12;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);
                currentCell++;

                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Value = companyDetail.Address;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("B{0}", currentCell)].Value = "Bill No.";
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("C{0}", currentCell)].Value = companyDetail.Prefix + "-" + invoicDetails.InvoiceEntry.InvoiceNumber;
                Sheet.Cells[string.Format("C{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;


                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Value = "INVOICE";
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Font.UnderLine = true;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Font.Size = 12;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);

                Sheet.Cells[string.Format("H{0}", currentCell)].Value = "Date:";
                Sheet.Cells[string.Format("I{0}", currentCell)].Value = invoicDetails.InvoiceEntry.InvoiceDate.ToString("dd/MM/yyyy");
                Sheet.Column(9).Width = 12.30;
                Sheet.Cells[string.Format("I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                currentCell++;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("B{0}", currentCell)].Value = "Name:";
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Value = invoicDetails.InvoiceEntry.FullName;
                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("B{0}", currentCell)].Value = "Address:";
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Value = invoicDetails.InvoiceEntry.Address;
                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Merge = true;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Value = "Particulars";
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Value = "Amount";
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                currentCell++;

                for (int i = currentCell; i < (invoicDetails.Particulars.Count() + currentCell); i++)
                {
                    Sheet.Cells[string.Format("B{0}:G{0}", i)].Value = string.Format("{0} Fees For F.Y.{1}", invoicDetails.Particulars[i - currentCell].FFParticulars, invoicDetails.Particulars[i - currentCell].Year);
                    Sheet.Cells[string.Format("B{0}:G{0}", i)].Merge = true;
                    Sheet.Cells[string.Format("B{0}:G{0}", i)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    Sheet.Cells[string.Format("B{0}:G{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    Sheet.Cells[string.Format("B{0}:G{0}", i)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

                    Sheet.Cells[string.Format("H{0}:I{0}", i)].Value = invoicDetails.Particulars[i - currentCell].Amount;
                    Sheet.Cells[string.Format("H{0}:I{0}", i)].Merge = true;
                    Sheet.Cells[string.Format("H{0}:I{0}", i)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    Sheet.Cells[string.Format("H{0}:I{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                }

                currentCell = (invoicDetails.Particulars.Count() + currentCell);
                for (int i = currentCell; i < currentCell + (invoicDetails.Particulars.Count() <= 5 ? 3 : 1); i++)
                {
                    Sheet.Cells[string.Format("B{0}:G{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    Sheet.Cells[string.Format("B{0}:G{0}", i)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                    Sheet.Cells[string.Format("B{0}:G{0}", i)].Merge = true;
                    Sheet.Cells[string.Format("H{0}:I{0}", i)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                    Sheet.Cells[string.Format("H{0}:I{0}", i)].Merge = true;
                }

                currentCell = currentCell + (invoicDetails.Particulars.Count() <= 5 ? 3 : 1);
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Value = "Amount Chargeable (in words)";
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                Sheet.Cells[string.Format("G{0}", currentCell)].Value = "Total";
                Sheet.Cells[string.Format("G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Value = invoicDetails.Particulars.Sum(_ => _.Amount);
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Value = "Rupees " + CurrencyConvertor.NumberToWords(Convert.ToInt32(invoicDetails.Particulars.Sum(_ => _.Amount))) + " Only";
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Value = "";
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                currentCell++;

                Sheet.Cells[string.Format("B{0}", currentCell)].Value = "For F.Y.";
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;

                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Value = invoicDetails.Particulars?.FirstOrDefault()?.Year;
                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                Sheet.Cells[string.Format("C{0}:I{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thick;
                currentCell += 5;

                Sheet.Cells[string.Format("G{0}:I{0}", currentCell)].Value = "Authorized Signatory";
                Sheet.Cells[string.Format("G{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                #endregion

                currentCell += 5;

                Sheet.Row(currentCell - 1).PageBreak = true;
                Sheet.Column(currentCell).PageBreak = true;

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
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Receipt");
            int currentCell = 1;


            var receiptIds = id.TrimEnd(',').Split(',').ToList<string>();
            foreach (var receiptId in receiptIds)
            {
                var receiptDetails = await _paymentService.GetReceipt(Convert.ToInt32(receiptId));
                var companyDetail = await _invoiceService.GetCompanyDetailById(Convert.ToInt32(receiptDetails.CompanyId));

                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Value = companyDetail.CompanyName;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.Font.Size = 16;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);
                Sheet.Cells[string.Format("I{0}", currentCell)].Value = "";
                Sheet.Cells[string.Format("I{0}", currentCell)].Style.Font.Bold = true;
                currentCell++;

                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Value = companyDetail.Type;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.Font.Size = 12;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                Sheet.Cells[string.Format("I{0}", currentCell)].Value = "";
                Sheet.Cells[string.Format("I{0}", currentCell)].Style.Font.Bold = true;
                currentCell++;

                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Value = companyDetail.Address;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("C{0}:H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("I{0}", currentCell)].Value = "";
                Sheet.Cells[string.Format("I{0}", currentCell)].Style.Font.Bold = true;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("B{0}", currentCell)].Value = "Rpt No.";
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("C{0}", currentCell)].Value = companyDetail.Prefix + "-" + receiptDetails.RecieptNo;
                Sheet.Cells[string.Format("C{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;


                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Value = "RECEIPT";
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Font.Size = 12;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Fill.BackgroundColor.SetColor(GetColor(companyDetail.PreferedColor ?? 0));
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("E{0}:F{0}", currentCell)].Style.Font.Color.SetColor(System.Drawing.Color.White);

                Sheet.Cells[string.Format("H{0}", currentCell)].Value = "Date:";
                Sheet.Cells[string.Format("H{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("I{0}", currentCell)].Value = receiptDetails.RecieptDate.ToString("dd/MM/yyyy");
                Sheet.Column(9).Width = 12.30;
                Sheet.Column(8).Width = 9.30;
                Sheet.Cells[string.Format("I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                currentCell++;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Value = "Particulars";
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Value = "Amount";
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                currentCell++;

                Sheet.Cells[string.Format(string.Format("B{0}:G{0}", currentCell), currentCell)].Value = !string.IsNullOrEmpty(receiptDetails.ClientName) ? receiptDetails.ClientName : receiptDetails.Invoices.FirstOrDefault()?.FullName ?? string.Empty;
                Sheet.Cells[string.Format(string.Format("B{0}:G{0}", currentCell), currentCell)].Merge = true;
                Sheet.Cells[string.Format(string.Format("B{0}:G{0}", currentCell), currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format(string.Format("B{0}:G{0}", currentCell), currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format(string.Format("B{0}:G{0}", currentCell), currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Value = receiptDetails.TotalAmount;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                currentCell++;

                Sheet.Cells[string.Format("B{0}", currentCell)].Value = "Against Ref.";
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Column(2).Width = 13.30;
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                Sheet.Cells[string.Format("C{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("C{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("C{0}:G{0}", currentCell)].Value = string.Join("&", receiptDetails.Invoices.Select(_ => _.InvoiceNumber));
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;

                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Value = receiptDetails.BankName + " " + receiptDetails.ChequeNumber;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Value = "Discount";
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                if (receiptDetails.Discount > 0)
                    Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Value = receiptDetails.Discount;
                else
                    Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Value = "";
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                #region Line Break
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;
                #endregion

                Sheet.Cells[string.Format("B{0}", currentCell)].Value = "Against F.Y.";
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("B{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("C{0}:G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("C{0}:G{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("C{0}:G{0}", currentCell)].Value = receiptDetails.Year;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Value = "Amount Received (in words)";
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("B{0}:F{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("G{0}", currentCell)].Value = "Total";
                Sheet.Cells[string.Format("G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("G{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Value = receiptDetails.TotalAmount - receiptDetails.Discount;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Font.Bold = true;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                Sheet.Cells[string.Format("H{0}:I{0}", currentCell)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                currentCell++;

                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Value = "Rupees " + CurrencyConvertor.NumberToWords(Convert.ToInt32(receiptDetails.TotalAmount - receiptDetails.Discount)) + " Only";
                Sheet.Cells[string.Format("B{0}:I{0}", currentCell)].Merge = true;

                currentCell += 5;

                Sheet.Cells[string.Format("G{0}:I{0}", currentCell)].Value = "Authorized Signatory";
                Sheet.Cells[string.Format("G{0}:I{0}", currentCell)].Merge = true;
                Sheet.Cells[string.Format("G{0}:I{0}", currentCell)].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                currentCell += 5;

                Sheet.Row(currentCell - 1).PageBreak = true;
                Sheet.Column(11).PageBreak = true;
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
                    .Replace("##DATE##", invoicDetails.InvoiceEntry.InvoiceDate.ToString("dd/MM/yyyy"))
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
            return File(fileData, "application/octet-stream", "Invoice.pdf");
        }


        public async Task<ActionResult> GenerateReceiptPDF(string id)
        {
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
                    .Replace("##DATE##", receiptDetails.RecieptDate.ToString("dd/MM/yyyy"))
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
            await _invoiceService.GetAllCompanyWithClients();

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
            return View();
        }

        public async Task<JsonResult> LoadUsers()
        {
            try
            {
                var users = await _accountService.GetAllUsersList();
                return Json(new { data = users }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
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
            catch
            {
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
                await _accountService.AddUpadate(user);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// remove User entry
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            try
            {
                await _accountService.RemoveUser(userId);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
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
            var tasks = await _taskService.GetAll(Convert.ToInt32(SessionHelper.UserId), Convert.ToInt32(SessionHelper.UserTypeId));
            return Json(tasks, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get current user notification
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetAllNotification(int type)
        {
            var notification = await _masterService.GetAllNotification(Convert.ToInt32(SessionHelper.UserId), Convert.ToInt32(SessionHelper.UserTypeId), type);
            return Json(notification, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadDocument(int documentID)
        {
            try
            {
                var documnet = _documentService.GetDocumentByID(documentID).Result;
                string folderPath = Server.MapPath("~/Content/Documents/" + documentID + "_" + documnet.FileName);
                byte[] fileBytes = System.IO.File.ReadAllBytes(@folderPath);
                string fileName = documnet.FileName;
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch
            {
                return Content("Some error occurred during download file. Please contact support team.");
            }
        }

        #endregion

        /// <summary>
        /// 404 page 
        /// </summary>
        /// <returns></returns>
        public ActionResult NotFound()
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
    <div style='page-break-after:always'></div><head>
            <style>
                body {
                font - family: 'Segoe UI', Tahoma, Arial, Helvetica, sans - serif;
                font - size: .813em;
                color: #222;
    background - color: #fff;
	margin: 3 % 18 % 3 % 18 %;
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
    <body style='margin:40px 40px;'>
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
   <div style='page-break-after:always'></div> <head>
            <style>
                body {
                font - family: 'Segoe UI', Tahoma, Arial, Helvetica, sans - serif;
                font - size: .813em;
                color: #222;
    background - color: #fff;
	margin: 3 % 18 % 3 % 18 %;
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
    <body  style='margin:40px 40px;'>
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