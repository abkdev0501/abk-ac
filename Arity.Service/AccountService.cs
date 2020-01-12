using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arity.Data;
using Arity.Service.Contract;

namespace Arity.Service
{
    public class AccountService : IAccountService
    {
        private readonly RMNEntities _dbContext;
        public AccountService()
        {
            _dbContext = new RMNEntities();
        }

        public async Task<User> Login(string username, string password)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(_ => _.Username == username && _.Password == password);
        }
    }
}

