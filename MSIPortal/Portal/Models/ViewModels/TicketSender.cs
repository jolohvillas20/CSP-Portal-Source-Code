using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class TicketSender
    {
    }

  
    public class TicketAdmin
    {
        public int TicketID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> TicketDate { get; set; }
        public Nullable<int> TicketSenderID { get; set; }

        ResellerRepository resellerRepo = new ResellerRepository();

        TicketRepository ticketRepo = new TicketRepository();


        public string ResellerName(int? UserID)
        {
            string reseller = resellerRepo.GetResellerName(UserID);

            return reseller;
        }

        public string LastRemarks(int _TicketID)
        {
            string remarks = ticketRepo.GetLastRemarks(_TicketID);

            return remarks;
        }




    }
}