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

        public async Task<List<User>> GetClient(int id)
        {
            return await (from cd in _dbContext.Users
                          join m in _dbContext.Company_Client_Mapping on cd.Id equals (m.UserId ?? 0)
                          where m.CompanyId == id
                          select cd).ToListAsync();
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
                    where invoiceParticular.InvoiceId == invoiceId && particular.IsExclude == false
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
                                                     FullName = user.FullName,
                                                     CompanyId = invoice.CompanyId
                                                 }).FirstOrDefault();

            documentViewDownload.Particulars = await GetAllInvoiceParticulars(id);
            return documentViewDownload;
        }

        public async Task<List<InvoiceEntry>> GetAllInvoice()
        {
            return await (from invoice in _dbContext.InvoiceDetails
                          select new InvoiceEntry()
                          {
                              InvoiceId = invoice.Id,
                              InvoiceNumber = invoice.Invoice_Number
                          }).ToListAsync();
        }

        public async Task<List<InvoiceEntry>> GetInvoiceByClientandCompany(int companyId, int clientId)
        {
            return await (from invoice in _dbContext.InvoiceDetails
                          join invoiceParti in _dbContext.InvoiceParticulars
                          on invoice.Id equals invoiceParti.InvoiceId
                          join parti in _dbContext.Particulars
                          on invoiceParti.ParticularId equals parti.Id
                          where (invoice.ClientId == clientId && invoice.CompanyId == companyId /*&& parti.isExcluded ==false*/)
                          select new InvoiceEntry()
                          {
                              CompanyId = invoice.CompanyId,
                              ClientId = invoice.ClientId,
                              InvoiceId = invoice.Id,
                              InvoiceNumber = invoice.Invoice_Number,
                              ParticularId = invoiceParti.ParticularId,
                              Amount = invoiceParti.Amount

                          }).ToListAsync();
        }

        #region company

        public async Task<CompanyDto> GetCompanyDetailById(int comId)
        {
            return (from company in _dbContext.Company_master
                    where company.Id == comId
                    select new CompanyDto
                    {
                        Address = company.Address,
                        CompanyBanner = company.CompanyBanner,
                        CompanyName = company.CompanyName,
                        IsActive = company.IsActive,
                        Id = company.Id,
                        PreferedColor = company.PreferedColor,
                        Type = company.Type
                    }).FirstOrDefault();
        }

        #endregion


        #region Private Method
        private string GenerateInvoiceNumber(int companyId)
        {
            var genericNumber = _dbContext.InvoiceDetails.OrderByDescending(_ => _.Id).Select(_ => _.Invoice_Number).FirstOrDefault();
            var compName = _dbContext.Company_master.Where(_ => _.Id == companyId).Select(_ => _.CompanyName).FirstOrDefault();
            if (!string.IsNullOrEmpty(genericNumber) && genericNumber.Split('-').Count() > 1)
            {
                genericNumber = (Convert.ToInt32(genericNumber.Split('-')[1].Substring(1, (genericNumber.Split('-')[1].Length - 1))) + 1).ToString();
            }
            else
                genericNumber = "1";

            return (compName.ToUpper().Substring(0, 3) + "-I" + genericNumber);
        }

        public async Task<List<TrackingInformation>> GetTrackingInformation(int invoiceId)
        {
            return (from tracking in _dbContext.InvoiceTrackings.ToList()
                    where tracking.InvoiceId == invoiceId
                    select new TrackingInformation()
                    {
                        TrackingId = tracking.id,
                        InvoiceId = tracking.InvoiceId,
                        Comment = tracking.Comment,
                        CreatedAt = tracking.CreatedAt.Value.ToString("MM/dd/yyyy")
                    }).ToList();
        }

        public async Task<bool> AddTrackingInformation(TrackingInformation trackingInformation)
        {
            try
            {
                if (trackingInformation.TrackingId > 0)
                {
                    var trackingDetails = await _dbContext.InvoiceTrackings.FirstOrDefaultAsync(_ => _.id == trackingInformation.TrackingId);
                    trackingDetails.Comment = trackingInformation.Comment;
                }
                else
                    _dbContext.InvoiceTrackings.Add(new InvoiceTracking
                    {
                        Comment = trackingInformation.Comment,
                        CreatedAt = DateTime.Now,
                        InvoiceId = trackingInformation.InvoiceId
                    });

                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<TrackingInformation> GetTrackingInformationById(int invoiceTrackingId)
        {

            var trackingDetails = await _dbContext.InvoiceTrackings.FirstOrDefaultAsync(_ => _.id == invoiceTrackingId);
            if (trackingDetails == null) return null;

            return new TrackingInformation
            {
                TrackingId = trackingDetails.id,
                Comment = trackingDetails.Comment,
                InvoiceId = trackingDetails.InvoiceId
            };
        }

        public async Task<bool> RemoveInvoiceTracking(int invoiceTrackingId)
        {
            try
            {
                _dbContext.InvoiceTrackings.Remove(await _dbContext.InvoiceTrackings.FirstOrDefaultAsync(_ => _.id == invoiceTrackingId));
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public async Task<decimal> GetInvoiceAmountTotal(List<long> invoices)
        {
            return await (from pm in _dbContext.InvoiceParticulars
                          join p in _dbContext.Particulars on pm.ParticularId equals p.Id
                          where (p.IsExclude ?? true) == false && invoices.Contains(pm.InvoiceId)
                          select pm).SumAsync(_ => _.Amount);
        }

        #endregion
    }
}
