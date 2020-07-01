using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service.Contract;
using Arity.Service.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service
{
    public class InvoiceService : IInvoiceService
    {
        private readonly RMNEntities _dbContext;
        public InvoiceService(RMNEntities rmnEntities)
        {
            _dbContext = rmnEntities;
        }

        public async Task<List<Company_master>> GetCompany()
        {

            try
            {
                var CompanyList = await _dbContext.Company_master.ToListAsync();
                return CompanyList;
            }
            catch
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
        public async Task<int> AddUpdateInvoiceEntry(int CompanyId, InvoiceEntry invoiceEntry)
        {
            InvoiceDetail invoiceDetail = new InvoiceDetail();
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
                if (invoiceEntry.InvoiceId > 0)
                {
                    invoiceDetail = await _dbContext.InvoiceDetails.Where(_ => _.Id == invoiceEntry.InvoiceId).FirstOrDefaultAsync();
                    invoiceDetail.Remarks = invoiceEntry.Remarks;
                }
                else
                {
                    invoiceDetail.ClientId = invoiceEntry.ClientId;
                    invoiceDetail.CompanyId = invoiceEntry.CompanyId;
                    invoiceDetail.Invoice_Number = GenerateInvoiceNumber((int)invoiceDetail.CompanyId, invoiceEntry.InvoiceDate);
                    invoiceDetail.UpdatedDate = DateTime.Now;
                    invoiceDetail.CreatedDate = DateTime.Now;
                    invoiceDetail.InvoiceDate = invoiceEntry.InvoiceDate;
                    invoiceDetail.CreatedBy = Convert.ToInt32(SessionHelper.UserId);
                    invoiceDetail.Remarks = invoiceEntry.Remarks;
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
                invoiceParticular.CreatedBy = Convert.ToInt32(SessionHelper.UserId);

                _dbContext.InvoiceParticulars.Add(invoiceParticular);
            }
            await _dbContext.SaveChangesAsync();
            return Convert.ToInt32(invoiceDetail.Id);
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
                              ParticularId = invoiceParticular.ParticularId,
                              Remarks = invoice.Remarks

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
                              InvoiceId = invoice.Id,
                              InvoiceDate = invoice.InvoiceDate,
                              CreatedDate = invoice.CreatedDate,
                              CreatedBy = invoice.CreatedBy,
                              Remarks = invoice.Remarks
                          }).FirstOrDefaultAsync();
        }

        public async Task<List<InvoiceEntry>> GetAllInvoice(DateTime fromDate, DateTime toDate)
        {
            if (SessionHelper.UserTypeId == (int)Arity.Service.Core.UserType.User)
                return (from invoice in _dbContext.InvoiceDetails.ToList()
                        join company in _dbContext.Company_master.ToList() on invoice.CompanyId equals company.Id
                        join user in _dbContext.Users.ToList() on invoice.ClientId equals user.Id
                        join createdby in _dbContext.Users.ToList() on invoice.CreatedBy equals createdby.Id
                        where invoice.CreatedDate >= fromDate && invoice.CreatedDate <= toDate && invoice.ClientId == SessionHelper.UserId
                        select new InvoiceEntry()
                        {
                            Amount = _dbContext.InvoiceParticulars.Where(_ => _.InvoiceId == invoice.Id).Sum(_ => (decimal?)_.Amount) ?? 0,
                            CreatedDateString = invoice.InvoiceDate.ToString("dd/MM/yyyy"),
                            UpdatedDate = invoice.UpdatedDate,
                            InvoiceNumber = company.Prefix + "-" + invoice.Invoice_Number,
                            InvoiceId = invoice.Id,
                            Address = user.Address + ", " + user.City,
                            City = user.City,
                            FullName = user.FullName,
                            CreatedBy = invoice.CreatedBy,
                            CompanyName = company.CompanyName,
                            CreatedByString = createdby.FullName,
                            Year = _dbContext.InvoiceParticulars.Where(_ => _.InvoiceId == invoice.Id)?.FirstOrDefault()?.year ?? string.Empty,
                            GroupName = (user.GroupId ?? 0) > 0 ? _dbContext.GroupMasters.FirstOrDefault(_ => _.GroupId == user.GroupId).Name : string.Empty,
                            AddedBy = Convert.ToInt32(createdby.UserTypeId)
                        }).ToList();
            else if (SessionHelper.UserTypeId == (int)Arity.Service.Core.UserType.MasterAdmin)

                return (from invoice in _dbContext.InvoiceDetails.ToList()
                        join company in _dbContext.Company_master.ToList() on invoice.CompanyId equals company.Id
                        join user in _dbContext.Users.ToList() on invoice.ClientId equals user.Id
                        join createdby in _dbContext.Users.ToList() on invoice.CreatedBy equals createdby.Id
                        where invoice.CreatedDate >= fromDate && invoice.CreatedDate <= toDate
                        select new InvoiceEntry()
                        {
                            Amount = _dbContext.InvoiceParticulars.Where(_ => _.InvoiceId == invoice.Id).Sum(_ => (decimal?)_.Amount) ?? 0,
                            CreatedDateString = invoice.InvoiceDate.ToString("dd/MM/yyyy"),
                            UpdatedDate = invoice.UpdatedDate,
                            InvoiceNumber = company.Prefix + "-" + invoice.Invoice_Number,
                            InvoiceId = invoice.Id,
                            Address = user.Address + ", " + user.City,
                            City = user.City,
                            FullName = user.FullName,
                            CreatedBy = invoice.CreatedBy,
                            CompanyName = company.CompanyName,
                            CreatedByString = createdby.FullName,
                            Year = _dbContext.InvoiceParticulars.Where(_ => _.InvoiceId == invoice.Id)?.FirstOrDefault()?.year ?? string.Empty,
                            GroupName = (user.GroupId ?? 0) > 0 ? _dbContext.GroupMasters.FirstOrDefault(_ => _.GroupId == user.GroupId).Name : string.Empty,
                            AddedBy = Convert.ToInt32(createdby.UserTypeId)
                        }).ToList();

            else
                return (from invoice in _dbContext.InvoiceDetails.ToList()
                        join company in _dbContext.Company_master.ToList() on invoice.CompanyId equals company.Id
                        join user in _dbContext.Users.ToList() on invoice.ClientId equals user.Id
                        join createdby in _dbContext.Users.ToList() on invoice.CreatedBy equals createdby.Id
                        where invoice.CreatedDate >= fromDate && invoice.CreatedDate <= toDate
                        && invoice.CreatedBy == SessionHelper.UserId
                        select new InvoiceEntry()
                        {
                            Amount = _dbContext.InvoiceParticulars.Where(_ => _.InvoiceId == invoice.Id).Sum(_ => (decimal?)_.Amount) ?? 0,
                            CreatedDateString = invoice.InvoiceDate.ToString("dd/MM/yyyy"),
                            UpdatedDate = invoice.UpdatedDate,
                            InvoiceNumber = company.Prefix + "-" + invoice.Invoice_Number,
                            InvoiceId = invoice.Id,
                            Address = user.Address + ", " + user.City,
                            City = user.City,
                            FullName = user.FullName,
                            CreatedBy = invoice.CreatedBy,
                            CompanyName = company.CompanyName,
                            CreatedByString = createdby.FullName,
                            Year = _dbContext.InvoiceParticulars.Where(_ => _.InvoiceId == invoice.Id)?.FirstOrDefault()?.year ?? string.Empty,
                            GroupName = (user.GroupId ?? 0) > 0 ? _dbContext.GroupMasters.FirstOrDefault(_ => _.GroupId == user.GroupId).Name : string.Empty,
                            AddedBy = Convert.ToInt32(createdby.UserTypeId)
                        }).ToList();
        }

        public async Task<List<InvoiceEntry>> GetAllInvoiceParticulars(int invoiceId)
        {
            return (from invoiceParticular in _dbContext.InvoiceParticulars.ToList()
                    join particular in _dbContext.Particulars.ToList() on invoiceParticular.ParticularId equals particular.Id
                    join createdby in _dbContext.Users.ToList() on invoiceParticular.CreatedBy equals createdby.Id
                    where invoiceParticular.InvoiceId == invoiceId
                    select new InvoiceEntry()
                    {
                        Amount = invoiceParticular.Amount,
                        CreatedDateString = invoiceParticular.CreatedDate.ToString("dd/MM/yyyy"),
                        SFParticulars = particular.ParticularSF,
                        FFParticulars = particular.ParticularFF,
                        InvoiceParticularId = invoiceParticular.Id,
                        Year = invoiceParticular.year,
                        InvoiceId = invoiceParticular.InvoiceId,
                        CreatedBy = invoiceParticular.CreatedBy,
                        AddedBy = Convert.ToInt32(createdby.UserTypeId),
                        CreatedByString = createdby.FullName,
                        IsExclude = particular.IsExclude ?? false
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
                                                     CompanyId = invoice.CompanyId,
                                                     InvoiceDate = invoice.InvoiceDate
                                                 }).FirstOrDefault();

            documentViewDownload.Particulars = GetAllInvoiceParticulars(id).Result.Where(_ => _.IsExclude == false).ToList();
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

        public async Task<List<InvoiceEntry>> GetInvoiceByClientandCompany(int companyId, int clientId, int? receiptId)
        {
            var invoices = (from invoice in _dbContext.InvoiceDetails
                            where (invoice.ClientId == clientId && invoice.CompanyId == companyId)
                            select new InvoiceEntry()
                            {
                                CompanyId = invoice.CompanyId,
                                ClientId = invoice.ClientId,
                                InvoiceNumber = invoice.Invoice_Number,
                                InvoiceId = invoice.Id
                            }).Distinct().ToList();

            List<long?> invoiceIds = new List<long?>();

            if (receiptId.HasValue)
            {
                var convertedReceiptId = Convert.ToInt32(receiptId);
                invoiceIds = await _dbContext.InvoiceReciepts.Where(_ => _.RecieptId != convertedReceiptId).Select(_ => _.InvoiceId).ToListAsync();
            }
            else
                invoiceIds = await _dbContext.InvoiceReciepts.Select(_ => _.InvoiceId).ToListAsync();

            invoices.RemoveAll(_ => invoiceIds.Contains(_.InvoiceId));

            return invoices;
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
                        IsActive = company.IsActive ?? false,
                        Id = company.Id,
                        PreferedColor = company.PreferedColor,
                        Type = company.Type,
                        Prefix = company.Prefix
                    }).FirstOrDefault();
        }

        #endregion


        #region Private Method
        private string GenerateInvoiceNumber(int companyId, DateTime invoiceDate)
        {

            var yearStartAt = Convert.ToDateTime(ConfigurationManager.AppSettings["YearStart"].Replace("XXXX", (invoiceDate.Month >= 4 ? invoiceDate.Year : (invoiceDate.Year - 1)).ToString()));
            var yearEndedAt = Convert.ToDateTime(ConfigurationManager.AppSettings["YearEnd"].Replace("XXXX", (invoiceDate.Month > 3 ? (invoiceDate.Year + 1) : invoiceDate.Year).ToString()));

            var genericNumber = (from invoice in _dbContext.InvoiceDetails
                                 join particular in _dbContext.InvoiceParticulars on invoice.Id equals particular.InvoiceId
                                 where invoice.CompanyId == companyId && invoice.InvoiceDate >= yearStartAt && invoice.InvoiceDate <= yearEndedAt
                                 select invoice)
                                 .OrderByDescending(_ => _.Id)
                                 .Select(_ => _.Invoice_Number)
                                 .FirstOrDefault();
            if (!string.IsNullOrEmpty(genericNumber))
            {
                genericNumber = (Convert.ToInt32(genericNumber) + 1).ToString();
            }
            else
                genericNumber = "1";

            return (genericNumber);
        }

        public async Task<List<TrackingInformation>> GetTrackingInformation(int invoiceId)
        {
            return (from tracking in _dbContext.InvoiceTrackings.ToList()
                    join user in _dbContext.Users on (tracking.UserId ?? 0) equals user.Id
                    where tracking.InvoiceId == invoiceId
                    select new TrackingInformation()
                    {
                        TrackingId = tracking.id,
                        InvoiceId = tracking.InvoiceId,
                        Comment = tracking.Comment,
                        CreatedAt = tracking.CreatedAt.Value.ToString("dd/MM/yyyy"),
                        CreatedBy = tracking.CreatedBy,
                        AddedBy = user.FullName
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
                        InvoiceId = trackingInformation.InvoiceId,
                        CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId),
                        UserId = Convert.ToInt32(SessionHelper.UserTypeId)
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
                InvoiceId = trackingDetails.InvoiceId,
                CreatedBy = trackingDetails.CreatedBy
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

        public async Task<bool> DeleteInvoiceById(int invoiceId)
        {
            try
            {
                _dbContext.InvoiceDetails.Remove(await _dbContext.InvoiceDetails.Where(_ => _.Id == invoiceId).FirstOrDefaultAsync());
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<CompanyClientList>> GetAllCompanyWithClients()
        {
            try
            {
                return (from company in _dbContext.Company_master
                        join client in _dbContext.Company_Client_Mapping on company.Id equals (client.CompanyId ?? 0)
                        join user in _dbContext.Users on (client.UserId ?? 0) equals user.Id
                        select new CompanyClientList
                        {
                            CompanyName = company.CompanyName,
                            ClientName = user.Username
                        }).ToList();
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
