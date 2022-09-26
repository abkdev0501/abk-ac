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

        public async Task<TaskDTO> GetTask(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            var result = await _dbConnection.QueryAsync<TaskDTO>("GetTaskById", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task DeleteTask(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            await _dbConnection.QueryAsync("DeleteTaskById", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return;
        }

        public async Task<TaskResult> CreateTask(int currentUserId, int userTypeId, TaskDTO task)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserTypeId", userTypeId);
            parameters.Add("@CurrentUserId", currentUserId);
            parameters.Add("@TaskName", task.TaskName);
            parameters.Add("@Active", task.Active);
            parameters.Add("@StatusId", task.StatusId );
            parameters.Add("@Description", task.Description );
            parameters.Add("@Remarks", task.Remarks);
            parameters.Add("@UserComment", task.UserComment);
            parameters.Add("@UserId", task.UserId);
            parameters.Add("@IsChargeble", task.IsChargeble);
            parameters.Add("@ClientId", task.ClientId);
            parameters.Add("@Priorities", task.Priorities);
            parameters.Add("@CompletedOn", task.CompletedOn);
            parameters.Add("@DueDate", task.DueDate);
            parameters.Add("@ChargeAmount", task.ChargeAmount);
            var result = await _dbConnection.QueryAsync<TaskResult>("TaskCreate", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<TaskResult> UpdateTask(TaskDTO task)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", task.TaskId);
            parameters.Add("@TaskName", task.TaskName);
            parameters.Add("@Active", task.Active);
            parameters.Add("@StatusId", task.StatusId);
            parameters.Add("@Description", task.Description);
            parameters.Add("@Remarks", task.Remarks);
            parameters.Add("@UserComment", task.UserComment);
            parameters.Add("@UserId", task.UserId);
            parameters.Add("@IsChargeble", task.IsChargeble);
            parameters.Add("@ClientId", task.ClientId);
            parameters.Add("@Priorities", task.Priorities);
            parameters.Add("@CompletedOn", task.CompletedOn);
            parameters.Add("@DueDate", task.DueDate);
            parameters.Add("@ChargeAmount", task.ChargeAmount);
            var result = await _dbConnection.QueryAsync<TaskResult>("TaskUpdate", parameters, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return result.FirstOrDefault();
        }
    }
}
