using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service.Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service
{
    public class PaymentService : IPaymentService
    {

        private readonly RMNEntities _dbContext;
        private readonly IInvoiceService _invoiceService;

        public PaymentService(IInvoiceService invoiceService,
            RMNEntities rmnEntities)
        {
            _dbContext = rmnEntities;
            _invoiceService = invoiceService;
        }

        public async Task AddUpdateReceiptEntry(ReceiptDto receiptEntry)
        {
            long ReceiptId = 0;
            if (receiptEntry.ReceiptId > 0)
            {
                var exitingReceipt = await _dbContext.RecieptDetails.FirstOrDefaultAsync(_ => _.Id == receiptEntry.ReceiptId);
                exitingReceipt.Status = receiptEntry.Status;
                exitingReceipt.TotalAmount = receiptEntry.InvoiceIds?.Count() == 0 ? receiptEntry.TotalAmount : await _invoiceService.GetInvoiceAmountTotal(receiptEntry.InvoiceIds);
                exitingReceipt.UpdatedDate = DateTime.Now;
                exitingReceipt.Discount = receiptEntry.Discount;
                exitingReceipt.ChequeNumber = receiptEntry.ChequeNumber;
                exitingReceipt.BankName = receiptEntry.BankName;
                exitingReceipt.Remarks = receiptEntry.Remarks;
                exitingReceipt.companyId = receiptEntry.CompanyId;
                exitingReceipt.clientId = receiptEntry.ClientId;

                _dbContext.InvoiceReciepts.RemoveRange(await _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == exitingReceipt.Id).ToListAsync());
                await _dbContext.SaveChangesAsync();
                ReceiptId = exitingReceipt.Id;
            }
            else
            {
                var receiptDetail = new RecieptDetail
                {
                    RecieptNo = GenerateNextRecieptNumber(Convert.ToInt32(receiptEntry.CompanyId), receiptEntry.RecieptDate, receiptEntry.InvoiceIds),
                    Status = receiptEntry.Status,
                    TotalAmount = (receiptEntry.InvoiceIds == null || receiptEntry.InvoiceIds?.Count() == 0) ? receiptEntry.TotalAmount : await _invoiceService.GetInvoiceAmountTotal(receiptEntry.InvoiceIds),
                    UpdatedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Discount = receiptEntry.Discount,
                    ChequeNumber = receiptEntry.ChequeNumber,
                    BankName = receiptEntry.BankName,
                    CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId),
                    AddedBy = Convert.ToInt32(SessionHelper.UserId),
                    RecieptDate = receiptEntry.RecieptDate,
                    Remarks = receiptEntry.Remarks,
                    companyId = receiptEntry.CompanyId,
                    clientId = receiptEntry.ClientId
                };
                _dbContext.RecieptDetails.Add(receiptDetail);
                await _dbContext.SaveChangesAsync();
                ReceiptId = receiptDetail.Id;
            }

            if (receiptEntry.InvoiceIds != null && receiptEntry.InvoiceIds.Any())
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

        public async Task DeleteReceipt(int receiptId)
        {
            _dbContext.InvoiceReciepts.RemoveRange(_dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receiptId));
            _dbContext.RecieptDetails.RemoveRange(_dbContext.RecieptDetails.Where(_ => _.Id == receiptId));
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ReceiptDto>> GetAllReceipts(DateTime fromDate, DateTime toDate)
        {
            var clients = await _dbContext.Users.ToListAsync();
            var companies = await _dbContext.Company_master.ToListAsync();
            var groups = await _dbContext.GroupMasters.ToListAsync();

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
                                    RecieptNo = (receipt.companyId.HasValue ? companies.FirstOrDefault(_ => _.Id == receipt.companyId)?.Prefix ?? string.Empty : companies.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails
                                                              .Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                              .Select(i => i.CompanyId)?.FirstOrDefault())?.Prefix ?? string.Empty) + "-" + receipt.RecieptNo,
                                    TotalAmount = receipt.TotalAmount,
                                    Status = receipt.Status ?? false,
                                    CreatedDateString = receipt.RecieptDate.ToString("dd/MM/yyyy"),
                                    CreatedBy = receipt.CreatedBy,
                                    InvoiceNumbers = string.Join(",", _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id)
                                                            .Select(_ => _.InvoiceId).ToList().Contains(i.Id)).Select(i => i.Invoice_Number).ToList()),
                                    CompanyName = receipt.companyId.HasValue ? companies.FirstOrDefault(_ => _.Id == receipt.companyId)?.CompanyName ?? string.Empty : companies.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails
                                                              .Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                              .Select(i => i.CompanyId)?.FirstOrDefault())?.CompanyName ?? string.Empty,
                                    ClientName = receipt.clientId.HasValue ? clients.FirstOrDefault(_ => _.Id == receipt.clientId)?.FullName ?? string.Empty : clients.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails
                                                            .Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                            .Select(i => i.ClientId)?.FirstOrDefault())?.FullName ?? string.Empty,
                                    GroupName = receipt.clientId.HasValue ? groups.FirstOrDefault(_ => _.GroupId == clients.FirstOrDefault(x => x.Id == receipt.clientId)?.GroupId)?.Name ?? string.Empty : groups.FirstOrDefault(g => g.GroupId == clients.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails.Where(i =>
                                                                _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                                .Select(i => i.ClientId)?.FirstOrDefault())?.GroupId)?.Name ?? string.Empty,
                                    AddedBy = clients.FirstOrDefault(_ => _.Id == receipt.AddedBy)?.FullName ?? string.Empty,
                                    Remarks = receipt.Remarks
                                }).ToList();

                var clientIds = (from rec in _dbContext.InvoiceReciepts.ToList()
                                 join inv in _dbContext.InvoiceDetails on rec.InvoiceId equals inv.Id
                                 where receipts.Any(_ => _.ReceiptId == rec.RecieptId) && inv.ClientId != SessionHelper.UserId
                                 select new ReceiptDto
                                 {
                                     ReceiptId = rec.RecieptId ?? 0
                                 }).Distinct().ToList();

                receipts.RemoveAll(_ => clientIds.Any(c => c.ReceiptId == _.ReceiptId));

                return receipts;

            }
            else if (SessionHelper.UserTypeId == (int)Arity.Service.Core.UserType.MasterAdmin)
                return (from receipt in _dbContext.RecieptDetails.ToList()
                        where receipt.CreatedDate >= fromDate && receipt.CreatedDate <= toDate
                        select new ReceiptDto()
                        {
                            ReceiptId = receipt.Id,
                            ChequeNumber = receipt.ChequeNumber,
                            BankName = receipt.BankName,
                            Discount = receipt.Discount,
                            RecieptNo = (receipt.companyId.HasValue ? companies.FirstOrDefault(_ => _.Id == receipt.companyId)?.Prefix ?? string.Empty : companies.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails
                                                              .Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                              .Select(i => i.CompanyId)?.FirstOrDefault())?.Prefix ?? string.Empty) + "-" + receipt.RecieptNo,
                            TotalAmount = receipt.TotalAmount,
                            Status = receipt.Status ?? false,
                            CreatedDateString = receipt.RecieptDate.ToString("dd/MM/yyyy"),
                            CreatedBy = receipt.CreatedBy,
                            InvoiceNumbers = string.Join(",", _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id)
                                                    .Select(_ => _.InvoiceId).ToList().Contains(i.Id)).Select(i => i.Invoice_Number).ToList()),
                            CompanyName = receipt.companyId.HasValue ? companies.FirstOrDefault(_ => _.Id == receipt.companyId)?.CompanyName ?? string.Empty : companies.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails
                                                      .Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                      .Select(i => i.CompanyId)?.FirstOrDefault())?.CompanyName ?? string.Empty,
                            ClientName = receipt.clientId.HasValue ? clients.FirstOrDefault(_ => _.Id == receipt.clientId)?.FullName ?? string.Empty : clients.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails
                                                    .Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                    .Select(i => i.ClientId)?.FirstOrDefault())?.FullName ?? string.Empty,
                            GroupName = receipt.clientId.HasValue ? groups.FirstOrDefault(_ => _.GroupId == clients.FirstOrDefault(x => x.Id == receipt.clientId)?.GroupId)?.Name ?? string.Empty : groups.FirstOrDefault(g => g.GroupId == clients.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails.Where(i =>
                                                        _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                        .Select(i => i.ClientId)?.FirstOrDefault())?.GroupId)?.Name ?? string.Empty,
                            AddedBy = clients.FirstOrDefault(_ => _.Id == receipt.AddedBy)?.FullName ?? string.Empty,
                            Remarks = receipt.Remarks
                        }).ToList();

            else
                return (from receipt in _dbContext.RecieptDetails.ToList()
                        where receipt.CreatedDate >= fromDate && receipt.CreatedDate <= toDate
                        && receipt.AddedBy == SessionHelper.UserId
                        select new ReceiptDto()
                        {
                            ReceiptId = receipt.Id,
                            ChequeNumber = receipt.ChequeNumber,
                            BankName = receipt.BankName,
                            Discount = receipt.Discount,
                            RecieptNo = (receipt.companyId.HasValue ? companies.FirstOrDefault(_ => _.Id == receipt.companyId)?.Prefix ?? string.Empty : companies.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails
                                                              .Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                              .Select(i => i.CompanyId)?.FirstOrDefault())?.Prefix ?? string.Empty) + "-" + receipt.RecieptNo,
                            TotalAmount = receipt.TotalAmount,
                            Status = receipt.Status ?? false,
                            CreatedDateString = receipt.RecieptDate.ToString("dd/MM/yyyy"),
                            CreatedBy = receipt.CreatedBy,
                            InvoiceNumbers = string.Join(",", _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id)
                                                    .Select(_ => _.InvoiceId).ToList().Contains(i.Id)).Select(i => i.Invoice_Number).ToList()),
                            CompanyName = receipt.companyId.HasValue ? companies.FirstOrDefault(_ => _.Id == receipt.companyId)?.CompanyName ?? string.Empty : companies.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails
                                                      .Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                      .Select(i => i.CompanyId)?.FirstOrDefault())?.CompanyName ?? string.Empty,
                            ClientName = receipt.clientId.HasValue ? clients.FirstOrDefault(_ => _.Id == receipt.clientId)?.FullName ?? string.Empty : clients.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails
                                                    .Where(i => _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                    .Select(i => i.ClientId)?.FirstOrDefault())?.FullName ?? string.Empty,
                            GroupName = receipt.clientId.HasValue ? groups.FirstOrDefault(_ => _.GroupId == clients.FirstOrDefault(x => x.Id == receipt.clientId)?.GroupId)?.Name ?? string.Empty : groups.FirstOrDefault(g => g.GroupId == clients.FirstOrDefault(x => x.Id == _dbContext.InvoiceDetails.Where(i =>
                                                        _dbContext.InvoiceReciepts.Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                                        .Select(i => i.ClientId)?.FirstOrDefault())?.GroupId)?.Name ?? string.Empty,
                            AddedBy = clients.FirstOrDefault(_ => _.Id == receipt.AddedBy)?.FullName ?? string.Empty,
                            Remarks = receipt.Remarks
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
                               RecieptDate = receipt.RecieptDate,
                               Remarks = receipt.Remarks,
                               CompanyId = receipt.companyId.HasValue ? receipt.companyId.Value : _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts
                                           .Where(_ => _.RecieptId == receipt.Id).Select(_ => _.InvoiceId).ToList().Contains(i.Id))
                                           .Select(i => i.CompanyId).FirstOrDefault(),
                               ClientId = receipt.clientId.HasValue ? receipt.clientId.Value : _dbContext.InvoiceDetails.Where(i => _dbContext.InvoiceReciepts
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
                                           }).ToList(),
                               ClientName = receipt.clientId.HasValue ? _dbContext.Users.FirstOrDefault(_ => _.Id == receipt.clientId.Value).FullName : string.Empty
                           }).FirstOrDefault();
            if (reciept != null)
            {
                long invoiceId = reciept.Invoices.FirstOrDefault()?.InvoiceId ?? 0;
                reciept.Year = _dbContext.InvoiceParticulars.FirstOrDefault(pm => invoiceId == pm.InvoiceId)?.year ?? string.Empty;
            }
            return reciept;
        }

        /// <summary>
        /// Generate generic receipt number based on receipt date
        /// It will adjust receipt number in between dates and current year
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="recieptDate"></param>
        /// <param name="invoiceIds"></param>
        /// <returns></returns>
        private string GenerateNextRecieptNumber(int companyId, DateTime recieptDate, List<long> invoiceIds)
        {
            string genericNumber = "";
            int recieptNum = 0;
            var yearStartAt = Convert.ToDateTime(ConfigurationManager.AppSettings["YearStart"].Replace("XXXX", (recieptDate.Month >= 4 ? recieptDate.Year : (recieptDate.Year - 1)).ToString()));
            var yearEndedAt = Convert.ToDateTime(ConfigurationManager.AppSettings["YearEnd"].Replace("XXXX", (recieptDate.Month > 3 ? (recieptDate.Year + 1) : recieptDate.Year).ToString()));
            var reciepts = new List<InvoiceReciept>();

            //var invoiceIdslst = new List<InvoiceDetail>();

            //invoiceIdslst = _dbContext.InvoiceDetails.Where(_ => _.CompanyId == companyId).ToList();

            //if (invoiceIds?.Count() == 0)
            reciepts = _dbContext.RecieptDetails.Where(_ => _.companyId == companyId).ToList().Select(_ => new InvoiceReciept
            {
                RecieptId = _.Id
            }).ToList();
            //else
            //    reciepts = (from invoiceRec in _dbContext.InvoiceReciepts.ToList()
            //                join ids in invoiceIdslst on invoiceRec.InvoiceId equals ids.Id
            //                select invoiceRec)
            //                         .Distinct()
            //                         .ToList();

            var allReceipt = (from reciept in _dbContext.RecieptDetails.ToList()
                              join ids in reciepts on reciept.Id equals ids.RecieptId
                              where reciept.RecieptDate >= yearStartAt
                              && reciept.RecieptDate <= yearEndedAt
                              && reciept.RecieptDate >= recieptDate
                              select reciept)
                                 .OrderBy(_ => _.RecieptDate)
                                 .Distinct()
                                 .ToList();
            if (allReceipt != null && (allReceipt?.Any() ?? false))
            {
                var sameDateReceipt = allReceipt.Where(_ => _.RecieptDate == recieptDate).ToList();
                if (sameDateReceipt != null && sameDateReceipt.Any())
                {
                    genericNumber = sameDateReceipt.OrderByDescending(_ => _.Id).Select(_ => _.RecieptNo).FirstOrDefault();
                    genericNumber = (Convert.ToInt32(genericNumber) + 1).ToString();
                }
                else
                    genericNumber = allReceipt.OrderBy(_ => _.RecieptDate).Select(_ => _.RecieptNo).FirstOrDefault();
                genericNumber = (Convert.ToInt32(genericNumber) - 1).ToString();
                recieptNum = Convert.ToInt32(genericNumber) + 1;
                allReceipt.Where(_ => _.RecieptDate > recieptDate).ToList().ForEach(_ =>
                {
                    recieptNum++;
                    _.RecieptNo = recieptNum.ToString();
                });
            }
            else
            {
                genericNumber = (from reciept in _dbContext.RecieptDetails.ToList()
                                 join ids in reciepts on reciept.Id equals ids.RecieptId
                                 where reciept.RecieptDate >= yearStartAt
                                 && reciept.RecieptDate <= yearEndedAt
                                 select reciept)
                                 .OrderByDescending(_ => _.RecieptDate)
                                 .Distinct()
                                 .Select(_ => _.RecieptNo)
                                 .FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(genericNumber))
            {
                genericNumber = (Convert.ToInt32(genericNumber) + 1).ToString();
            }
            else
                genericNumber = "1";

            return (genericNumber);
        }
    }
}
