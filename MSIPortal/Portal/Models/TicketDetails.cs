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
    
    public partial class TicketDetails
    {
        public int id { get; set; }
        public int TicketID { get; set; }
        public string Remarks { get; set; }
        public System.DateTime Date { get; set; }
        public int EditedBy { get; set; }
    
        public virtual Users Users { get; set; }
        public virtual Tickets Tickets { get; set; }
    }
}
