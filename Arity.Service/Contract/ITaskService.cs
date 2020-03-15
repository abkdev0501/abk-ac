using Arity.Data.Dto;
using Arity.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arity.Service.Contract
{
    public interface ITaskService
    {
        Task<List<TaskDTO>> GetAll(DateTime from,DateTime to);
        Task<TaskDTO> GetTask(int id);
        Task AddUpdateTask(TaskDTO task);
        Task<List<TaskDTO>> GetAll(int userId);
    }
}
