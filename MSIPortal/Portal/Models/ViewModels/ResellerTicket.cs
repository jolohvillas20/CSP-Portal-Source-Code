using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class ResellerTicket
    {
        public string SenderName { get; set; }
        public string MPNID { get; set; }
        public Tickets Ticket { get; set; }
    }
}