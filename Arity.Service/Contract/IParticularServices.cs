using Arity.Data.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service.Contract
{
    public interface IParticularServices
    {
        void AddParticular(ParticularDto particular);
        Task<List<ParticularDto>> FetchParticular(DateTime toDate, DateTime fromDate);
        ParticularDto FetchParticularById(int id);
    }
}
