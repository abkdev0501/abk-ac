using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Helpers;
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

        public async Task AddUpadate(User user)
        {
            if (user.Id > 0)
            {
                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(_ => _.Id == user.Id);
                existingUser.FullName = user.FullName;
                existingUser.Address = user.Address;
                existingUser.City = user.City;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Pincode = user.Pincode;
                existingUser.UpdatedDated = DateTime.Now;
                existingUser.UserTypeId = user.UserTypeId;

                if (existingUser.Username != user.Username)
                {
                    var isSameUserExist = await _dbContext.Users.FirstOrDefaultAsync(_ => _.Username == user.Username);
                    if (isSameUserExist != null)
                        throw new Exception();
                }

                if (existingUser.UserTypeId != user.UserTypeId)
                {
                    _dbContext.User_Role.RemoveRange(_dbContext.User_Role.Where(_ => _.UserId == user.Id));
                    await _dbContext.SaveChangesAsync();

                    _dbContext.User_Role.Add(new User_Role
                    {
                        CreatedDate = DateTime.Now,
                        RoleId = user.UserTypeId,
                        UserId = user.Id,
                        UpdatedDate = DateTime.Now
                    });
                    await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                var isSameUserExist = await _dbContext.Users.FirstOrDefaultAsync(_ => _.Username == user.Username);
                if (isSameUserExist != null)
                    throw new Exception();

                var userModel = new User();
                userModel.FullName = user.FullName;
                userModel.Address = user.Address;
                userModel.City = user.City;
                userModel.PhoneNumber = user.PhoneNumber;
                userModel.Pincode = user.Pincode;
                userModel.UpdatedDated = DateTime.Now;
                userModel.Username = user.Username;
                userModel.Password = Functions.Encrypt_QueryString(user.Password);
                userModel.CreatedDate = DateTime.Now;
                userModel.UserTypeId = user.UserTypeId;
                _dbContext.Users.Add(userModel);
                await _dbContext.SaveChangesAsync();

                _dbContext.User_Role.Add(new User_Role
                {
                    CreatedDate = DateTime.Now,
                    RoleId = user.UserTypeId,
                    UserId = userModel.Id,
                    UpdatedDate = DateTime.Now
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<UsersDto>> GetAllUsers(DateTime from, DateTime to)
        {
            return (from user in _dbContext.Users
                    join type in _dbContext.UserTypes on user.UserTypeId equals type.Id
                    select new UsersDto
                    {
                        Id = user.Id,
                        Address = user.Address,
                        City = user.City,
                        Pincode = user.Pincode,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Username = user.Username,
                        UserType = type.UserTypeName
                    }).ToList();
        }

        public async Task<IList<UserType>> GetAllUserType()
        {
            return await _dbContext.UserTypes.ToListAsync();
        }

        public async Task<UsersDto> GetUser(int id)
        {
            return (from user in _dbContext.Users
                    where user.Id == id
                    select new UsersDto
                    {
                        Id = user.Id,
                        Address = user.Address,
                        City = user.City,
                        CreatedDate = user.CreatedDate,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Pincode = user.Pincode,
                        Username = user.Username,
                        UserTypeId = user.UserTypeId
                    }).FirstOrDefault();
        }

        public async Task<User> Login(string username, string password)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(_ => _.Username == username && _.Password == password);
        }
    }
}

