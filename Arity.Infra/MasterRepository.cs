using Arity.Data.Dto;
using Arity.Infra.Factory;
using Arity.Infra.Interface;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Arity.Infra
{
    public class MasterRepository : IMasterRepository
    {
        private readonly IDbConnection _dbConnection;
        public MasterRepository(IConnectionFactory connectionFactory)
        {
            _dbConnection = connectionFactory.GetConnection;
        }
        public async Task<List<NotificationDTO>> GetAllNotification(int userId, int userType, int type)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@UserType", userType);
            parameters.Add("@Type", type);
            var result = await _dbConnection.QueryAsync<NotificationDTO>("GetAllNotification", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.ToList();
        }

        public async Task<List<NotificationDTO>> GetAllNotificationList()
        {
            var parameters = new DynamicParameters();
            var result = await _dbConnection.QueryAsync<NotificationDTO>("GetAllNotificationList", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.ToList();
        }

        public async Task<NotificationDTO> GetNotification(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            var result = await _dbConnection.QueryAsync<NotificationDTO>("GetAllNotificationList", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.FirstOrDefault();
        }
    }
}
