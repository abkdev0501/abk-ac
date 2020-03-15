using Arity.Data.Entity;
using System.Data.Entity;

namespace Arity.Data
{
    public class RMNEntities : DbContext
    {
        public RMNEntities()
            : base("name=RMNEntities")
        {
        }

        public virtual DbSet<Client_Particular_Mapping> Client_Particular_Mapping { get; set; }
        public virtual DbSet<Company_Client_Mapping> Company_Client_Mapping { get; set; }
        public virtual DbSet<Company_master> Company_master { get; set; }
        public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public virtual DbSet<InvoiceParticular> InvoiceParticulars { get; set; }
        public virtual DbSet<InvoiceReciept> InvoiceReciepts { get; set; }
        public virtual DbSet<InvoiceTracking> InvoiceTrackings { get; set; }
        public virtual DbSet<RecieptDetail> RecieptDetails { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User_Role> User_Role { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserType> UserTypes { get; set; }
        public virtual DbSet<Particular> Particulars { get; set; }
        public virtual DbSet<Tasks> Tasks { get; set; }
        public virtual DbSet<UserTask> UserTasks { get; set; }
        public virtual DbSet<Consultants> Consultant { get; set; }
        public virtual DbSet<DocumentMaster> DocumentMasters { get; set; }
    }
}
