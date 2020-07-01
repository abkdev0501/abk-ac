﻿using Arity.Data.Dto;
using Arity.Data.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arity.Service.Contract
{
    public interface IMasterService
    {
        Task<List<CompanyDto>> GetAllCompany();
        Task<CompanyDto> GetCompanyById(int id);
        Task AddUpdateCompany(CompanyDto data);
        Task<List<GroupMasterDTO>> GetAllGroup();
        Task<GroupMasterDTO> GetGroupById(int id);
        Task AddUpdateGroup(GroupMasterDTO groupMaster);
        Task<List<UsersDto>> GetAllClient(DateTime from, DateTime to);
        Task<List<UsersDto>> GetAllClient();
        Task<UsersDto> GetClientById(int id);
        Task<List<Consultants>> GetAllConsultant();
        Task<List<GroupMasterDTO>> GetAllGroups();
        Task AddUpdateClient(UsersDto usersDto);
        Task<List<NotificationDTO>> GetAllNotification();
        Task<NotificationDTO> GetNotificationById(int id);
        Task AddUpdateNotification(NotificationDTO notification);
        Task<List<NotificationDTO>> GetAllNotification(int userId, int userType,int type);
        Task<List<NotificationDTO>> GetAllNotes(int userId, int userType);
        Task DeleteNotification(int notificationId);
        Task DeleteGroup(int groupId);
        Task<List<CommodityDTO>> GetAllCommodities();
        Task<CommodityMaster> GetCommodityById(int id);
        Task AddUpdateCommodity(CommodityMaster commodity);
        Task<List<ConsultantDTO>> GetAllConsultants();
        Task<ConsultantDTO> GetConsultantById(int id);
        Task AddUpdateConsultant(ConsultantDTO consultant);
        Task RemoveConsultant(int id);
        Task DeleteCompany(int companyId);
    }
}
