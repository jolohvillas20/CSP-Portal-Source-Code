using Portal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Models
{
    public class ResellerRepository
    {
        private MSIPortalEntities db = new MSIPortalEntities();


        public IQueryable<Resellers> List()
        {
            return db.Resellers;
        }
        public string GetResellerName(int? UserID)
        {

            Resellers reseller = db.Resellers.FirstOrDefault(p => p.UsersID == UserID);

            if (reseller != null)
                return reseller.ResellerName;

            else
            {
                Users user = db.Users.FirstOrDefault(p => p.ID == UserID);
                return user.FirstName + " " + user.LastName;
            }

        }

        public string GetResellerEmails(int? UserID)
        {

            Resellers reseller = db.Resellers.FirstOrDefault(p => p.UsersID == UserID);

            if (reseller != null)
                return reseller.Emails;

            else
            {
                Users user = db.Users.FirstOrDefault(p => p.ID == UserID);
                return user.Email;
            }


        }

        public List<SelectListItem> GetResellerSubs()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem
            {
                Text = "No indirect reseller",
                Value = "0"
            });
            //listItems.Add(new SelectListItem
            //{
            //    Text = "Indirect reseller not in the list? Enter MPN ID",
            //    Value = "enterMPNID"
            //});

            var resellers = db.Resellers.Where(x => x.MPNID != null && x.MPNID != null).ToList();
            foreach (var item in resellers)
            {
                string vatable = item.IsVatExempted == true ? "Non-Vat" : "Vatable";
                listItems.Add(new SelectListItem
                {
                    Text = item.ResellerName + " (" + vatable + ")",
                    Value = item.ResellerID.ToString()
                });
            }
            return listItems;
        }

        public List<SelectListItem> GetResellerSubs_ID(int mpnid)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();

            var resellers = db.Resellers.Where(x => x.ResellerID == mpnid).ToList();
            foreach (var item in resellers)
            {
                string vatable = item.IsVatExempted == true ? "Non-Vat" : "Vatable";
                listItems.Add(new SelectListItem
                {
                    Text = item.ResellerName + " (" + vatable + ")",
                    Value = item.ResellerID.ToString()
                });
            }
            return listItems;
        }

        public List<SelectListItem> GetResellerSubs(int id, string mpnid )
        {
            List<SelectListItem> listItems = new List<SelectListItem>();

            if (id != 0)
            {
                var resellers = db.Resellers.Where(x => x.ResellerID == id).ToList();
                foreach (var item in resellers)
                {
                    string vatable = item.IsVatExempted == true ? "Non-Vat" : "Vatable";
                    listItems.Add(new SelectListItem
                    {
                        Text = item.ResellerName + " (" + vatable + ")",
                        Value = item.ResellerID.ToString()
                    });
                }
            }
            else
            {
                CustomerRepository _customerRepo = new CustomerRepository(); 
                listItems.Add(new SelectListItem
                {
                    Text = _customerRepo.VerifyMPNID(mpnid) + " (" + mpnid + ")",
                    Value = "none"
                });

            }
            return listItems;
        }


        public List<SelectListItem> GetResellerSubsNo()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem
            {
                Text = "No indirect reseller",
                Value = "0",
                Selected = true
            });
            return listItems;

        }


    }
}