using Arity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service.Contract
{
   public interface IAccountService
    {
        Task<User> Login(string username, string password);
    }
}
