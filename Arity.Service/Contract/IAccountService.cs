using Arity.Data.Dto;
using Arity.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arity.Service.Contract
{
    public interface IAccountService
    {
        Task<User> Login(string username, string password);
        Task<List<UsersDto>> GetAllUsers(DateTime from, DateTime to);
        Task<List<UsersDto>> GetAllUsers();
        Task<UsersDto> GetUser(int id);
        Task<IList<UserType>> GetAllUserType();
        Task AddUpadate(UsersDto user);
        Task RemoveUser(int userId);
    }
}
