using Arity.Data;
using Arity.Data.Dto;
using Arity.Service.Contract;
using Arity.Service.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service
{
    public class InvoiceService : IInvoiceService
    {
        private readonly RMNEntities _dbContext;
        public InvoiceService()
        {
            _dbContext = new RMNEntities();
        }

        public async Task<List<Company_master>> GetCompany()
        {

            try
            {
                var CompanyList = await _dbContext.Company_master.ToListAsync();
                return CompanyList;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<User>> GetClient(int CompanyId)
        {
            return await (from client in _dbContext.Users
                          join companyMapp in _dbContext.Company_Client_Mapping on client.Id equals (companyMapp.UserId ?? 0)
                          where companyMapp.CompanyId == CompanyId
                          select client)
                    .ToListAsync();
        }

        public async Task<List<Particular>> GetParticular()
        {
            return await _dbContext.Particulars.ToListAsync();
        }
        public async Task AddUpdateInvoiceEntry(int CompanyId, InvoiceEntry invoiceEntry)
        {
            if (invoiceEntry.InvoiceParticularId > 0)
            {
                var existingParticular = await _dbContext.InvoiceParticulars.Where(_ =>
                _.Id == invoiceEntry.InvoiceParticularId).FirstOrDefaultAsync();

                existingParticular.Amount = invoiceEntry.Amount;
                existingParticular.year = invoiceEntry.Year;
                existingParticular.CreatedDate = invoiceEntry.CreatedDate ?? DateTime.Now;
                existingParticular.UpdatedDate = DateTime.Now;
            }
            else
            {
                InvoiceDetail invoiceDetail = new InvoiceDetail();
                if (invoiceEntry.InvoiceId > 0)
                {
                    invoiceDetail = await _dbContext.InvoiceDetails.Where(_ => _.Id == invoiceEntry.InvoiceId).FirstOrDefaultAsync();
                }
                else
                {
                    //var companyDeail = await GetCompanyDetailById((int)invoiceDetail.CompanyId);
                    invoiceDetail.ClientId = invoiceEntry.ClientId;
                    invoiceDetail.CompanyId = invoiceEntry.CompanyId;
                    invoiceDetail.Invoice_Number = GenerateInvoiceNumber((int)invoiceDetail.CompanyId);
                    invoiceDetail.UpdatedDate = DateTime.Now;
                    invoiceDetail.CreatedDate = DateTime.Now;
                    _dbContext.InvoiceDetails.Add(invoiceDetail);
                    await _dbContext.SaveChangesAsync();
                }

                InvoiceParticular invoiceParticular = new InvoiceParticular();
                invoiceParticular.Amount = invoiceEntry.Amount;
                invoiceParticular.year = invoiceEntry.Year;
                invoiceParticular.CreatedDate = DateTime.Now;
                invoiceParticular.UpdatedDate = DateTime.Now;
                invoiceParticular.InvoiceId = invoiceDetail.Id;
                invoiceParticular.ParticularId = invoiceEntry.ParticularId;

                _dbContext.InvoiceParticulars.Add(invoiceParticular);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<InvoiceEntry> GetInvoice(int id)
        {
            return await (from invoiceParticular in _dbContext.InvoiceParticulars
                          join invoice in _dbContext.InvoiceDetails on invoiceParticular.InvoiceId equals invoice.Id
                          where invoiceParticular.Id == id
                          select new InvoiceEntry()
                          {
                              InvoiceParticularId = invoiceParticular.Id,
                              Amount = invoiceParticular.Amount,
                              Year = invoiceParticular.year,
                              CreatedDate = invoiceParticular.CreatedDate,
                              UpdatedDate = invoiceParticular.UpdatedDate,
                              ClientId = invoice.ClientId,
                              CompanyId = invoice.CompanyId,
                              InvoiceId = invoice.Id,
                              ParticularId = invoiceParticular.ParticularId

                          }).FirstOrDefaultAsync();
        }

        public async Task<InvoiceEntry> GetInvoiceSingle(int invoiceId)
        {
            return await (from invoice in _dbContext.InvoiceDetails
                          where invoice.Id == invoiceId
                          select new InvoiceEntry()
                          {
                              ClientId = invoice.ClientId,
                              CompanyId = invoice.CompanyId,
                              InvoiceId = invoice.Id
                          }).FirstOrDefaultAsync();
        }

        public async Task<List<InvoiceEntry>> GetAllInvoice(DateTime fromDate, DateTime toDate)
        {
            return (from invoice in _dbContext.InvoiceDetails.ToList()
                    join user in _dbContext.Users.ToList() on invoice.ClientId equals user.Id
                    where invoice.CreatedDate >= fromDate && invoice.CreatedDate <= toDate
                    select new InvoiceEntry()
                    {
                        Amount = _dbContext.InvoiceParticulars.Where(_ => _.InvoiceId == invoice.Id).Sum(_ => (decimal?)_.Amount) ?? 0,
                        CreatedDateString = invoice.CreatedDate.ToString("MM/dd/yyyy"),
                        UpdatedDate = invoice.UpdatedDate,
                        InvoiceNumber = invoice.Invoice_Number,
                        InvoiceId = invoice.Id,
                        Address = user.Address,
                        City = user.City,
                        FullName = user.FullName
                    }).ToList();
        }

        public async Task<List<InvoiceEntry>> GetAllInvoiceParticulars(int invoiceId)
        {
            return (from invoiceParticular in _dbContext.InvoiceParticulars.ToList()
                    join particular in _dbContext.Particulars.ToList() on invoiceParticular.ParticularId equals particular.Id
                    where invoiceParticular.InvoiceId == invoiceId
                    select new InvoiceEntry()
                    {
                        Amount = invoiceParticular.Amount,
                        CreatedDateString = invoiceParticular.CreatedDate.ToString("MM/dd/yyyy"),
                        SFParticulars = particular.ParticularSF,
                        FFParticulars = particular.ParticularFF,
                        InvoiceParticularId = invoiceParticular.Id,
                        Year = invoiceParticular.year,
                        InvoiceId = invoiceParticular.InvoiceId
                    }).ToList();
        }

        public async Task DeleteInvoiceParticularEntry(int id)
        {
            _dbContext.InvoiceParticulars.Remove(await _dbContext.InvoiceParticulars.FirstOrDefaultAsync(_ => _.Id == id));
            await _dbContext.SaveChangesAsync();
        }

        public async Task<DocumentViewDownload> DownloadInvoice(int id)
        {
            DocumentViewDownload documentViewDownload = new DocumentViewDownload();
            documentViewDownload.InvoiceEntry = (from invoice in _dbContext.InvoiceDetails.ToList()
                                                 join user in _dbContext.Users.ToList() on invoice.ClientId equals user.Id
                                                 where invoice.Id == id
                                                 select new InvoiceEntry()
                                                 {
                                                     Amount = _dbContext.InvoiceParticulars.Where(_ => _.InvoiceId == invoice.Id).Sum(_ => (decimal?)_.Amount) ?? 0,
                                                     CreatedDate = invoice.CreatedDate,
                                                     UpdatedDate = invoice.UpdatedDate,
                                                     InvoiceNumber = invoice.Invoice_Number,
                                                     InvoiceId = invoice.Id,
                                                     Address = user.Address,
                                                     City = user.City,
                                                     FullName = user.FullName
                                                 }).FirstOrDefault();

            documentViewDownload.Particulars = await GetAllInvoiceParticulars(id);

            //Document document = new Document(PageSize.A4, 94f, 94f, 10f, 10f);
            //Font NormalFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);

            //using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            //{
            //    PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            //    Phrase phrase = null;
            //    PdfPCell cell = null;
            //    PdfPTable table = null;

            //    document.Open();

            //    //Header Table
            //    table = new PdfPTable(5);
            //    table.HorizontalAlignment = Element.ALIGN_CENTER;
            //    //table.SetWidths(new float[] { 0.3f, 1f });
            //    table.SpacingBefore = 20f;

            //    // Header
            //    cell = PhraseCell(new Phrase("Nilesh Narshana & Associates", FontFactory.GetFont("Arial", 12, Font.BOLD)), PdfPCell.ALIGN_CENTER);
            //    cell.Colspan = 5;
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    // Address
            //    cell = PhraseCell(new Phrase(invoiceDetails.Address, FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_CENTER);
            //    cell.Colspan = 5;
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    // breka line 
            //    cell = PhraseCell(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.Colspan = 5;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    // Bill no
            //    cell = PhraseCell(new Phrase("Bill No.", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);
            //    cell = PhraseCell(new Phrase(invoiceDetails.InvoiceNumber, FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    // Bill no
            //    cell = PhraseCell(new Phrase("INVOICE", FontFactory.GetFont("Arial", 10, Font.BOLD)), PdfPCell.ALIGN_CENTER);
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);


            //    // Date 
            //    cell = PhraseCell(new Phrase("Date :", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);
            //    cell = PhraseCell(new Phrase(invoiceDetails.CreatedDate.Value.ToString("MM/dd/yyyy"), FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.RIGHT_BORDER);
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    // breka line 
            //    cell = PhraseCell(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.Colspan = 5;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    // Name 
            //    cell = PhraseCell(new Phrase("Name :", FontFactory.GetFont("Arial", 10, Font.BOLD)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 10f;
            //    cell.Colspan = 1;
            //    table.AddCell(cell);
            //    cell = PhraseCell(new Phrase(invoiceDetails.FullName, FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.Colspan = 4;
            //    cell.PaddingBottom = 10f;
            //    table.AddCell(cell);

            //    // address 
            //    cell = PhraseCell(new Phrase("Address :", FontFactory.GetFont("Arial", 10, Font.BOLD)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 30f;
            //    cell.Colspan = 1;
            //    table.AddCell(cell);
            //    cell = PhraseCell(new Phrase(invoiceDetails.Address, FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.Colspan = 4;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);


            //    // Particular & Amount 
            //    cell = PhraseCell(new Phrase("Particulars", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_CENTER);
            //    cell.BorderWidth = 1;
            //    cell.PaddingBottom = 10f;
            //    cell.Colspan = 3;
            //    table.AddCell(cell);
            //    cell = PhraseCell(new Phrase("Amount", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_CENTER);
            //    cell.BorderWidth = 1;
            //    cell.Colspan = 2;
            //    cell.PaddingBottom = 10f;
            //    table.AddCell(cell);

            //    foreach (var particular in particulars)
            //    {
            //        cell = PhraseCell(new Phrase(particular.FFParticulars + " Fees For F.Y." + particular.Year, FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //        cell.BorderWidth = 1;
            //        cell.PaddingBottom = 10f;
            //        cell.Colspan = 3;
            //        table.AddCell(cell);
            //        cell = PhraseCell(new Phrase(particular.Amount.ToString(), FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_RIGHT);
            //        cell.BorderWidth = 1;
            //        cell.Colspan = 2;
            //        cell.PaddingBottom = 10f;
            //        table.AddCell(cell);

            //    }

            //    // breka line 
            //    cell = PhraseCell(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.Colspan = 5;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    // total
            //    cell = PhraseCell(new Phrase("Amount Chargable (in words)", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 1;
            //    cell.PaddingBottom = 10f;
            //    cell.Colspan = 3;
            //    table.AddCell(cell);
            //    cell = PhraseCell(new Phrase("Total", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_RIGHT);
            //    cell.BorderWidth = 0;
            //    cell.Colspan = 1;
            //    cell.PaddingBottom = 10f;
            //    table.AddCell(cell);
            //    cell = PhraseCell(new Phrase(particulars.Sum(_ => _.Amount).ToString(), FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_RIGHT);
            //    cell.BorderWidth = 1;
            //    cell.Colspan = 1;
            //    cell.PaddingBottom = 10f;
            //    table.AddCell(cell);

            //    cell = PhraseCell(new Phrase("Rupees One Thousand  Only ", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.PaddingBottom = 10f;
            //    cell.Colspan = 4;
            //    table.AddCell(cell);
            //    cell = PhraseCell(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 1;
            //    cell.Colspan = 1;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    cell = PhraseCell(new Phrase("For F.Y.", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 1;
            //    cell.Colspan = 1;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);
            //    cell = PhraseCell(new Phrase(particulars.FirstOrDefault().Year, FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 1;
            //    cell.Colspan = 4;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    cell = PhraseCell(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.Colspan = 5;
            //    cell.PaddingBottom = 60f;
            //    table.AddCell(cell);

            //    cell = PhraseCell(new Phrase("", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_LEFT);
            //    cell.BorderWidth = 0;
            //    cell.Colspan = 3;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);

            //    cell = PhraseCell(new Phrase("Authorised Signatory", FontFactory.GetFont("Arial", 10, Font.NORMAL)), PdfPCell.ALIGN_CENTER);
            //    cell.BorderWidth = 0;
            //    cell.Colspan = 2;
            //    cell.PaddingBottom = 30f;
            //    table.AddCell(cell);
            //    document.Add(table);
            //    document.Close();
            //    memoryStream.Close();
            //    DocumentViewDownload documentViewDownload = new DocumentViewDownload();
            //    documentViewDownload.ContentType = "pdf";
            //    documentViewDownload.ByteArray = memoryStream.ToArray();
            //    documentViewDownload.DocumentName = invoiceDetails.InvoiceNumber + ".pdf";
            return documentViewDownload;
        }

        #region company

        public async Task<CompanyDto> GetCompanyDetailById(int comId)
        {
            var CompanyDetail = (from Company in _dbContext.Company_master
                                 where Company.Id == comId
                                 select new Company_master()
                                 {
                                     Id = Company.Id,
                                     CompanyName = Company.CompanyName,
                                     CompanyBanner = Company.CompanyBanner,
                                     Address = Company.Address
                                 }).FirstOrDefault();
            return AutoMapper.Mapper.Map<CompanyDto>(CompanyDetail);
        }


        #endregion


        #region Private Method
        //private static PdfPCell PhraseCell(Phrase phrase, int align)
        //{
        //    PdfPCell cell = new PdfPCell(phrase);
        //    cell.HorizontalAlignment = align;
        //    cell.PaddingBottom = 2f;
        //    cell.PaddingTop = 0f;
        //    return cell;
        //}

        private string GenerateInvoiceNumber(int companyId)
        {
            var count = GetCompnyCount(companyId);
            var CompName = _dbContext.Company_master.Where(_ => _.Id == companyId).Select(_ => _.CompanyName).FirstOrDefault();

            return (CompName.Substring(0, 3) + "_" + count);
        }
        private int GetCompnyCount(int comId)
        {
            var count = _dbContext.InvoiceDetails.Where(_ => _.CompanyId == comId).ToList().Count();

            return count;
        }

        #endregion
    }
}
