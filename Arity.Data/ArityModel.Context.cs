﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Arity.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class RMNEntities : DbContext
    {
        public RMNEntities()
            : base("name=RMNEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    
        public virtual DbSet<Client_Particular_Mapping> Client_Particular_Mapping { get; set; }
        public virtual DbSet<Company_master> Company_master { get; set; }
        public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public virtual DbSet<InvoiceParticular> InvoiceParticulars { get; set; }
        public virtual DbSet<InvoiceRecieptMapping> InvoiceRecieptMappings { get; set; }
        public virtual DbSet<Particular> Particulars { get; set; }
        public virtual DbSet<RecieptDetail> RecieptDetails { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User_Role> User_Role { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserType> UserTypes { get; set; }
        public virtual DbSet<Company_Client_Mapping> Company_Client_Mapping { get; set; }
    }
}
