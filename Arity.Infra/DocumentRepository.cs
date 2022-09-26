using Arity.Data.Dto;
using Arity.Infra.Factory;
using Arity.Infra.Interface;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Infra
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IDbConnection _dbConnection;
        public DocumentRepository(IConnectionFactory connectionFactory)
        {
            _dbConnection = connectionFactory.GetConnection;
        }

        public async Task<List<DocumentMasterDto>> GetDocumentList(int userId, int userTypeId, int recordFrom, int pageSize, string sortColumn, string sortOrder, Dictionary<string, object> filterParams)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@UserTypeId ", userTypeId);
            parameters.Add("@RecordFrom", recordFrom);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SortOrder", sortOrder);
            parameters.Add("@SortColumn", sortColumn);

            foreach (var filterParam in filterParams)
                parameters.Add($"@{filterParam.Key}", filterParam.Value);

            var result = await _dbConnection.QueryAsync<DocumentMasterDto>("DocumentList", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.ToList();
        }
    }
}
