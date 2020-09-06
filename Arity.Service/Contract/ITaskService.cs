using Arity.Data.Dto;
using Arity.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arity.Service.Contract
{
    public interface ITaskService
    {
        Task<IQueryable<TaskDTO>> GetAll();
        Task<TaskDTO> GetTask(int id);
        Task AddUpdateTask(TaskDTO task);
        Task<List<TaskDTO>> GetAll(int userId, int typeId);
        Task DeleteTask(int taskId);
    }
}
