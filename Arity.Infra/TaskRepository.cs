﻿using Arity.Data.Dto;
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
    public class TaskRepository : ITaskRepository
    {
        private readonly IDbConnection _dbConnection;
        public TaskRepository(IConnectionFactory connectionFactory)
        {
            _dbConnection = connectionFactory.GetConnection;
        }

        public async Task<List<TaskDTO>> GetAll(int userId, int typeId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@TypeId", typeId);
            var result = await _dbConnection.QueryAsync<TaskDTO>("GetAllTask", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.ToList();
        }

        public async Task<List<TaskDTO>> GetAllTaskDetail(long userId, long userTypeId, int recordFrom, int pageSize, string sortColumn, string sortOrder, Dictionary<string, object> filterParams)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserTypeId", userTypeId);
            parameters.Add("@UserId", userId);
            parameters.Add("@RecordFrom", recordFrom);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SortOrder", sortOrder);
            parameters.Add("@SortColumn", sortColumn);

            foreach (var filterParam in filterParams)
                parameters.Add($"@{filterParam.Key}", filterParam.Value);

            var result = await _dbConnection.QueryAsync<TaskDTO>("GetAllTaskDetail", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.ToList();
        }
    }
}