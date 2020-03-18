using Arity.Data.Dto;
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
        Task<UsersDto> GetClientById(int id);
        Task<List<Consultants>> GetAllConsultant();
        Task<List<GroupMasterDTO>> GetAllGroups();
        Task AddUpdateClient(UsersDto usersDto);
    }
}
