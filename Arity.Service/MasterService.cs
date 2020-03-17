using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service.Contract;

namespace Arity.Service
{
    public class MasterService : IMasterService
    {
        private readonly RMNEntities _dbContext;
        public MasterService()
        {
            _dbContext = new RMNEntities();
        }

        #region Company
        /// <summary>
        /// Get all company
        /// </summary>
        /// <returns></returns>
        public async Task<List<CompanyDto>> GetAllCompany()
        {
            return await (from company in _dbContext.Company_master
                          select new CompanyDto
                          {
                              CompanyName = company.CompanyName,
                              Address = company.Address,
                              Id = company.Id,
                              Prefix = company.Prefix,
                              Type = company.Type,
                              IsActive = company.IsActive ?? false
                          }).ToListAsync();
        }

        /// <summary>
        /// Get company by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CompanyDto> GetCompanyById(int id)
        {
            return await (from company in _dbContext.Company_master
                          where company.Id == id
                          select new CompanyDto
                          {
                              CompanyName = company.CompanyName,
                              Address = company.Address,
                              Id = company.Id,
                              Prefix = company.Prefix,
                              Type = company.Type,
                              IsActive = company.IsActive ?? false,
                              PreferedColor = company.PreferedColor
                          }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Add update company
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task AddUpdateCompany(CompanyDto data)
        {

            if (data.Id > 0)
            {
                var companyDetails = await _dbContext.Company_master.FirstOrDefaultAsync(_ => _.Id == data.Id);
                companyDetails.IsActive = data.IsActive;
                companyDetails.CompanyName = data.CompanyName;
                companyDetails.Address = data.Address;
                companyDetails.PreferedColor = data.PreferedColor;
                companyDetails.Prefix = data.Prefix;
                companyDetails.Type = data.Type;
            }
            else
                _dbContext.Company_master.Add(new Data.Entity.Company_master
                {
                    IsActive = data.IsActive,
                    CompanyName = data.CompanyName,
                    Address = data.Address,
                    PreferedColor = data.PreferedColor,
                    Prefix = data.Prefix,
                    Type = data.Type
                });
            await _dbContext.SaveChangesAsync();
        }


        #endregion

        #region Group Master
        public async Task<List<GroupMasterDTO>> GetAllGroup()
        {
            return await (from gm in _dbContext.GroupMasters
                          join user in _dbContext.Users on gm.AddedBy equals user.Id
                          select new GroupMasterDTO
                          {
                              GroupId = gm.GroupId,
                              Name = gm.Name,
                              Description = gm.Description,
                              AddedByName = user.FullName
                          }).ToListAsync();
        }

        /// <summary>
        /// Get group details by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GroupMasterDTO> GetGroupById(int id)
        {
            return await (from gm in _dbContext.GroupMasters
                          where gm.GroupId == id
                          select new GroupMasterDTO
                          {
                              GroupId = gm.GroupId,
                              Name = gm.Name,
                              Description = gm.Description,
                              AddedBy = gm.AddedBy,
                              CreatedBy = gm.CreatedBy,
                              CreatedOn = gm.CreatedOn
                          }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Add/update group master
        /// </summary>
        /// <param name="groupMaster"></param>
        /// <returns></returns>
        public async Task AddUpdateGroup(GroupMasterDTO groupMaster)
        {
            if (groupMaster.GroupId > 0)
            {
                var groupDetails = await _dbContext.GroupMasters.FirstOrDefaultAsync(_ => _.GroupId == groupMaster.GroupId);
                groupDetails.Name = groupMaster.Name;
                groupDetails.Description = groupMaster.Description;
                groupDetails.ModifiedBy = Convert.ToInt32(SessionHelper.UserId);
                groupDetails.ModifiedOn = DateTime.Now;
            }
            else
                _dbContext.GroupMasters.Add(new Data.Entity.GroupMaster
                {
                    Name = groupMaster.Name,
                    Description = groupMaster.Description,
                    AddedBy = Convert.ToInt32(SessionHelper.UserId),
                    CreatedOn = DateTime.Now,
                    CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId)
                });
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        /// <summary>
        /// Get all clients
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task<List<UsersDto>> GetAllClient(DateTime fromDate, DateTime toDate)
        {
            return (from user in _dbContext.Users
                    join type in _dbContext.UserTypes on user.UserTypeId equals type.Id
                    where user.CreatedDate >= fromDate && user.CreatedDate <= toDate && user.UserTypeId == (int)Arity.Service.Core.UserType.User
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
                    }).ToList();
        }

        /// <summary>
        /// Get client by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UsersDto> GetClientById(int id)
        {
            return (from client in _dbContext.Users.ToList()
                    where client.Id == id
                    select new UsersDto
                    {
                        FullName = client.FullName,
                        Id = client.Id,
                        AccountantMobile = client.AccountantMobile,
                        AccountantName = client.AccountantName,
                        Active = client.Active,
                        AddedBy = client.AddedBy,
                        Address = client.Address,
                        ApplicableRate = client.ApplicableRate,
                        BusinessStatus = client.BusinessStatus,
                        BusinessType = client.BusinessType,
                        City = client.City,
                        CommodityHSN = client.CommodityHSN,
                        CommodityName = client.CommodityName,
                        ConsultantId = client.ConsultantId,
                        ContactPerson = client.ContactPerson,
                        CreatedBy = client.CreatedBy,
                        CreatedDate = client.CreatedDate,
                        EFFDate = client.EFFDate,
                        Email = client.Email,
                        GroupId = client.GroupId,
                        GSTIN = client.GSTIN,
                        GSTRate = client.GSTRate,
                        JURISDICTION = client.JURISDICTION,
                        MasterMobile = client.MasterMobile,
                        PanNumber = client.PanNumber,
                        Password = Functions.Decrypt(client.Password),
                        PhoneNumber = client.PhoneNumber,
                        Pincode = client.Pincode,
                        Remarks = client.Remarks,
                        Rates = client.Rates,
                        RTNType = client.RTNType,
                        ServiceTypes = client.ServiceTypes,
                        TANNumber = client.TANNumber,
                        Username = client.Username,
                        UserTypeId = client.UserTypeId,
                        CompanyIds = _dbContext.Company_Client_Mapping.Where(_ => _.UserId == client.Id)?.Select(_ => _.CompanyId ?? 0)?.ToArray()
                    }).FirstOrDefault();
        }

        /// <summary>
        /// Get all consultants
        /// </summary>
        /// <returns></returns>
        public async Task<List<Consultants>> GetAllConsultant()
        {
            return await _dbContext.Consultant.ToListAsync();
        }

        /// <summary>
        /// Get all groups
        /// </summary>
        /// <returns></returns>
        public async Task<List<GroupMasterDTO>> GetAllGroups()
        {
            return await (from gm in _dbContext.GroupMasters
                          select new GroupMasterDTO
                          {
                              GroupId = gm.GroupId,
                              Name = gm.Name
                          }).ToListAsync();
        }

        public async Task AddUpdateClient(UsersDto usersDto)
        {
            if (usersDto.Id > 0)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(_ => _.Id == usersDto.Id);
                user.FullName = usersDto.FullName;
                user.AccountantMobile = usersDto.AccountantMobile;
                user.JURISDICTION = usersDto.JURISDICTION;
                user.MasterMobile = usersDto.MasterMobile;
                user.PanNumber = usersDto.PanNumber;
                user.Password = Functions.Encrypt(usersDto.Password);
                user.PhoneNumber = usersDto.PhoneNumber;
                user.Pincode = usersDto.Pincode;
                user.Rates = usersDto.Rates;
                user.Remarks = usersDto.Remarks;
                user.AccountantName = usersDto.AccountantName;
                user.Active = usersDto.Active;
                user.Address = usersDto.Address;
                user.ApplicableRate = usersDto.ApplicableRate;
                user.BusinessStatus = usersDto.BusinessStatus;
                user.BusinessType = usersDto.BusinessType;
                user.City = usersDto.City;
                user.CommodityHSN = usersDto.CommodityHSN;
                user.CommodityName = usersDto.CommodityName;
                user.ConsultantId = usersDto.ConsultantId;
                user.ContactPerson = usersDto.ContactPerson;
                user.EFFDate = usersDto.EFFDate;
                user.Email = usersDto.Email;
                user.GroupId = usersDto.GroupId;
                user.GSTIN = usersDto.GSTIN;
                user.GSTRate = usersDto.GSTRate;
                user.RTNType = usersDto.RTNType;
                user.ServiceTypes = usersDto.ServiceTypes;
                user.TANNumber = usersDto.TANNumber;
                user.Username = usersDto.Username;
                user.UpdatedDated = DateTime.Now;

                if (user.Username != usersDto.Username)
                {
                    var isSameUserExist = await _dbContext.Users.FirstOrDefaultAsync(_ => _.Username == usersDto.Username);
                    if (isSameUserExist != null)
                        throw new Exception();
                }

                if (usersDto.CompanyIds?.Count() > 0)
                {
                    _dbContext.Company_Client_Mapping.RemoveRange(_dbContext.Company_Client_Mapping.Where(_ => _.UserId == user.Id));
                    await _dbContext.SaveChangesAsync();

                    foreach (var companyid in usersDto.CompanyIds)
                        _dbContext.Company_Client_Mapping.Add(new Company_Client_Mapping
                        {
                            UserId = Convert.ToInt32(user.Id),
                            CompanyId = companyid
                        });
                }
            }
            else
            {
                var isSameUserExist = await _dbContext.Users.FirstOrDefaultAsync(_ => _.Username == usersDto.Username);
                if (isSameUserExist != null)
                    throw new Exception();

                var user = new User();
                user.FullName = usersDto.FullName;
                user.AccountantMobile = usersDto.AccountantMobile;
                user.JURISDICTION = usersDto.JURISDICTION;
                user.MasterMobile = usersDto.MasterMobile;
                user.PanNumber = usersDto.PanNumber;
                user.Password = Functions.Encrypt(usersDto.Password);
                user.PhoneNumber = usersDto.PhoneNumber;
                user.Pincode = usersDto.Pincode;
                user.Rates = usersDto.Rates;
                user.Remarks = usersDto.Remarks;
                user.AccountantName = usersDto.AccountantName;
                user.Active = usersDto.Active;
                user.Address = usersDto.Address;
                user.ApplicableRate = usersDto.ApplicableRate;
                user.BusinessStatus = usersDto.BusinessStatus;
                user.BusinessType = usersDto.BusinessType;
                user.City = usersDto.City;
                user.CommodityHSN = usersDto.CommodityHSN;
                user.CommodityName = usersDto.CommodityName;
                user.ConsultantId = usersDto.ConsultantId;
                user.ContactPerson = usersDto.ContactPerson;
                user.EFFDate = usersDto.EFFDate;
                user.Email = usersDto.Email;
                user.GroupId = usersDto.GroupId;
                user.GSTIN = usersDto.GSTIN;
                user.GSTRate = usersDto.GSTRate;
                user.RTNType = usersDto.RTNType;
                user.ServiceTypes = usersDto.ServiceTypes;
                user.TANNumber = usersDto.TANNumber;
                user.Username = usersDto.Username;
                user.UserTypeId = (int)Arity.Service.Core.UserType.User;
                user.AddedBy = Convert.ToInt32(SessionHelper.UserId);
                user.CreatedBy = Convert.ToInt32(SessionHelper.UserTypeId);
                user.CreatedDate = user.UpdatedDated = DateTime.Now;
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                _dbContext.User_Role.Add(new User_Role
                {
                    CreatedDate = DateTime.Now,
                    RoleId = (int)Arity.Service.Core.UserType.User,
                    UserId = user.Id,
                    UpdatedDate = DateTime.Now
                });

                if (usersDto.CompanyIds?.Count() > 0)
                {
                    foreach (var companyid in usersDto.CompanyIds)
                        _dbContext.Company_Client_Mapping.Add(new Company_Client_Mapping
                        {
                            UserId = Convert.ToInt32(user.Id),
                            CompanyId = companyid
                        });
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}

