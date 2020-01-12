using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.AutoMapper
{
    public static class AutoMapperProfile
    {
        public static void Init()
        {
            Mapper.Initialize(config =>
            {

                //config.CreateMap<Tbl_Vendor_Master, Vendor>();
            });
        }

    }
}
