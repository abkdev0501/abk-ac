using Arity.Data;
using Arity.Data.Dto;
using Arity.Service.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service.Contract
{
    public interface IInvoiceService
    {
        Task<List<Company_master>> GetCompany();
        Task<List<User>> GetClient(int companyId);
        Task<List<Particular>> GetParticular();
        Task AddUpdateInvoiceEntry(int CompanyId, InvoiceEntry invoiceEntry);
        Task<Data.Dto.InvoiceEntry> GetInvoice(int id);
        Task<List<InvoiceEntry>> GetAllInvoice(DateTime from, DateTime to);
        Task<List<InvoiceEntry>> GetAllInvoiceParticulars(int invoiceId);
        Task DeleteInvoiceParticularEntry(int id);
        Task<DocumentViewDownload> DownloadInvoice(int id);
        Task<InvoiceEntry> GetInvoiceSingle(int invoiceId);
        Task<List<TrackingInformation>> GetTrackingInformation(int invoiceId);
        Task<bool> AddTrackingInformation(TrackingInformation trackingInformation);
        Task<bool> RemoveInvoiceTracking(int trackingId);
        Task<TrackingInformation> GetTrackingInformationById(int invoiceTrackingId);
        Task<CompanyDto> GetCompanyDetailById(int companyId);
    }
}
