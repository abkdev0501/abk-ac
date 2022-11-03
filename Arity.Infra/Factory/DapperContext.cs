using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Arity.Infra.Factory
{
    public class DapperContext
    {
        private readonly string _connectionString;
        public DapperContext()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["RMNEntities"].ConnectionString;
        }
        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
