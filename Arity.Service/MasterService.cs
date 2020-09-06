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
using Newtonsoft.Json;

namespace Arity.Service
{
    public class MasterService : IMasterService
    {
        private readonly RMNEntities _dbContext;
        public MasterService(RMNEntities rmnEntities)
        {
            _dbContext = rmnEntities;
        }

        #region Company
        /// <summary>
        /// Get all company
        /// </summary>
        /// <returns></returns>
        public async Task<List<CompanyDto>> GetAllCompany()
        {
            return (from company in _dbContext.Company_master
                    select new CompanyDto
                    {
                        CompanyName = company.CompanyName,
                        Address = company.Address,
                        Id = company.Id,
                        Prefix = company.Prefix,
                        Type = company.Type,
                        IsActive = company.IsActive ?? false,
                        IsAvailableForDelete = !(_dbContext.InvoiceDetails.Any(_ => _.CompanyId == company.Id))
                    }).ToList();
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

        /// <summary>
        /// Delete company
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task DeleteCompany(int companyId)
        {
            _dbContext.Company_master.Remove(_dbContext.Company_master.Where(_ => _.Id == companyId).FirstOrDefault());
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
                              AddedByName = user.FullName,
                              IsAsociatedWithClient = _dbContext.Users.Any(_ => _.GroupId == gm.GroupId)
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
        public async Task<List<UsersDto>> GetAllClient()
        {
            return (from user in _dbContext.Users
                    join type in _dbContext.UserTypes on user.UserTypeId equals type.Id
                    where SessionHelper.UserTypeId == (int)EnumHelper.UserType.Consultant ?
                     user.UserTypeId == (int)Core.UserType.User && user.ConsultantId == SessionHelper.UserId
                    : user.UserTypeId == (int)Core.UserType.User
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
                        Services = !string.IsNullOrEmpty(client.Services) ? JsonConvert.DeserializeObject<List<ServiceTypes>>(client.Services) : null,
                        BankDetails = !string.IsNullOrEmpty(client.BankDetails) ? JsonConvert.DeserializeObject<List<BankDetail>>(client.BankDetails) : null,
                        AdditionlPlaces = !string.IsNullOrEmpty(client.AdditionalPlaces) ? JsonConvert.DeserializeObject<List<GodownDetail>>(client.AdditionalPlaces) : null,
                        CompanyIds = _dbContext.Company_Client_Mapping.Where(_ => _.UserId == client.Id)?.Select(_ => _.CompanyId ?? 0)?.ToArray()
                    }).FirstOrDefault();
        }

        /// <summary>
        /// Get all consultants
        /// </summary>
        /// <returns></returns>
        public async Task<List<UsersDto>> GetAllConsultant()
        {
            return await (from cons in _dbContext.Users
                          where cons.UserTypeId == (int)EnumHelper.UserType.Consultant
                          select new UsersDto()
                          {
                              Id = cons.Id,
                              Username = cons.Username,
                              FullName = cons.FullName,
                              ConsultantId = cons.ConsultantId,
                          }).ToListAsync();
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
                user.Services = JsonConvert.SerializeObject(usersDto.Services);
                user.BankDetails = JsonConvert.SerializeObject(usersDto.BankDetails);
                user.AdditionalPlaces = JsonConvert.SerializeObject(usersDto.AdditionlPlaces);

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
                user.Services = JsonConvert.SerializeObject(usersDto.Services);
                user.AdditionalPlaces = JsonConvert.SerializeObject(usersDto.AdditionlPlaces);
                user.BankDetails = JsonConvert.SerializeObject(usersDto.BankDetails);
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

        public async Task<List<NotificationDTO>> GetAllNotification()
        {
            var users = await _dbContext.Users.ToListAsync();

            return (from n in _dbContext.Notifications.ToList()
                    select new NotificationDTO
                    {
                        NotificationId = n.NotificationId,
                        ClientId = n.ClientId,
                        ClientName = users.FirstOrDefault(_ => _.Id == (n.ClientId ?? 0))?.FullName ?? "All",
                        CreatedBy = n.CreatedBy,
                        CreatedByName = users.FirstOrDefault(_ => _.Id == n.CreatedBy)?.FullName,
                        IsComplete = n.IsComplete ?? false,
                        Message = n.Message,
                        OffBroadcastDateTime = n.OffBroadcastDateTime,
                        OnBroadcastDateTime = n.OnBroadcastDateTime,
                        OffBroadcastDateTimeString = n.OffBroadcastDateTime.ToString("dd/MM/yyyy"),
                        OnBroadcastDateTimeString = n.OnBroadcastDateTime.ToString("dd/MM/yyyy"),
                        TypeString = ((EnumHelper.NotificationType)n.Type).ToString()
                    }).ToList();

        }

        public async Task<NotificationDTO> GetNotificationById(int id)
        {
            return (from n in _dbContext.Notifications
                    where n.NotificationId == id
                    select new NotificationDTO
                    {
                        NotificationId = n.NotificationId,
                        ClientId = n.ClientId,
                        CreatedBy = n.CreatedBy,
                        IsComplete = n.IsComplete ?? false,
                        Message = n.Message,
                        OffBroadcastDateTime = n.OffBroadcastDateTime,
                        OnBroadcastDateTime = n.OnBroadcastDateTime,
                        Type = n.Type
                    }).FirstOrDefault();
        }

        public async Task AddUpdateNotification(NotificationDTO notification)
        {

            if (notification.NotificationId > 0)
            {
                var existingNotifiction = await _dbContext.Notifications.FirstOrDefaultAsync(_ => _.NotificationId == notification.NotificationId);
                existingNotifiction.Message = notification.Message;
                existingNotifiction.IsComplete = notification.IsComplete;
                existingNotifiction.OnBroadcastDateTime = notification.OnBroadcastDateTime;
                existingNotifiction.OffBroadcastDateTime = notification.OffBroadcastDateTime;
                existingNotifiction.ClientId = notification.ClientId;
                existingNotifiction.Type = notification.Type;
                existingNotifiction.CompletedOn = notification.IsComplete ? DateTime.Now : existingNotifiction.CompletedOn;
            }
            else
            {
                _dbContext.Notifications.Add(new Notification
                {
                    Message = notification.Message,
                    IsComplete = notification.IsComplete,
                    OnBroadcastDateTime = notification.OnBroadcastDateTime,
                    OffBroadcastDateTime = notification.OffBroadcastDateTime,
                    ClientId = notification.ClientId,
                    Type = notification.Type,
                    CreatedBy = Convert.ToInt32(SessionHelper.UserId),
                    CreatedOn = DateTime.Now
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<NotificationDTO>> GetAllNotification(int userId, int userType, int type)
        {
            var dd = _dbContext.Notifications.ToList();
            if (userType == (int)EnumHelper.UserType.User)
            {
                return (from n in _dbContext.Notifications.ToList()
                        where (n.ClientId == 0 || n.ClientId == userId)
                        && (n.IsComplete ?? false) == false
                        && DateTime.Now >= n.OnBroadcastDateTime
                        && DateTime.Now <= n.OffBroadcastDateTime
                        && n.Type == type
                        select new NotificationDTO
                        {
                            NotificationId = n.NotificationId,
                            ClientId = n.ClientId,
                            CreatedBy = n.CreatedBy,
                            IsComplete = n.IsComplete ?? false,
                            Message = n.Message,
                            OffBroadcastDateTime = n.OffBroadcastDateTime,
                            OnBroadcastDateTime = n.OnBroadcastDateTime,
                        }).OrderBy(_ => _.ClientId).ToList();
            }
            else
            {
                var tt = (from n in _dbContext.Notifications.ToList()
                          where n.Type == (int)EnumHelper.NotificationType.Notification
                          && (n.IsComplete ?? false) == false
                          && DateTime.Now >= n.OnBroadcastDateTime
                          && DateTime.Now <= n.OffBroadcastDateTime
                          && n.Type == type
                          select new NotificationDTO
                          {
                              NotificationId = n.NotificationId,
                              ClientId = n.ClientId,
                              CreatedBy = n.CreatedBy,
                              IsComplete = n.IsComplete ?? false,
                              Message = n.Message,
                              OffBroadcastDateTime = n.OffBroadcastDateTime,
                              OnBroadcastDateTime = n.OnBroadcastDateTime,
                          }).AsQueryable();
                return tt.OrderBy(_ => _.ClientId).ToList();
            }
        }

        public async Task<List<NotificationDTO>> GetAllNotes(int userId, int userType)
        {
            return (from n in _dbContext.Notifications.ToList()
                    where n.Type == (int)EnumHelper.NotificationType.Notes
                    && (n.ClientId == 0 || n.ClientId == userId)
                    select new NotificationDTO
                    {
                        NotificationId = n.NotificationId,
                        ClientId = n.ClientId,
                        CreatedBy = n.CreatedBy,
                        IsComplete = n.IsComplete ?? false,
                        Message = n.Message,
                        CreatedOn = n.CreatedOn.HasValue ? n.CreatedOn.Value.ToString("dd/MM/yyyy") : String.Empty,
                        CompletedOn = n.CompletedOn.HasValue ? n.CompletedOn.Value.ToString("dd/MM/yyyy") : String.Empty
                    }).OrderBy(_ => _.ClientId).ToList();
        }

        public async Task DeleteNotification(int notificationId)
        {
            _dbContext.Notifications.Remove(_dbContext.Notifications.FirstOrDefault(_ => _.NotificationId == notificationId));
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteGroup(int groupId)
        {
            _dbContext.GroupMasters.Remove(_dbContext.GroupMasters.FirstOrDefault(_ => _.GroupId == groupId));
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<CommodityDTO>> GetAllCommodities()
        {
            return _dbContext.CommodityMasters.ToList().Select(_ => new CommodityDTO
            {
                Id = _.Id,
                GST_Rate = _.GST_Rate,
                HSN = _.HSN,
                Name = _.Name,
                EFDate = _.EFDate,
                EFDateString = _.EFDate.ToString("dd/MM/yyyy")
            }).ToList();
        }

        public async Task<CommodityMaster> GetCommodityById(int id)
        {
            return await _dbContext.CommodityMasters.FirstOrDefaultAsync(_ => _.Id == id);
        }

        public async Task AddUpdateCommodity(CommodityMaster commodity)
        {
            if (commodity.Id > 0)
            {
                var existing = await _dbContext.CommodityMasters.FirstOrDefaultAsync(_ => _.Id == commodity.Id);

                existing.Name = commodity.Name;
                existing.EFDate = commodity.EFDate;
                existing.HSN = commodity.HSN;
                existing.GST_Rate = commodity.GST_Rate;
            }
            else
            {
                _dbContext.CommodityMasters.Add(new CommodityMaster
                {
                    Name = commodity.Name,
                    EFDate = commodity.EFDate,
                    HSN = commodity.HSN,
                    GST_Rate = commodity.GST_Rate,
                    CreatedBy = Convert.ToInt32(SessionHelper.UserId),
                    CreatedOn = DateTime.Now
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ConsultantDTO>> GetAllConsultants()
        {
            return (from _ in _dbContext.Consultant.ToList()
                    join user in _dbContext.Users.ToList() on _.CreatedBy equals user.Id
                    select new ConsultantDTO
                    {
                        Name = _.Name,
                        ConsultantId = _.ConsultantId,
                        Address = _.Address,
                        City = _.City,
                        Mobile = _.Mobile,
                        Notes = _.Notes,
                        CreatedByName = user.FullName
                    }).ToList();
        }

        public async Task<ConsultantDTO> GetConsultantById(int id)
        {
            return await _dbContext.Consultant.Select(_ => new ConsultantDTO
            {
                Name = _.Name,
                ConsultantId = _.ConsultantId,
                Address = _.Address,
                City = _.City,
                Mobile = _.Mobile,
                Notes = _.Notes,
                CreatedBy = _.CreatedBy,
                CreatedOn = _.CreatedOn
            }).FirstOrDefaultAsync();
        }

        public async Task AddUpdateConsultant(ConsultantDTO consultant)
        {
            if (consultant.ConsultantId > 0)
            {
                var existingConsultant = await _dbContext.Consultant.FirstOrDefaultAsync(_ => _.ConsultantId == consultant.ConsultantId);
                existingConsultant.Name = consultant.Name;
                existingConsultant.Notes = consultant.Notes;
                existingConsultant.Mobile = consultant.Mobile;
                existingConsultant.City = consultant.City;
                existingConsultant.Address = consultant.Address;
                existingConsultant.ModifiedOn = DateTime.Now;
                existingConsultant.ModifiedBy = Convert.ToInt32(SessionHelper.UserId);

            }
            else
            {
                _dbContext.Consultant.Add(new Consultants
                {
                    Name = consultant.Name,
                    Notes = consultant.Notes,
                    Mobile = consultant.Mobile,
                    City = consultant.City,
                    Address = consultant.Address,
                    CreatedOn = DateTime.Now,
                    CreatedBy = Convert.ToInt32(SessionHelper.UserId)
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveConsultant(int id)
        {
            _dbContext.Consultant.Remove(_dbContext.Consultant.FirstOrDefault(_ => _.ConsultantId == id));
            await _dbContext.SaveChangesAsync();
        }

        #region Document type
        /// <summary>
        /// Get list of document type
        /// </summary>
        /// <returns></returns>
        public async Task<List<DocumentTypes>> GetAllDocumentTypes()
        {
            return await _dbContext.DocumentTypes.ToListAsync();
        }

        /// <summary>
        /// Get document type by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DocumentTypes> GetDocumentTypeById(int id)
        {
            return await _dbContext.DocumentTypes.FirstOrDefaultAsync(_ => _.DocumnetTypeId == id);
        }

        /// <summary>
        /// Add update document type
        /// </summary>
        /// <param name="documentType"></param>
        /// <returns></returns>
        public async Task AddUpdateDocumentType(DocumentTypes documentType)
        {
            if (documentType.DocumnetTypeId > 0)
            {
                var existingDocumentType = await _dbContext.DocumentTypes.FirstOrDefaultAsync(_ => _.DocumnetTypeId == documentType.DocumnetTypeId);
                existingDocumentType.Name = documentType.Name;
                existingDocumentType.IsActive = documentType.IsActive;
            }
            else
            {
                var existingDocumentType = await _dbContext.DocumentTypes.FirstOrDefaultAsync(_ => _.Name == documentType.Name);
                if (existingDocumentType == null)
                    _dbContext.DocumentTypes.Add(documentType);
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete document type by id
        /// </summary>
        /// <param name="documentTypeId"></param>
        /// <returns></returns>
        public async Task DeleteDocumentType(int documentTypeId)
        {
            _dbContext.DocumentTypes.Remove(await _dbContext.DocumentTypes.FirstOrDefaultAsync(_ => _.DocumnetTypeId == documentTypeId));
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region Business status
        /// <summary>
        /// Get list of business status
        /// </summary>
        /// <returns></returns>
        public async Task<List<BusinessStatus>> GetAllBusinessStatus()
        {
            return await _dbContext.BusinessStatus.ToListAsync();
        }

        /// <summary>
        /// Get business status by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BusinessStatus> GetBusinessStatusById(int id)
        {
            return await _dbContext.BusinessStatus.FirstOrDefaultAsync(_ => _.BusinessStatusId == id);
        }

        /// <summary>
        /// Add update business status
        /// </summary>
        /// <param name="documentType"></param>
        /// <returns></returns>
        public async Task AddUpdateBusinessStatus(BusinessStatus businessStatus)
        {
            if (businessStatus.BusinessStatusId > 0)
            {
                var existingBusinessStatus = await _dbContext.BusinessStatus.FirstOrDefaultAsync(_ => _.BusinessStatusId == businessStatus.BusinessStatusId);
                existingBusinessStatus.Name = businessStatus.Name;
                existingBusinessStatus.IsActive = businessStatus.IsActive;
            }
            else
            {
                var existingBusinessStatus = await _dbContext.BusinessStatus.FirstOrDefaultAsync(_ => _.Name == businessStatus.Name);
                if (existingBusinessStatus == null)
                    _dbContext.BusinessStatus.Add(businessStatus);
            }
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete business status by id
        /// </summary>
        /// <param name="documentTypeId"></param>
        /// <returns></returns>
        public async Task DeleteBusinessStatus(int businessStatusId)
        {
            _dbContext.BusinessStatus.Remove(await _dbContext.BusinessStatus.FirstOrDefaultAsync(_ => _.BusinessStatusId == businessStatusId));
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region Business Types
        /// <summary>
        /// Get list of business types
        /// </summary>
        /// <returns></returns>
        public async Task<List<BusinessTypes>> GetAllBusinessType()
        {
            return await _dbContext.BusinessTypes.ToListAsync();
        }

        /// <summary>
        /// Get business types by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BusinessTypes> GetBusinessTypeById(int id)
        {
            return await _dbContext.BusinessTypes.FirstOrDefaultAsync(_ => _.BusinessTypeId == id);
        }

        /// <summary>
        /// Add update business types
        /// </summary>
        /// <param name="documentType"></param>
        /// <returns></returns>
        public async Task AddUpdateBusinessTypes(BusinessTypes businessTypes)
        {
            if (businessTypes.BusinessTypeId > 0)
            {
                var existingBusinessTypes = await _dbContext.BusinessTypes.FirstOrDefaultAsync(_ => _.BusinessTypeId == businessTypes.BusinessTypeId);
                existingBusinessTypes.Name = businessTypes.Name;
                existingBusinessTypes.IsActive = businessTypes.IsActive;
            }
            else
            {
                var existingBusinessTypes = await _dbContext.BusinessTypes.FirstOrDefaultAsync(_ => _.Name == businessTypes.Name);
                if (existingBusinessTypes == null)
                    _dbContext.BusinessTypes.Add(businessTypes);
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete business types by id
        /// </summary>
        /// <param name="businessTypesId"></param>
        /// <returns></returns>
        public async Task DeleteBusinessTypes(int businessTypesId)
        {
            _dbContext.BusinessTypes.Remove(await _dbContext.BusinessTypes.FirstOrDefaultAsync(_ => _.BusinessTypeId == businessTypesId));
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}

