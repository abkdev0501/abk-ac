using Arity.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service.Infrastructure
{
    public class DocumentViewDownload
    {
        public byte[] ByteArray { get; set; }
        public string ContentType { get; set; }
        public string DocumentName { get; set; }

        public InvoiceEntry InvoiceEntry { get; set; }
        public List<InvoiceEntry> Particulars { get; set; }
    }
}
