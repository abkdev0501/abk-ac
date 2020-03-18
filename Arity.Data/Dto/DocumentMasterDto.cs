using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Arity.Data.Helpers.EnumHelper;

namespace Arity.Data.Dto
{
   public class DocumentMasterDto 
    {
        public int DocumentId { get; set; }
        public string Name { get; set; }
        public int ClientId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentTypeName { get; set; }
        public DocumentStatus Status { get; set; }
        public string StatusName { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public HttpPostedFile  DocumentFile { get; set; }
        public string ClientName { get; set; }
        public string FileName { get; set; }
        public string ALreadyUploadedFile { get; set; }


    }
}
