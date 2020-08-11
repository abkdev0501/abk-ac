using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service.Contract;

namespace Arity.Service
{
    public class AccountService : IAccountService
    {
        private readonly RMNEntities _dbContext;

        public AccountService(RMNEntities rMNEntities)
        {
            _dbContext = rMNEntities;
        }

        public async Task AddUpadate(UsersDto user)
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
                existingUser.Email = user.Email;
                existingUser.Active = user.Active;
                existingUser.Password = Functions.Encrypt(user.Password);

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

                if (user.CompanyIds?.Count() > 0)
                {
                    _dbContext.Company_Client_Mapping.RemoveRange(_dbContext.Company_Client_Mapping.Where(_ => _.UserId == user.Id));
                    await _dbContext.SaveChangesAsync();

                    foreach (var companyid in user.CompanyIds)
                        _dbContext.Company_Client_Mapping.Add(new Company_Client_Mapping
                        {
                            UserId = Convert.ToInt32(existingUser.Id),
                            CompanyId = companyid
                        });
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
                userModel.Password = Functions.Encrypt(user.Password);
                userModel.CreatedDate = DateTime.Now;
                userModel.UserTypeId = user.UserTypeId;
                userModel.Email = user.Email;
                userModel.Active = user.Active;
                userModel.CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId);
                _dbContext.Users.Add(userModel);
                await _dbContext.SaveChangesAsync();

                _dbContext.User_Role.Add(new User_Role
                {
                    CreatedDate = DateTime.Now,
                    RoleId = user.UserTypeId,
                    UserId = userModel.Id,
                    UpdatedDate = DateTime.Now
                });

                if (user.CompanyIds?.Count() > 0)
                {
                    foreach (var companyid in user.CompanyIds)
                        _dbContext.Company_Client_Mapping.Add(new Company_Client_Mapping
                        {
                            UserId = Convert.ToInt32(userModel.Id),
                            CompanyId = companyid
                        });
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<UsersDto>> GetAllUsersList()
        {
            return (from user in _dbContext.Users
                    join type in _dbContext.UserTypes on user.UserTypeId equals type.Id
                    where user.UserTypeId != (int)Core.UserType.User
                    select new UsersDto
                    {
                        Id = user.Id,
                        Address = user.Address,
                        City = user.City,
                        Pincode = user.Pincode,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Username = user.Username,
                        UserType = type.UserTypeName,
                        Email = user.Email,
                        Active = user.Active,
                        UserTypeId = user.UserTypeId,
                        CreatedBy = user.CreatedBy,
                        AccountantName = user.AccountantName
                    }).ToList();
        }

        public async Task<List<UsersDto>> GetAllUsers()
        {
            return await (from user in _dbContext.Users
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
                        UserType = type.UserTypeName,
                        Email = user.Email,
                        Active = user.Active,
                        UserTypeId = user.UserTypeId,
                        CreatedBy = user.CreatedBy
                    }).ToListAsync();
        }

        public async Task RemoveUser(int userId)
        {
            _dbContext.Users.Remove(await _dbContext.Users.FirstOrDefaultAsync(_ => _.Id == userId));
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<UserType>> GetAllUserType()
        {
            return await _dbContext.UserTypes.ToListAsync();
        }

        public async Task<UsersDto> GetUser(int id)
        {
            var userDetail = _dbContext.Users.Where(_ => _.Id == id).ToList()
                    .Select(user => new UsersDto
                    {
                        Id = user.Id,
                        Address = user.Address,
                        City = user.City,
                        CreatedDate = user.CreatedDate,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Pincode = user.Pincode,
                        Username = user.Username,
                        UserTypeId = user.UserTypeId,
                        Email = user.Email,
                        Active = user.Active,
                        CreatedBy = user.CreatedBy,
                        Password = user.Password,
                        AccountantMobile = user.AccountantMobile,
                        AccountantName = user.AccountantName,
                        CommodityName = user.CommodityName,
                        CompanyIds = _dbContext.Company_Client_Mapping.Where(_ => _.UserId == user.Id)?.Select(_ => _.CompanyId ?? 0)?.ToArray()
                    }).FirstOrDefault();
            if (id > 0)
                userDetail.Password = Functions.Decrypt(userDetail.Password);
            return userDetail;
        }

        public async Task<User> Login(string username, string password)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(_ => _.Username == username && _.Password == password && _.Active == true);
        }
    }
}

