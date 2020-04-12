using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service.Contract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arity.Data.Helpers.EnumHelper;

namespace Arity.Service
{
    public class DocumentService : IDocumentService
    {
        private readonly RMNEntities _dbContext;

        public DocumentService()
        {
            _dbContext = new RMNEntities();
        }

        

        /// <summary> 
        /// Fetch document list from database
        /// </summary>
        /// <param name="toDate"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        public async Task<List<DocumentMasterDto>> FetchDocuments(DateTime toDate, DateTime fromDate)
        {
            return (from data in _dbContext.DocumentMasters.ToList()
                    join user in _dbContext.Users.ToList() on data.ClientId equals user.Id
                    where data.CreatedOn >= fromDate && data.CreatedOn <= toDate
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
            DocumentMasterDto document = new DocumentMasterDto();
            return (from documentMaster
                    in _dbContext.DocumentMasters.Where(_ => _.DocumentId == documentID)
                    select new DocumentMasterDto()
                    {
                        DocumentId = documentMaster.DocumentId,
                        Name = documentMaster.Name,
                        ClientId = documentMaster.ClientId,
                        DocumentType = (DocumentType)documentMaster.DocumentType,
                        CreatedBy = documentMaster.CreatedBy,
                        CreatedOn = documentMaster.CreatedOn,
                        Status = (DocumentStatus)documentMaster.Status,
                        IsActive = documentMaster.IsActive,
                        FileName = documentMaster.FileName
        }).FirstOrDefault();
        }

        public int SaveDocument(DocumentMasterDto document)
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
                if(document.DocumentId > 0)
                {
                    documentMaster = _dbContext.DocumentMasters.Where(_ => _.DocumentId == document.DocumentId).FirstOrDefault();
                    documentMaster.Name = document.Name;
                    documentMaster.DocumentType = (int)document.DocumentType;
                    documentMaster.ClientId = document.ClientId;
                    documentMaster.ModifiedOn = DateTime.Now;
                    documentMaster.ModifiedBy = Convert.ToInt32(SessionHelper.UserId);
                    documentMaster.Status = (int)document.Status;
                    if(document.FileName != null) 
                    documentMaster.FileName = document.FileName;
                    documentMaster.IsActive = document.IsActive;
                }
                _dbContext.SaveChanges();

            }
            catch (Exception ex)
            {

            }
            return documentMaster.DocumentId;
        }

        public bool DeleteDocumentByID(int documentID)
        {
            try
            {
                _dbContext.DocumentMasters.Remove(_dbContext.DocumentMasters.Where(_ => _.DocumentId == documentID).FirstOrDefault());
                //_dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
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
                    }).OrderBy(_=>_.ClientId).ToList();
        }

        DocumentMasterDto IDocumentService.GetDocumentByID(int documentID)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DocumentMasterDto>> GetDocumentByUserID(int userId)
        {
            return (from data in _dbContext.DocumentMasters.ToList()
                    join user in _dbContext.Users.ToList() on data.ClientId equals user.Id
                    where data.CreatedBy == userId
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
