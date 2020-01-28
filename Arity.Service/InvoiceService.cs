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
                                                     FullName = user.FullName,
                                                     CompanyId = invoice.CompanyId
                                                 }).FirstOrDefault();

            documentViewDownload.Particulars = await GetAllInvoiceParticulars(id);
            return documentViewDownload;
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
            var count = GetCompnyCount(companyId);
            var CompName = _dbContext.Company_master.Where(_ => _.Id == companyId).Select(_ => _.CompanyName).FirstOrDefault();

            return (CompName.Substring(0, 3) + "_" + count);
        }
        private int GetCompnyCount(int comId)
        {
            var count = _dbContext.InvoiceDetails.Where(_ => _.CompanyId == comId).ToList().Count();

            return count;
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

        #endregion
    }
}
