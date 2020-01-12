using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arity.Data;
using Arity.Data.Dto;
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
                parti.UpdatedDate = DateTime.Now;
            }
            else
            {
                parti.ParticularFF = particular.ParticularFF;
                parti.ParticularSF = particular.ParticularSF;
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
        public async Task<List<ParticularDto>> FetchParticular()
        {
            return await (from data in _dbContext.Particulars
                    select new ParticularDto()
                    {
                        Id = (int)data.Id,
                        ParticularFF = data.ParticularFF,
                        ParticularSF = data.ParticularSF,
                        CreatedDate = data.CreatedDate,
                        UpdatedDate = data.UpdatedDate
                    }).ToListAsync();
        }

        public ParticularDto FetchParticularById(int id)
        {
            return (from particular in _dbContext.Particulars.Where(_ => _.Id == id)
                    select new ParticularDto()
                    {
                        Id = (int)particular.Id,
                        ParticularFF = particular.ParticularFF,
                        ParticularSF = particular.ParticularSF,
                        CreatedDate = particular.CreatedDate
                    }).FirstOrDefault();
        }
    }
}
