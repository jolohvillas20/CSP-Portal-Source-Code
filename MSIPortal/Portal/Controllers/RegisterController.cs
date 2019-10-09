using Portal.Helpers;
using Portal.Models;
using Portal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{
    public class RegisterController : Controller
    {

        private MSIPortalEntities db = new MSIPortalEntities();
        UserRepository userRepo = new UserRepository();
        Mailer _mailer = new Mailer();
        // GET: Register
        public ActionResult Register()
        {
            Resellers re = new Resellers();
            Users user = new Users();
            AuthorizedRepresentatives rep = new AuthorizedRepresentatives();

            ResellerAccount reselleracct = new ResellerAccount();
            reselleracct.Reseller = re;
            reselleracct.User = user;
            reselleracct.AuthorizedRepresentative = rep;


            ViewBag.TNC = db.TermsAndConditions.FirstOrDefault().TermsAndConditions1;

            return View(reselleracct);
          
        }

        public void Save(ResellerDTO entry)
        {

            if (entry.ResellerID == 0)
            {
                Users user = new Users();
                AuthorizedRepresentatives rep = new AuthorizedRepresentatives();
                Resellers reseller = new Resellers();

                user.FirstName = entry.ResellerName;
                user.LastName = "-Reseller";
                user.UserTypeID = 5;

                user.username = userRepo.Encrypt(entry.username);
                user.password = userRepo.GenaratePassword();
                if (db.Resellers.Where(p => p.MPNID == entry.MPNID && p.Users.password == user.password).Count() == 0)
                {
                    user.Active = false;
                    
                    db.Users.Add(user);
                    db.SaveChanges();

                    int _repid = 0;
                    AuthorizedRepresentatives _repexist = db.AuthorizedRepresentatives.Find(entry.repID);
                    if (_repexist == null)
                    {
                        rep.FirstName = entry.FirstName;
                        rep.LastName = entry.LastName;
                        rep.ContactNo = entry.ContactNo;
                        rep.EmailAddress = entry.EmailAddress;

                        db.AuthorizedRepresentatives.Add(rep);
                        db.SaveChanges();

                        _repid = rep.id;
                    }
                    else
                    {
                        _repid = entry.repID;
                    }

                    reseller.UsersID = user.ID;
                    reseller.AuthorizedRepresentative = _repid;
                    reseller.MPNID = Convert.ToInt32(entry.MPNID);
                    reseller.ResellerName = entry.ResellerName;
                    reseller.CompanyName = entry.CompanyName;
                    reseller.DateRegistered = DateTime.Now;
                    //reseller.ResellerStatus = entry.ResellerStatus;
                    //reseller.OnHold = entry.OnHold;
                    reseller.Emails = entry.Emails;
                    reseller.UserTypeID = 5;
                    reseller.ResellerStatus = 1;
                    reseller.IsApprove = false;

                    db.Resellers.Add(reseller);
                    db.SaveChanges();

                    // send email to notify reseller that his/her account is for admin's approval
                    _mailer.SendAccountForApproval(entry.Emails, entry.ResellerName);
                    _mailer.SendEmailToAllAdmin("New Reseller", "Request for Approval  " + entry.Emails, entry.ResellerName);


                }
                else
                {

                }

            }
            
        }

        public JsonResult CheckValidEmail(ResellerDTO entry)
        {   
            try
            {
                var addr = new System.Net.Mail.MailAddress(entry.Emails);

                bool exists1 = db.Users.Count(p => p.Email.Trim() == entry.Emails.Trim()) > 0;
                if (exists1)
                {
                    try
                    {
                        int? userid = db.Resellers.Where(p => p.ResellerID == entry.ResellerID).FirstOrDefault().UsersID;

                        if (entry.ResellerID == 0)
                        {
                            throw new Exception();
                        }
                        else if (db.Users.Count(p => p.Email.Trim() == entry.Emails.Trim() && p.ID == userid) > 0)
                        {
                           
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    catch
                    { return Json("Email already exists"); }
                }
            }
            catch
            { return Json("invalid email"); }

            string username = userRepo.Encrypt(entry.username);
            bool exists = db.Users.Count(p => p.username == username) > 0;
            if (exists)
            {
                try 
                {
                    int? userid = db.Resellers.Where(p => p.ResellerID == entry.ResellerID).FirstOrDefault().UsersID;

                    if (entry.ResellerID == 0)
                    {
                        return Json("Username already exists");
                    }
                    else if (db.Users.Count(p => p.username == entry.username && p.ID == userid) > 0)
                    {
                        return Json("okay");
                    }
                    else
                    {
                        return Json("Username already exists");
                    }
                }
                catch
                { return Json("Username already exists"); }
            }
            return Json("okay");
        }

        [HttpPost]
        public JsonResult GetAuthorized(string email)
        {
            var authorized = db.AuthorizedRepresentatives.Distinct().Where(x => x.EmailAddress.Contains(email))
                            .Select(x => new
                            {
                                ID = x.id,
                                Email = x.EmailAddress,
                                Firstname = x.FirstName,
                                LastName = x.LastName,
                                Contactno = x.ContactNo == null ? " " : x.ContactNo
                            });
            return Json(authorized, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VerifyMPNID(string mpnID)
        {
            var exists = "";

            CustomerRepository _customerRepo = new CustomerRepository();
            exists = _customerRepo.VerifyMPNID(mpnID);

            return Json(exists, JsonRequestBehavior.AllowGet);

        }

    }
}