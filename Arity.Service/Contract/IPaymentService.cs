using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arity.Data.Dto;

namespace Arity.Service.Contract
{
   public interface IPaymentService
    {
        Task<List<ReceiptDto>> GetAllReceipts(DateTime from, DateTime to);
        Task<ReceiptDto> GetReceipt(int id);
        
        Task AddUpdateReceiptEntry(ReceiptDto receiptEntry);
    }
}
