using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
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
            IInvoiceService invoiceService = new InvoiceService();
            long ReceiptId = 0;
            if (receiptEntry.ReceiptId > 0)
            {
                var exitingReceipt = await _dbContext.RecieptDetails.FirstOrDefaultAsync(_ => _.Id == receiptEntry.ReceiptId);
                exitingReceipt.Status = receiptEntry.Status;
                exitingReceipt.TotalAmount = await invoiceService.GetInvoiceAmountTotal(receiptEntry.InvoiceIds);
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
                    RecieptNo = GenerateNextRecieptNumber(Convert.ToInt32(receiptEntry.CompanyId)),
                    Status = receiptEntry.Status,
                    TotalAmount = await invoiceService.GetInvoiceAmountTotal(receiptEntry.InvoiceIds),
                    UpdatedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Discount = receiptEntry.Discount,
                    ChequeNumber = receiptEntry.ChequeNumber,
                    BankName = receiptEntry.BankName,
                    CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId),
                    RecieptDate = receiptEntry.RecieptDate
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



        public async Task<List<ReceiptDto>> GetAllReceipts(DateTime fromDate, DateTime toDate)
        {
            if (SessionHelper.UserTypeId == (int)Arity.Service.Core.UserType.User)
            {
                var receipts = (from receipt in _dbContext.RecieptDetails.ToList()
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
                                    CreatedDateString = receipt.CreatedDate.ToString("dd/MM/yyyy"),
                                    InvoiceNumbers = string.Join(",", _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id)).Select(i => i.Invoice_Number).ToList())
                                }).ToList();

                var clientIds = (from rec in _dbContext.InvoiceReciepts.ToList()
                                 join inv in _dbContext.InvoiceDetails on rec.InvoiceId equals inv.Id
                                 where receipts.Any(_ => _.ReceiptId == rec.RecieptId) && inv.ClientId == SessionHelper.UserId
                                 select new ReceiptDto
                                 {
                                     ReceiptId = rec.RecieptId ?? 0
                                 }).Distinct().ToList();

                if (clientIds.Count()==0)
                    return new List<ReceiptDto>();

                receipts.RemoveAll(_ => clientIds.Any(c => c.ReceiptId == _.ReceiptId));

                return receipts;

            }
            else
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
                            CreatedDateString = receipt.CreatedDate.ToString("dd/MM/yyyy"),
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
                               CompanyId = _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts
                               .Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                               .Select(i => i.CompanyId).FirstOrDefault(),
                               ClientId = _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts
                               .Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                               .Select(i => i.ClientId).FirstOrDefault(),
                               InvoiceIds = _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => (_.InvoiceId ?? 0)).ToList(),
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

        private string GenerateNextRecieptNumber(int companyId)
        {
            var genericNumber = _dbContext.RecieptDetails.OrderByDescending(_ => _.Id).Select(_ => _.RecieptNo).FirstOrDefault();
            var compName = _dbContext.Company_master.Where(_ => _.Id == companyId).Select(_ => _.CompanyName).FirstOrDefault();
            if (!string.IsNullOrEmpty(genericNumber) && genericNumber.Split('-').Count() > 1)
            {
                genericNumber = (Convert.ToInt32(genericNumber.Split('-')[1].Substring(1, (genericNumber.Split('-')[1].Length - 1))) + 1).ToString();
            }
            else
                genericNumber = "1";

            return (compName.ToUpper().Substring(0, 3) + "-R" + genericNumber);
        }

    }
}
