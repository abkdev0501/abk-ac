using System;
using System.Collections.Generic;

namespace Arity.Data.Dto
{
    public class UsersDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string Pincode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDated { get; set; }
        public long UserTypeId { get; set; }
        public int[] CompanyIds { get; set; }
        public long Id { get; set; }
        public string UserType { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public int CreatedBy { get; set; }
        public int? GroupId { get; set; }
        public string MasterMobile { get; set; }
        public string AccountantName { get; set; }
        public string AccountantMobile { get; set; }
        public string ContactPerson { get; set; }
        public int? ConsultantId { get; set; }
        public string Remarks { get; set; }
        public int? BusinessType { get; set; }
        public int? BusinessStatus { get; set; }
        public string PanNumber { get; set; }
        public string TANNumber { get; set; }
        public string GSTIN { get; set; }
        public DateTime? EFFDate { get; set; }
        public string JURISDICTION { get; set; }
        public string RTNType { get; set; }
        public string CommodityName { get; set; }
        public string CommodityHSN { get; set; }
        public string GSTRate { get; set; }
        public DateTime? ApplicableRate { get; set; }
        public string ServiceTypes { get; set; }
        public string Rates { get; set; }
        public int AddedBy { get; set; }
        public string AddedByName { get; set; }

        public List<ServiceTypes> Services { get; set; }
        public List<CompanyDto> Companies { get; set; }
        public List<ParticularDto> Particulars { get; set; }
        public List<GodownDetail> AdditionlPlaces { get; set; }
        public List<BankDetail> BankDetails { get; set; }
    }
}
