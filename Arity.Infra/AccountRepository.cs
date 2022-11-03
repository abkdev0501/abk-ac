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
        private readonly DapperContext _context;
        public AccountRepository(DapperContext context)
        {
            _context = context;
        }
        public async Task<List<UsersDto>> GetAllUsers()
        {
            using (var dbConnection = _context.CreateConnection())
            {
                var parameters = new DynamicParameters();
                var result = await dbConnection.QueryAsync<UsersDto>("GetAllUsers", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
                return result.ToList();
            }
        }
    }
}
