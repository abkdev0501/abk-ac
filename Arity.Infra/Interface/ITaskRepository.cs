using Arity.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Infra.Interface
{
    public interface ITaskRepository
    {
        Task<List<TaskDTO>> GetAll(int userId, int typeId);

        Task<List<TaskDTO>> GetAllTaskDetail(long userId, long userTypeId, int recordFrom, int pageSize, string sortColumn, string sortOrder, Dictionary<string, object> filterParams);

        Task<TaskDTO> GetTask(int id);

        Task DeleteTask(int id);

        Task<TaskResult> CreateTask(int currentUserId, int userTypeId, TaskDTO task);

        Task<TaskResult> UpdateTask(TaskDTO task);
    }
}
