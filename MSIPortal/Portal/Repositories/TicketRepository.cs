using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Models
{
    public class TicketRepository
    {
        private MSIPortalEntities db = new MSIPortalEntities();
     

        public string GetLastRemarks(int TicketID)
        {
            string remarks = "";
            TicketDetails _ticketdetails = db.TicketDetails.Where(p => p.TicketID == TicketID).OrderByDescending(p=>p.id).FirstOrDefault();

            if (_ticketdetails != null)
            {
                remarks =  _ticketdetails.Remarks;
            }
            return remarks;



        }

    }
}