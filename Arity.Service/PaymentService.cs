using Arity.Data;
using Arity.Data.Dto;
using Arity.Service.Contract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service
{
    public class PaymentService : IPaymentService
    {

        private readonly RMNEntities _dbContext;
        public PaymentService()
        {
            _dbContext = new RMNEntities();
        }

        public async Task AddUpdateReceiptEntry(ReceiptDto receiptEntry)
        {
            long ReceiptId = 0;
            if (receiptEntry.ReceiptId > 0)
            {
                var exitingReceipt = await _dbContext.RecieptDetails.FirstOrDefaultAsync(_ => _.Id == receiptEntry.ReceiptId);
                exitingReceipt.Status = receiptEntry.Status;
                exitingReceipt.TotalAmount = receiptEntry.TotalAmount;
                exitingReceipt.UpdatedDate = DateTime.Now;
                exitingReceipt.Discount = receiptEntry.Discount;
                exitingReceipt.ChequeNumber = receiptEntry.ChequeNumber;
                exitingReceipt.BankName = receiptEntry.BankName;

                _dbContext.InvoiceReciepts.RemoveRange(await _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == exitingReceipt.Id).ToListAsync());
                await _dbContext.SaveChangesAsync();
                ReceiptId = exitingReceipt.Id;
            }
            else
            {
                var receiptDetail = new RecieptDetail
                {
                    RecieptNo = "R1",
                    Status = receiptEntry.Status,
                    TotalAmount = receiptEntry.TotalAmount,
                    UpdatedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Discount = receiptEntry.Discount,
                    ChequeNumber = receiptEntry.ChequeNumber,
                    BankName = receiptEntry.BankName
                };
                _dbContext.RecieptDetails.Add(receiptDetail);
                await _dbContext.SaveChangesAsync();
                ReceiptId = receiptDetail.Id;
            }

            if (receiptEntry.InvoiceIds.Any())
            {
                foreach (var invoiceId in receiptEntry.InvoiceIds)
                {
                    _dbContext.InvoiceReciepts.Add(new InvoiceReciept
                    {
                        RecieptId = ReceiptId,
                        InvoiceId = invoiceId
                    });
                }
            }
            await _dbContext.SaveChangesAsync();
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

        public async Task<List<ReceiptDto>> GetAllReceipts(DateTime fromDate, DateTime toDate)
        {
            return (from receipt in _dbContext.RecieptDetails.ToList()
                    where receipt.CreatedDate >= fromDate && receipt.CreatedDate <= toDate
                    select new ReceiptDto()
                    {
                        ReceiptId = receipt.Id,
                        ChequeNumber = receipt.ChequeNumber,
                        BankName = receipt.BankName,
                        Discount = receipt.Discount,
                        RecieptNo = receipt.RecieptNo,
                        TotalAmount = receipt.TotalAmount,
                        Status = receipt.Status ?? false,
                        CreatedDateString = receipt.CreatedDate.ToString("MM/dd/yyyy"),
                        InvoiceNumbers = string.Join(",", _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id)).Select(i => i.Invoice_Number).ToList())
                    }).ToList();
        }

        public async Task<ReceiptDto> GetReceipt(int id)
        {
            var reciept = (from receipt in _dbContext.RecieptDetails.ToList()
                           where receipt.Id == id
                           select new ReceiptDto()
                           {
                               ReceiptId = receipt.Id,
                               ChequeNumber = receipt.ChequeNumber,
                               BankName = receipt.BankName,
                               Discount = receipt.Discount,
                               RecieptNo = receipt.RecieptNo,
                               TotalAmount = receipt.TotalAmount,
                               Status = receipt.Status ?? false,
                               CreatedDate = receipt.CreatedDate,
                               InvoiceIds = _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList(),
                               Invoices = _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts
                               .Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                               .Select(i => new InvoiceEntry
                               {
                                   InvoiceNumber = i.Invoice_Number,
                                   ClientId = i.ClientId,
                                   InvoiceId = i.Id,
                                   CompanyId = i.CompanyId,
                                   FullName = _dbContext.Users.FirstOrDefault(_ => _.Id == i.ClientId).FullName
                               }).ToList()
                           }).FirstOrDefault();
            if (reciept != null)
            {
                long invoiceId = reciept.Invoices.FirstOrDefault().InvoiceId;
                reciept.Year = _dbContext.InvoiceParticulars.FirstOrDefault(pm => invoiceId == pm.InvoiceId).year;
            }
            return reciept;
        }
    }
}
