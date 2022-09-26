using Arity.Data.Dto;
using Arity.Infra.Factory;
using Arity.Infra.Interface;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Arity.Infra
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IDbConnection _dbConnection;
        public AccountRepository(IConnectionFactory connectionFactory)
        {
            _dbConnection = connectionFactory.GetConnection;
        }
        public async Task<List<UsersDto>> GetAllUsers()
        {
            var parameters = new DynamicParameters();
            var result = await _dbConnection.QueryAsync<UsersDto>("GetAllUsers", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.ToList();
        }
    }
}
