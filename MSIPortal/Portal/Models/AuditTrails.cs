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
    
    public partial class AuditTrails
    {
        public int AuditID { get; set; }
        public string Module { get; set; }
        public string TaskDesc { get; set; }
        public Nullable<System.DateTime> AuditDate { get; set; }
        public Nullable<int> UserID { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
