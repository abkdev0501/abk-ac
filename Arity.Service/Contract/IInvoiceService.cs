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
        Task<List<InvoiceEntry>> GetAllInvoice();
        Task<Data.Dto.InvoiceEntry> GetInvoice(int id);
        Task<List<InvoiceEntry>> GetAllInvoice(DateTime from, DateTime to);
        Task<List<InvoiceEntry>> GetAllInvoiceParticulars(int invoiceId);
        Task<List<InvoiceEntry>> GetInvoiceByClientandCompany(int companyId, int clientId);
        Task DeleteInvoiceParticularEntry(int id);
        Task<DocumentViewDownload> DownloadInvoice(int id);
        Task<InvoiceEntry> GetInvoiceSingle(int invoiceId);
        Task<decimal> GetInvoiceAmountTotal(List<long> invoices);
    }
}
