﻿using Arity.Data.Dto;
using Arity.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service.Contract
{
    public interface IDocumentService 
    {

        Task<List<User>> GetClient();
        Task<int> SaveDocument(DocumentMasterDto document);
        Task<List<DocumentMasterDto>> FetchDocuments(DateTime toDate, DateTime fromDate);
        Task<List<DocumentMasterDto>> FetchDocuments();
        Task<DocumentMasterDto> GetDocumentByID(int documentID);
        bool DeleteDocumentByID(int documentID);
        Task<List<DocumentMasterDto>> GetAllDocuments();
        Task<List<DocumentMasterDto>> GetDocumentByUserID(int userId);
    }
}
