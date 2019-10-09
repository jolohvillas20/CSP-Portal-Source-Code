using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
//using System.Net;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using PagedList;
using System.Net.Mail;
using System.Threading.Tasks;
using Portal.Helpers;
using Portal.Repositories;
using Microsoft.Store.PartnerCenter;
using Microsoft.Store.PartnerCenter.Samples.Context;
using System.Net;

namespace Portal.Controllers
{
    [AuthorizationFilter]
    public class ResellersController : Controller
    {
        private MSIPortalEntities db = new MSIPortalEntities();
        UserRepository userRepo = new UserRepository();
        ResellerRepository resellerRepo = new ResellerRepository();

        Mailer _mailer = new Mailer();
        //  [AuthorizationFilterPM(ModuleName ="Reseller",ViewName = "NoAccess")]
        // GET: Resellers

        [AccessChecker(Action = 1, ModuleID = 1)]
        public ActionResult Index(string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
        {
            //int _usertype;
            //if (UserType == null)
            //{
            //    _usertype = 1;
            //}
            //else
            //{
            //    _usertype = UserType.Value;
            //}


            //if (Sessions.UserType == "PM")
            //    return RedirectToAction("PendingAccounts");

            if (Sessions.UserType == "Reseller")
                return RedirectToAction("Details", new { id = Sessions.ResellerID });



            ViewBag.CurrentSort = sortColumn;
            ViewBag.SortOrder = sortOrder == "asc" ? "desc" : "asc";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            try
            {


                IQueryable<Resellers> _lstResellers = resellerRepo.List().AsQueryable();

                if (Sessions.UserType == "Sales AE")
                {
                    var _assign = db.UserAccessHeader.Where(u => u.UserID == Sessions.UserID);

                    if (_assign.Count() > 0)
                    {
                        var _taggedid = db.UserAccessDetails.Where(a => a.UserAccessHeader.UserID == Sessions.UserID && a.UserAccessHeader.Category == "Resellers")
                                                                                    .Select(a => a.TaggedID).ToArray();

                        _lstResellers = _lstResellers.Where(p => _taggedid.Contains(p.ResellerID.ToString()));
                    }
                    else
                        _lstResellers = null;


                }



                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    _lstResellers = _lstResellers.Where(s => s.MPNID.ToString().ToLower().Contains(searchString)
                                           || s.ResellerName.ToLower().Contains(searchString)
                                           || s.Emails.ToLower().Contains(searchString));
                }

                if (String.IsNullOrEmpty(sortColumn))
                {
                    _lstResellers = _lstResellers.OrderByDescending(p => p.ResellerID);
                }
                else
                {
                    _lstResellers = _lstResellers.OrderBy(sortColumn + " " + sortOrder);
                }

                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(_lstResellers.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception e)
            {
                e.ToString();
                TempData["Message"] = "No Assigned Reseller/s";
                return View("Index");

            }
        }

        [AccessChecker(Action = 4, ModuleID = 1)]
        [AuthorizationFilterPM(ModuleName = "Reseller", ViewName = "PendingAccounts")]
        public ActionResult PendingAccounts(string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (Sessions.UserType != "Admin" && Sessions.UserType != "PM")
                return RedirectToAction("Details", new { @id = Sessions.UserID });


            ViewBag.CurrentSort = sortColumn;
            ViewBag.SortOrder = sortOrder == "asc" ? "desc" : "asc";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            try
            {

                IQueryable<Resellers> _lstResellers = db.Resellers.Where(p => p.IsApprove == false && p.UserTypeID == 5).ToList().AsQueryable();

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    _lstResellers = _lstResellers.Where(s => s.MPNID.ToString().ToLower().Contains(searchString)
                                           || s.ResellerName.ToLower().Contains(searchString)
                                           || s.Emails.ToLower().Contains(searchString));
                }

                if (String.IsNullOrEmpty(sortColumn))
                {
                    _lstResellers = _lstResellers.OrderByDescending(p => p.ResellerID);
                }
                else
                {
                    _lstResellers = _lstResellers.OrderBy(sortColumn + " " + sortOrder);
                }


                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(_lstResellers.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception e)
            {
                e.ToString();
                return View("Index", "Home");
            }
        }

        [AccessChecker(Action = 1, ModuleID = 1)]
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    //check Sessions.UserID
                    id = Sessions.ResellerID;
                    if (id == null)
                    {
                        if (Sessions.UserType == "Admin")
                            return RedirectToAction("Index");
                        else
                            return RedirectToAction("Login", "Home");

                        //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }


                }
                Resellers resellers = db.Resellers.Find(id);
                Users users = db.Users.Find(resellers.UsersID);

                AuthorizedRepresentatives rep = db.AuthorizedRepresentatives.Find(resellers.AuthorizedRepresentative);

                ResellerAccount reselleracct = new ResellerAccount();
                reselleracct.Reseller = resellers;
                users.username = userRepo.Decrypt(users.username);
                users.password = userRepo.Decrypt(users.password);
                reselleracct.User = users;
                reselleracct.AuthorizedRepresentative = rep;

                ViewBag.UserType = resellers.UserTypeID.ToString();
                if (resellers.ResellerStatus == 1)
                {
                    ViewBag.Status = "Active";
                }
                else
                {
                    ViewBag.Status = "Inactive";
                }

                if (reselleracct == null)
                {
                    return HttpNotFound();
                }
                return View(reselleracct);
            }
            catch (Exception x)
            {
                return RedirectToAction("Login", "Home");
            }

        }

        [AccessChecker(Action = 2, ModuleID = 1)]
        public ActionResult Create()
        {

            Resellers re = new Resellers();
            Users user = new Users();
            AuthorizedRepresentatives rep = new AuthorizedRepresentatives();

            ResellerAccount reselleracct = new ResellerAccount();
            reselleracct.Reseller = re;
            reselleracct.User = user;
            reselleracct.AuthorizedRepresentative = rep;
            return View(reselleracct);
        }


        [AccessChecker(Action = 2, ModuleID = 1)]
        public ActionResult Edit(int? id)
        {
            ViewBag.UserType = Sessions.UserType;
            if (Sessions.UserType == "Admin")
                ViewBag.TypeID = 1;
            else
                ViewBag.TypeID = 5;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Resellers resellers = db.Resellers.Find(id);
            Users users = db.Users.Find(resellers.UsersID);
            AuthorizedRepresentatives rep = db.AuthorizedRepresentatives.Find(resellers.AuthorizedRepresentative);

            ResellerAccount reselleracct = new ResellerAccount();
            reselleracct.Reseller = resellers;
            users.username = userRepo.Decrypt(users.username);
            users.password = userRepo.Decrypt(users.password);
            reselleracct.User = users;
            reselleracct.AuthorizedRepresentative = rep;


            if (reselleracct == null)
            {
                return HttpNotFound();
            }
            ViewBag.UsersID = new SelectList(db.Users, "ID", "username", resellers.UsersID);
            ViewBag.UserTypeID = resellers.UserTypeID;

            //if (Sessions.UserType != "Admin")
            //{
            //    ViewBag.AllowEdit = Convert.ToInt32(resellers.IsAllowedToEdit);
            //}
            //else
            //{
            ViewBag.AllowEdit = Convert.ToInt32(resellers.IsAllowedToEdit);
            // }

            return View(reselleracct);
        }

        [AccessChecker(Action = 3, ModuleID = 1)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Resellers resellers = db.Resellers.Find(id);
            Users users = db.Users.Find(resellers.UsersID);

            ResellerAccount reselleracct = new ResellerAccount();
            reselleracct.Reseller = resellers;
            reselleracct.User = users;

            if (resellers.ResellerStatus == 1)
            {
                ViewBag.Status = "Active";
            }
            else
            {
                ViewBag.Status = "Inactive";
            }

            if (reselleracct == null)
            {
                return HttpNotFound();
            }
            return View(reselleracct);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Resellers resellers = db.Resellers.Find(id);
            Users users = db.Users.Find(resellers.UsersID);
            db.Users.Remove(users);
            db.Resellers.Remove(resellers);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        // ------------ ajax

        public JsonResult Approve(ResellerDTO res)
        {
            string msg = "okay";
            Resellers reseller = db.Resellers.Find(res.ResellerID);

            if (reseller != null)
            {
                //int vatfalse = db.Resellers.Count(p => p.MPNID == reseller.MPNID && p.IsVatExempted == false && p.IsApprove == true);
                //int vattrue = db.Resellers.Count(p => p.MPNID == reseller.MPNID && p.IsVatExempted == true && p.IsApprove == true);

                //if (vattrue >= 1 && vatfalse >= 1)
                //{
                //    msg = reseller.ResellerName + "(" + reseller.MPNID + ")" + "already exists";
                //}
                //else
                //{
                //    if (res.IsVatExempted == true && vattrue == 0)
                //    {
                //        reseller.IsVatExempted = true;
                //        reseller.IsApprove = true;
                //        reseller.ApprovedBy = Sessions.UserID;
                //        _mailer.SendEmailToAllAdmin("Reseller", "Approved Account with MPNID w/ Vat Exempted" + reseller.MPNID, Sessions.ResellerName);
                //    }

                //    else if (res.IsVatExempted == false && vatfalse == 0)
                //    {
                //        res.IsVatExempted = false;
                //        reseller.IsApprove = true;
                //        reseller.ApprovedBy = Sessions.UserID;
                //        _mailer.SendEmailToAllAdmin("Reseller", "Updated Account with MPNID  w/o Vat Exempted" + reseller.MPNID, Sessions.ResellerName);
                //    }
                //    else
                //    {
                //        msg = reseller.ResellerName + "(" + reseller.MPNID + ")" + "already exists";
                //    }
                //    db.SaveChanges();
                //}//validation for mpnid (depending on the VAT Exempt)

                if (res.IsVatExempted == true)
                {
                    reseller.IsVatExempted = true;
                    reseller.IsApprove = true;
                    reseller.ApprovedBy = Sessions.UserID;
                    _mailer.SendEmailToAllAdmin("Reseller", "Approved Account with MPNID w/ Vat Exempted  " + reseller.MPNID, Sessions.ResellerName);
                }

                else if (res.IsVatExempted == false)
                {
                    res.IsVatExempted = false;
                    reseller.IsApprove = true;
                    reseller.ApprovedBy = Sessions.UserID;
                    _mailer.SendEmailToAllAdmin("Reseller", "Approved Account with MPNID  w/o Vat Exempted  " + reseller.MPNID, Sessions.ResellerName);
                }

                db.SaveChanges();
            }

            //SAVE TO AUDIT TRAIL
            int _empID = Sessions.UserID.Value;
            AuditTrails _audit = new AuditTrails();
            _audit.AuditDate = DateTime.Now;
            _audit.Module = "Account Approval";
            _audit.TaskDesc = "Approved Account Account ID:  " + reseller.ResellerID.ToString();
            _audit.UserID = Sessions.UserID;
            db.AuditTrails.Add(_audit);
            db.SaveChanges();

            // send email for password given
            Users user = db.Users.Find(reseller.UsersID);
            _mailer.SendConfirmEmail(reseller.Emails, reseller.ResellerName, userRepo.Decrypt(user.password));

            return Json(msg, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Save(ResellerDTO entry)
        {
            string x = "okay";

            try
            {
                if (entry.ResellerID == 0)
                {
                    int repid = 0;

                    Users user = new Users();

                    Resellers reseller = new Resellers();

                    user.FirstName = entry.ResellerName;
                    user.LastName = "-Reseller";
                    user.Email = entry.Emails;
                    user.username = userRepo.Encrypt(entry.username);
                    user.password = userRepo.GenaratePassword();

                    if (db.Resellers.Where(p => p.MPNID == entry.MPNID && p.Users.password == user.password).Count() == 0)
                    {
                        user.Active = false;
                        user.UserTypeID = 5;

                        db.Users.Add(user);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception e) { x = "error 1" + e.Message; }

                        //if (entry.UserTypeID == 5){
                        var authorizedrep = db.AuthorizedRepresentatives.FirstOrDefault(p => p.EmailAddress.Trim() == entry.EmailAddress);
                        int ID = authorizedrep != null ? authorizedrep.id : 0;
                        if (ID == 0)
                        {
                            AuthorizedRepresentatives rep = new AuthorizedRepresentatives();
                            rep.FirstName = entry.FirstName;
                            rep.LastName = entry.LastName;
                            rep.ContactNo = entry.ContactNo;
                            rep.EmailAddress = entry.EmailAddress;

                            db.AuthorizedRepresentatives.Add(rep);
                            db.SaveChanges();

                            repid = rep.id;
                        }
                        else
                        {
                            AuthorizedRepresentatives rep = db.AuthorizedRepresentatives.Find(ID);

                            rep.FirstName = entry.FirstName;
                            rep.LastName = entry.LastName;
                            rep.ContactNo = entry.ContactNo;
                            rep.EmailAddress = entry.EmailAddress;

                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception e) { x = "error 2" + e.Message; }

                            repid = ID;
                        }
                        //}

                        reseller.UsersID = user.ID;
                        reseller.AuthorizedRepresentative = repid;
                        reseller.MPNID = Convert.ToInt32(entry.MPNID);
                        reseller.ResellerName = entry.ResellerName;
                        reseller.ResellerStatus = 1;
                        reseller.IsApprove = true;
                        reseller.IsVatExempted = entry.IsVatExempted;
                        reseller.Emails = entry.Emails;
                        reseller.CompanyName = entry.CompanyName;
                        reseller.SellerID = entry.SellerID;
                        reseller.DateRegistered = DateTime.Now;
                        reseller.UserTypeID = 5;
                        reseller.ApprovedBy = Sessions.UserID;

                        db.Resellers.Add(reseller);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception e) { x = "error 3" + e.Message; }

                        // send email for password given
                        _mailer.SendConfirmEmail(entry.Emails, entry.ResellerName, userRepo.Decrypt(user.password));
                        _mailer.SendEmailToAllAdmin("Reseller", "Created Account with MPNID" + reseller.MPNID, db.Users.Find(Sessions.UserID.Value).FirstName + "  " + db.Users.Find(Sessions.UserID.Value).LastName);

                    }
                    else
                    {
                        x = "An error occured while saving. pw change 1";
                        return Json(x);
                    }
                }



                else
                {

                    Resellers reseller = db.Resellers.Find(entry.ResellerID);

                    //string _emailchanges = "Reseller Name:  " + reseller.ResellerName + "-" + entry.ResellerName + "<br/>" +
                    //                       "Email Address:  " + reseller.Emails + "-" + entry.Emails + "<br/>" +
                    //                       "MPNID:  " + reseller.MPNID + "-" + entry.MPNID + "<br/>" +
                    //                       "Username:  " + userRepo.Decrypt(reseller.Users.username) + "-" + entry.username + "<br/>";

                    string _emailchanges = "This account has been updated <br/> <br/>" + "Reseller Name:  " + entry.ResellerName + "<br/>" +
                                           "Email Address:  " + entry.Emails + "<br/>" +
                                           "MPNID:  " + entry.MPNID + "<br/>" +
                                           "Username:  " + entry.username + "<br/>";


                    Users user = db.Users.Find(reseller.UsersID);

                    AuthorizedRepresentatives rep = db.AuthorizedRepresentatives.FirstOrDefault(p => p.EmailAddress.Trim() == entry.EmailAddress.Trim());
                    AuthorizedRepresentatives rep2 = db.AuthorizedRepresentatives.Find(reseller.AuthorizedRepresentative);


                    user.username = userRepo.Encrypt(entry.username);
                    entry.password = userRepo.Encrypt(entry.password);

                    if (db.Users.Where(p => p.password == entry.password && p.ID == reseller.UsersID).Count() == 0)
                    {

                        user.password = entry.password;
                        user.Active = false;
                        // send email for new password given
                        _mailer.SendConfirmEmail(entry.Emails, entry.ResellerName, userRepo.Decrypt(user.password));
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e) { x = "error 4" + e.Message; }

                    if (rep != null)
                    {

                        if (Sessions.UserType == "Reseller" && reseller.AuthorizedRepresentative != rep.id)
                        {

                            _emailchanges = "This account has been updated <br/> <br/>" + "Authorized Representative:  " + entry.FirstName + " " + entry.LastName + "<br/>" +
                                            "Contact No:  " + entry.ContactNo + "<br/>" +
                                            "Email Address:  " + entry.EmailAddress + "<br/>";

                            reseller.IsAllowedToEdit = false;
                        }

                        AuthorizedRepresentatives rep1 = db.AuthorizedRepresentatives.Find(rep.id);

                        rep1.FirstName = entry.FirstName;
                        rep1.LastName = entry.LastName;
                        rep1.ContactNo = entry.ContactNo;
                        rep1.EmailAddress = entry.EmailAddress;

                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception e) { x = "error 5" + e.Message; }

                        reseller.AuthorizedRepresentative = rep.id;
                    }
                    else
                    {
                        AuthorizedRepresentatives _rep = new AuthorizedRepresentatives();

                        _rep.FirstName = entry.FirstName;
                        _rep.LastName = entry.LastName;
                        _rep.ContactNo = entry.ContactNo;
                        _rep.EmailAddress = entry.EmailAddress;

                        db.AuthorizedRepresentatives.Add(_rep);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception e) { x = "error 6" + e.Message; }

                        if (Sessions.UserType == "Reseller" && reseller.AuthorizedRepresentative != rep.id)
                        {
                            reseller.IsAllowedToEdit = false;
                        }
                        reseller.AuthorizedRepresentative = _rep.id;

                    }



                    reseller.MPNID = Convert.ToInt32(entry.MPNID);
                    reseller.ResellerName = entry.ResellerName;
                    reseller.SellerID = entry.SellerID;
                    reseller.IsVatExempted = entry.IsVatExempted;
                    reseller.IsAllowedToEdit = entry.AllowEdit;

                    if (Sessions.UserType == "Admin" || Sessions.UserType == "PM")
                    {
                        reseller.ResellerStatus = entry.ResellerStatus;
                    }
                    reseller.Emails = entry.Emails;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e) { x = "error 7" + e.Message; }
                    _mailer.SendEmailToAllAdmin("Reseller Update Account", _emailchanges, reseller.ResellerName);


                }

            }// try

            catch (Exception e) { x = "An error occured while saving." + e.Message; }
            return Json(x);
        }//end



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
                        else if (db.Users.Count(p => p.Email.Trim() == entry.Emails.Trim() && p.ID == userid) == 0)
                        {
                            //return Json("okay");
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
                    else if (db.Users.Count(p => p.username == username && p.ID == userid) > 0)
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

        public JsonResult GeneratePassword()
        {
            return Json(userRepo.Decrypt(userRepo.GenaratePassword()));
        }

        public void RequestToEdit(int id)
        {
            Resellers reseller = db.Resellers.Find(id);

            if (reseller != null)
            {
                var admin = db.Users.Where(a => a.UserTypeID == 1 && a.Email != null).Select(a => a.Email).ToList();

                foreach (var _admin in admin)
                {
                    _mailer.SendRequestToAdminToEdit_AuthorizedRep(reseller.MPNID.ToString(), reseller.ResellerName, _admin);
                }
            }
        }

        public void AllowToEdit(int id, int check)
        {
            userRepo.AllowResellerToEdit(id, check);
        }


        [HttpPost]
        public JsonResult GetResellersName(string ResellersName)
        {
            if (ResellersName == "" || ResellersName == null)
                ResellersName = "";

            var resName = (from c in db.Resellers
                           where (c.ResellerName.ToUpper()).Contains(ResellersName.ToUpper())
                           select new { ResellerName = c.ResellerName, ID = c.ResellerID })
                              .Distinct().ToList();

            return Json(resName, JsonRequestBehavior.AllowGet);
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

        //public void GetAzureRecord()
        //{
        //    //var context = new ScenarioContext();
        //    //var partnerOperations = context.AppPartnerOperations;
        //    //var usage = partnerOperations.UsageSummary;
        //    //usage.Context.


        //   Microsoft.Store.PartnerCenter.Usage.ICustomerUsageRecordCollection  credentials = null;

        //    IAggregatePartner partnerOperations;
        //    string customerId = "";
        //    string subscriptionId = "";

        //    IPartner partner = PartnerService.Instance.CreatePartnerOperations(credentials);

        //    // Retrieve the utilization records for the last year in pages of 100 records.
        //    var utilizationRecords = partner.Customers[customerId].Subscriptions[subscriptionId].Utilization.Azure.Query(
        //        DateTimeOffset.Now.AddYears(-1),
        //        DateTimeOffset.Now,
        //        size: 100);

        //    // Create an Azure utilization enumerator which will aid us in traversing the utilization pages.
        //    var utilizationRecordEnumerator = partner.Enumerators.Utilization.Azure.Create(utilizationRecords);

        //    while (utilizationRecordEnumerator.HasValue)
        //    {
        //        //
        //        // Insert code here to work with this page.
        //        //

        //        // Get the next page.
        //        utilizationRecordEnumerator.Next();
        //    }
        //}

    }
}
