using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class ResellerDTO
    {
        public int ResellerID { get; set; }
        public int MPNID { get; set; }
        public string ResellerName { get; set; }
        public string Emails { get; set; }
        public Nullable<int> UsersID { get; set; }
        public Nullable<int> UserTypeID { get; set; }
        public int ResellerStatus { get; set; }
        public bool OnHold { get; set; }
        public bool IsVatExempted { get; set; }
        public bool AllowEdit { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        //authorized representative
        public int repID {get; set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactNo { get; set; }
        public string EmailAddress { get; set; }
        public string SellerID { get; set; }
        public string CompanyName { get; set; }
    }
}