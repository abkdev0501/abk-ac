using Arity.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Infra.Interface
{
    public interface IDocumentRepository
    {
        Task<List<DocumentMasterDto>> GetDocumentList(int userId, int userTypeId, int recordFrom, int pageSize, string sortColumn, string sortOrder, Dictionary<string, object> filterParams);
    }
}
