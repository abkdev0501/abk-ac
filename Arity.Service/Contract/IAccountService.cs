using Arity.Data;
using Arity.Data.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service.Contract
{
   public interface IAccountService
    {
        Task<User> Login(string username, string password);
        Task<List<UsersDto>> GetAllUsers(DateTime from, DateTime to);
        Task<UsersDto> GetUser(int id);
        Task<IList<UserType>> GetAllUserType();
        Task AddUpadate(User user);
    }
}
