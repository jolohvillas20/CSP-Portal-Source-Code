using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class TicketDTO
    {
        public int TicketID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> TicketDate { get; set; }
        public int TicketSenderID { get; set; }
    }
}