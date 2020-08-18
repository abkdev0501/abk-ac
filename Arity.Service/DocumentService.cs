using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service.Contract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using static Arity.Data.Helpers.EnumHelper;

namespace Arity.Service
{
    public class DocumentService : IDocumentService
    {
        private readonly RMNEntities _dbContext;

        public DocumentService(RMNEntities rMNEntities)
        {
            _dbContext = rMNEntities;
        }



        /// <summary> 
        /// Fetch document list from database
        /// </summary>
        /// <param name="toDate"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        public async Task<List<DocumentMasterDto>> FetchDocuments(DateTime toDate, DateTime fromDate)
        {
            var documentTypes = await _dbContext.DocumentTypes.ToListAsync();
            if (Convert.ToInt32(SessionHelper.UserTypeId) == (int)Arity.Service.Core.UserType.User)
            {
                return (from data in _dbContext.DocumentMasters.ToList()
                        join user in _dbContext.Users.ToList() on data.ClientId equals user.Id
                        join createdBy in _dbContext.Users.ToList() on data.CreatedBy equals createdBy.Id
                        where data.CreatedOn >= fromDate && data.CreatedOn <= toDate && data.ClientId == Convert.ToInt32(SessionHelper.UserId)
                        select new DocumentMasterDto()
                        {
                            DocumentId = data.DocumentId,
                            Name = data.Name,
                            ClientId = data.ClientId,
                            DocumentTypeName = documentTypes.FirstOrDefault(_ => _.DocumnetTypeId == data.DocumentType)?.Name ?? string.Empty,
                            IsActive = data.IsActive,
                            CreatedBy = data.CreatedBy,
                            CreatedOn = data.CreatedOn,
                            CreatedByString = createdBy.FullName,
                            AddedBy = Convert.ToInt32(createdBy.UserTypeId),
                            StatusName = Enum.GetName(typeof(DocumentStatus), data.Status),
                            ClientName = user.FullName,
                            UserName = user.Username,
                            FileName = data.FileName
                        }).ToList();
            }
            else
                return (from data in _dbContext.DocumentMasters.ToList()
                        join user in _dbContext.Users.ToList() on data.ClientId equals user.Id
                        join createdBy in _dbContext.Users.ToList() on data.CreatedBy equals createdBy.Id
                        where data.CreatedOn >= fromDate && data.CreatedOn <= toDate
                        select new DocumentMasterDto()
                        {
                            DocumentId = data.DocumentId,
                            Name = data.Name,
                            ClientId = data.ClientId,
                            DocumentTypeName = documentTypes.FirstOrDefault(_ => _.DocumnetTypeId == data.DocumentType)?.Name ?? string.Empty,
                            IsActive = data.IsActive,
                            CreatedBy = data.CreatedBy,
                            CreatedOn = data.CreatedOn,
                            CreatedByString = createdBy.FullName,
                            AddedBy = Convert.ToInt32(createdBy.UserTypeId),
                            StatusName = Enum.GetName(typeof(DocumentStatus), data.Status),
                            ClientName = user.FullName,
                            UserName = user.Username,
                            FileName = data.FileName
                        }).ToList();
        }

        public async Task<List<DocumentMasterDto>> FetchDocuments()
        {
            var documentTypes = await _dbContext.DocumentTypes.ToListAsync();
            if (Convert.ToInt32(SessionHelper.UserTypeId) == (int)Arity.Service.Core.UserType.User)
            {
                return (from data in _dbContext.DocumentMasters.ToList()
                        join user in _dbContext.Users.ToList() on data.ClientId equals user.Id
                        join createdBy in _dbContext.Users.ToList() on data.CreatedBy equals createdBy.Id
                        where  data.ClientId == Convert.ToInt32(SessionHelper.UserId)
                        select new DocumentMasterDto()
                        {
                            DocumentId = data.DocumentId,
                            Name = data.Name,
                            ClientId = data.ClientId,
                            DocumentTypeName = documentTypes.FirstOrDefault(_ => _.DocumnetTypeId == data.DocumentType)?.Name ?? string.Empty,
                            IsActive = data.IsActive,
                            CreatedBy = data.CreatedBy,
                            CreatedOn = data.CreatedOn,
                            CreatedByString = createdBy.FullName,
                            AddedBy = Convert.ToInt32(createdBy.UserTypeId),
                            StatusName = Enum.GetName(typeof(DocumentStatus), data.Status),
                            ClientName = user.FullName,
                            UserName = user.Username,
                            FileName = data.FileName
                        }).ToList();
            }
            else
                return (from data in _dbContext.DocumentMasters.ToList()
                        join user in _dbContext.Users.ToList() on data.ClientId equals user.Id
                        join createdBy in _dbContext.Users.ToList() on data.CreatedBy equals createdBy.Id
                        select new DocumentMasterDto()
                        {
                            DocumentId = data.DocumentId,
                            Name = data.Name,
                            ClientId = data.ClientId,
                            DocumentTypeName = documentTypes.FirstOrDefault(_ => _.DocumnetTypeId == data.DocumentType)?.Name ?? string.Empty,
                            IsActive = data.IsActive,
                            CreatedBy = data.CreatedBy,
                            CreatedOn = data.CreatedOn,
                            CreatedByString = createdBy.FullName,
                            AddedBy = Convert.ToInt32(createdBy.UserTypeId),
                            StatusName = Enum.GetName(typeof(DocumentStatus), data.Status),
                            ClientName = user.FullName,
                            UserName = user.Username,
                            FileName = data.FileName
                        }).ToList();
        }




        /// <summary>
        /// Get client list from database
        /// </summary>
        /// <returns></returns>
        public async Task<List<User>> GetClient()
        {
            return await (from cd in _dbContext.Users
                          select cd).ToListAsync();
        }

        public async Task<DocumentMasterDto> GetDocumentByID(int documentID)
        {
            var document = await _dbContext.DocumentMasters.Where(_ => _.DocumentId == documentID).Select(_ =>
                     new DocumentMasterDto()
                     {
                         DocumentId = _.DocumentId,
                         Name = _.Name,
                         ClientId = _.ClientId,
                         CreatedBy = _.CreatedBy,
                         CreatedOn = _.CreatedOn,
                         Status = (DocumentStatus)_.Status,
                         IsActive = _.IsActive,
                         FileName = _.FileName,
                         DocumentType = _.DocumentType
                     }).FirstOrDefaultAsync();

            if (document != null)
                document.DocumentType = _dbContext.DocumentTypes.FirstOrDefault(x => x.DocumnetTypeId == document.DocumentType)?.DocumnetTypeId ?? 0;

            return document;
        }

        public async Task<int> SaveDocument(DocumentMasterDto document)
        {
            DocumentMaster documentMaster = new DocumentMaster();
            try
            {
                // Add document into database
                if (document.DocumentId == 0)
                {
                    documentMaster.Name = document.Name;
                    documentMaster.DocumentType = (int)document.DocumentType;
                    documentMaster.ClientId = document.ClientId;
                    documentMaster.CreatedOn = DateTime.Now;
                    documentMaster.CreatedBy = Convert.ToInt32(SessionHelper.UserId);
                    documentMaster.Status = (int)document.Status;
                    documentMaster.FileName = document.FileName;
                    documentMaster.IsActive = document.IsActive;
                    _dbContext.DocumentMasters.Add(documentMaster);
                }
                if (document.DocumentId > 0)
                {
                    documentMaster = _dbContext.DocumentMasters.Where(_ => _.DocumentId == document.DocumentId).FirstOrDefault();
                    documentMaster.Name = document.Name;
                    documentMaster.DocumentType = (int)document.DocumentType;
                    documentMaster.ClientId = document.ClientId;
                    documentMaster.ModifiedOn = DateTime.Now;
                    documentMaster.ModifiedBy = Convert.ToInt32(SessionHelper.UserId);
                    documentMaster.Status = (int)document.Status;
                    if (document.FileName != null)
                        documentMaster.FileName = document.FileName;
                    documentMaster.IsActive = document.IsActive;
                }
                await _dbContext.SaveChangesAsync();

            }
            catch
            {
            }
            return documentMaster.DocumentId;
        }

        public bool DeleteDocumentByID(int documentID)
        {
            try
            {
                _dbContext.DocumentMasters.Remove(_dbContext.DocumentMasters.Where(_ => _.DocumentId == documentID).FirstOrDefault());
                _dbContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<DocumentMasterDto>> GetAllDocuments()
        {
            return (from data in _dbContext.DocumentMasters.ToList()
                    join user in _dbContext.Users.ToList() on data.ClientId equals user.Id
                    select new DocumentMasterDto()
                    {
                        DocumentId = data.DocumentId,
                        Name = data.Name,
                        ClientId = data.ClientId,
                        DocumentTypeName = Enum.GetName(typeof(DocumentType), data.DocumentType),
                        IsActive = data.IsActive,
                        CreatedBy = data.CreatedBy,
                        CreatedOn = data.CreatedOn,
                        StatusName = Enum.GetName(typeof(DocumentStatus), data.Status),
                        ClientName = user.FullName
                    }).OrderBy(_ => _.ClientId).ToList();
        }

        public async Task<List<DocumentMasterDto>> GetDocumentByUserID(int userId)
        {
            return (from data in _dbContext.DocumentMasters.ToList()
                    join user in _dbContext.Users.ToList() on data.ClientId equals user.Id
                    where data.ClientId == userId
                    select new DocumentMasterDto()
                    {
                        DocumentId = data.DocumentId,
                        Name = data.Name,
                        ClientId = data.ClientId,
                        DocumentTypeName = Enum.GetName(typeof(DocumentType), data.DocumentType),
                        IsActive = data.IsActive,
                        CreatedBy = data.CreatedBy,
                        CreatedOn = data.CreatedOn,
                        StatusName = Enum.GetName(typeof(DocumentStatus), data.Status),
                        ClientName = user.FullName
                    }).ToList();
        }
    }
}
