using Arity.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Infra.Interface
{
    public interface IAccountRepository
    {
        Task<List<UsersDto>> GetAllUsers();
    }
}
