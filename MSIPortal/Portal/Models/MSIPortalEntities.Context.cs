﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Portal.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MSIPortalEntities : DbContext
    {
        public MSIPortalEntities()
            : base("name=MSIPortalEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AuditTrails> AuditTrails { get; set; }
        public virtual DbSet<Tickets> Tickets { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<AuthorizedRepresentatives> AuthorizedRepresentatives { get; set; }
        public virtual DbSet<UserTypes> UserTypes { get; set; }
        public virtual DbSet<ErrorLogs> ErrorLogs { get; set; }
        public virtual DbSet<TermsAndConditions> TermsAndConditions { get; set; }
        public virtual DbSet<Modules1> Modules1 { get; set; }
        public virtual DbSet<UserAccesses1> UserAccesses1 { get; set; }
        public virtual DbSet<Resellers> Resellers { get; set; }
        public virtual DbSet<OrderItems> OrderItems { get; set; }
        public virtual DbSet<Subscriptions> Subscriptions { get; set; }
        public virtual DbSet<UserAccessHeader> UserAccessHeader { get; set; }
        public virtual DbSet<Offers> Offers { get; set; }
        public virtual DbSet<UserAccessDetails> UserAccessDetails { get; set; }
        public virtual DbSet<TicketDetails> TicketDetails { get; set; }
        public virtual DbSet<EmailTemplates> EmailTemplates { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<Customers> Customers { get; set; }
        public virtual DbSet<SpecialQualifications> SpecialQualifications { get; set; }
        public virtual DbSet<Transactions> Transactions { get; set; }
    }
}
