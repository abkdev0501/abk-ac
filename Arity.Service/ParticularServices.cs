using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arity.Data;
using Arity.Data.Dto;
using Arity.Data.Entity;
using Arity.Service.Contract;
namespace Arity.Service
{
    public class ParticularServices : IParticularServices
    {
        private readonly RMNEntities _dbContext;
        public ParticularServices()
        {
            _dbContext = new RMNEntities();
        }

        public void AddParticular(ParticularDto particular)
        {
            Particular parti = new Particular();
            if (particular.Id > 0)
            {
                parti = _dbContext.Particulars.Where(_ => _.Id == particular.Id).FirstOrDefault();
                parti.ParticularFF = particular.ParticularFF;
                parti.ParticularSF = particular.ParticularSF;
                parti.IsExclude = particular.IsExclude;
                parti.UpdatedDate = DateTime.Now;
            }
            else
            {
                parti.ParticularFF = particular.ParticularFF;
                parti.ParticularSF = particular.ParticularSF;
                parti.IsExclude = particular.IsExclude;
                parti.CreatedDate = DateTime.Now;
                parti.UpdatedDate = DateTime.Now;
                _dbContext.Particulars.Add(parti);
            }
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// fetch List of particulars from db 
        /// </summary>
        /// <returns></returns>
        public async Task<List<ParticularDto>> FetchParticular(DateTime toDate, DateTime fromDate)
        {
            return (from data in _dbContext.Particulars.ToList()
                    where data.CreatedDate >= fromDate && data.CreatedDate <= toDate
                    select new ParticularDto()
                    {
                        Id = (int)data.Id,
                        ParticularFF = data.ParticularFF,
                        ParticularSF = data.ParticularSF,
                        CreatedDate = data.CreatedDate,
                        UpdatedDate = data.UpdatedDate,
                        IsExclude = data.IsExclude ?? false,
                        CreatedDateString = data.CreatedDate.ToString("dd/MM/yyyy")
                    }).ToList();
        }

        public ParticularDto FetchParticularById(int id)
        {
            return (from particular in _dbContext.Particulars.Where(_ => _.Id == id)
                    select new ParticularDto()
                    {
                        Id = (int)particular.Id,
                        ParticularFF = particular.ParticularFF,
                        ParticularSF = particular.ParticularSF,
                        IsExclude = particular.IsExclude??false,
                        CreatedDate = particular.CreatedDate
                    }).FirstOrDefault();
        }
    }
}
