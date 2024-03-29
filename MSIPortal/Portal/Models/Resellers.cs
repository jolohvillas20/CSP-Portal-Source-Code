//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Resellers
    {
        public int ResellerID { get; set; }
        public Nullable<int> MPNID { get; set; }
        public bool IsVatExempted { get; set; }
        public string ResellerName { get; set; }
        public string Emails { get; set; }
        public Nullable<int> UsersID { get; set; }
        public int ResellerStatus { get; set; }
        public bool IsApprove { get; set; }
        public Nullable<int> UserTypeID { get; set; }
        public Nullable<int> AuthorizedRepresentative { get; set; }
        public bool IsAllowedToEdit { get; set; }
        public Nullable<System.DateTime> DateRegistered { get; set; }
        public string CompanyName { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public string SellerID { get; set; }
    
        public virtual UserTypes UserTypes { get; set; }
        public virtual Users Users { get; set; }
    }
}
