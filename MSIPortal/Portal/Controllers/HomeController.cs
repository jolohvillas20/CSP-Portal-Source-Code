using Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.Store.PartnerCenter.Models.Customers;
using Microsoft.Store.PartnerCenter.Models;
using Microsoft.Store.PartnerCenter.Samples.Context;
using Microsoft.Store.PartnerCenter.Samples.Customers;
using Portal.Helpers;
using Microsoft.Store.PartnerCenter.Customers;
using Microsoft.Store.PartnerCenter.Models.Orders;
using Microsoft.Store.PartnerCenter.Models.Offers;
using System.Configuration;
using System.Web.Configuration;

namespace Portal.Controllers
{
    public class HomeController : Controller
    {
        MSIPortalEntities db = new MSIPortalEntities();

        UserRepository userRepo = new UserRepository();
        Mailer _mailer = new Mailer();

        public ActionResult Index()
        {
            return View();

        }
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            Session.Clear();
            return View();
        }

        public ActionResult ChangePassword()
        {
            try
            {
                ViewBag.UserID = Session["msiUserID"].ToString();
                return View();
            }
            catch (Exception x)
            {
                return View("Login");
            }

        }


        // POST: /Account/Login
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]


        //public async Task<ActionResult> Login(LoginViewModel model)
        //{
        //    string x = "";
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var resultUser = db.Users.Where(e => e.password == model.Password).FirstOrDefault();
        //    if (resultUser != null)
        //    {
        //        Session["msiUserID"] = resultUser.ID;
        //        Sessions.UserID = resultUser.ID;
        //        Session["msiUserType"] = resultUser.UserTypes.UserType;
        //        Sessions.UserType = resultUser.UserTypes.UserType;
        //        Session["msiUserTypeID"] = resultUser.UserTypeID;
        //        Sessions.UserTypeID = resultUser.UserTypeID;

        //        Session["msiResellerID"] = 0;
        //        Sessions.ResellerID = 0;
        //        Session["msiResellerName"] = "";
        //        Sessions.ResellerName = "";

        //        return RedirectToAction("Index", "Home");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("", "Invalid username/password.");
        //        return View(model);
        //    }

        //}





        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]


        public async Task<ActionResult> Login(LoginViewModel model)
        {
            string x = "";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = userRepo.Encrypt(model.Username);
            string password = userRepo.Encrypt(model.Password);

            var resultUser = db.Users.Where(e => e.password == password).ToList() ;
            var _user = db.Users.Where(p => p.username == username && p.password == password).FirstOrDefault();

            if (resultUser != null)
            {
                if (_user != null)
                {
                    x = "valid";
                }
                //else if (db.Resellers.Where(e => e.MPNID.ToString() == model.Username).FirstOrDefault() != null)
                //{
                //    x = "valid";
                //}
                else
                {
                    x = "invalid";
                }

            }

            if (x == "valid")
            {
                var resultReseller = db.Resellers.Where(e => e.UsersID == _user.ID).FirstOrDefault();
                try
                {

                    if (resultReseller == null && _user.Active == true)
                    {
                        Session["msiUserID"] = _user.ID;
                        Sessions.UserID = _user.ID;
                        Session["msiUserType"] = _user.UserTypes.UserType;
                        Sessions.UserType = _user.UserTypes.UserType;
                        Session["msiUserTypeID"] = _user.UserTypeID;
                        Sessions.UserTypeID = _user.UserTypeID;
                        Session["AccountName"] = _user.FirstName;


                        Session["msiResellerID"] = 0;
                        Sessions.ResellerID = 0;
                        Session["msiResellerName"] = "";
                        Sessions.ResellerName = "";

                        Session["msiResellerStatus"] ="";
                        Sessions.ResellerStatus =0;

                        return RedirectToAction("Index", "Home");
                    }

                    else
                    {
                        
                        if (_user.Active == false && ( resultReseller != null && resultReseller.ResellerStatus == 1))
                        {

                            Session["msiUserID"] = _user.ID;
                            Session["AccountName"] = _user.FirstName;
                            Sessions.UserID = _user.ID;
                            Session["msiUserTypeID"] = resultReseller != null ? resultReseller.UserTypeID : _user.UserTypeID;
                            Sessions.UserTypeID = resultReseller != null ? resultReseller.UserTypeID : _user.UserTypeID;
                            Session["msiUserType"] = resultReseller != null ? resultReseller.UserTypes.UserType : _user.UserTypes.UserType;
                            Sessions.UserType = resultReseller != null ? resultReseller.UserTypes.UserType : _user.UserTypes.UserType;
                            Session["IsActive"] = _user.Active;
                            Sessions.IsActive = _user.Active;
                            Session["ResellerMPNID"] = resultReseller != null ? resultReseller.MPNID : 0;
                            Sessions.ResellerMPNID = resultReseller != null ? resultReseller.MPNID.ToString() : " ";
                            Session["msiResellerStatus"] = resultReseller != null ? resultReseller.ResellerStatus : 0;
                            Sessions.ResellerStatus = resultReseller != null ? resultReseller.ResellerStatus : 0;

                            return RedirectToAction("ChangePassword");
                        }
                        else if (resultUser == null || _user.ID == 1 || (resultReseller != null && resultReseller.IsApprove == false))
                        {
                            ModelState.AddModelError("", "Invalid username/password.");
                            return View(model);
                        }
                        else if ((resultReseller != null && resultReseller.ResellerStatus == 0) || (_user.Active == false && _user.UserTypeID != 5))
                        {
                            ModelState.AddModelError("", "This Account is Inactive.");
                            return View(model);
                        }
                        else
                        {
                            Session["msiUserID"] = _user.ID;
                            Sessions.UserID = _user.ID;
                            Session["msiUserType"] = resultReseller.UserTypes.UserType;
                            Sessions.UserType = resultReseller.UserTypes.UserType;
                            Session["msiUserTypeID"] = resultReseller.UserTypeID;
                            Sessions.UserTypeID = resultReseller.UserTypeID;
                            Session["AccountName"] = _user.FirstName;


                            Session["ResellerMPNID"] = resultReseller.MPNID;
                            Sessions.ResellerMPNID = resultReseller.MPNID.ToString();

                            Session["msiResellerID"] = resultReseller.ResellerID;
                            Sessions.ResellerID = resultReseller.ResellerID;
                            Session["msiResellerName"] = resultReseller.ResellerName;
                            Sessions.ResellerName = resultReseller.ResellerName;

                            Session["msiResellerStatus"] = resultReseller.ResellerStatus;
                            Sessions.ResellerStatus = resultReseller.ResellerStatus;

                            return RedirectToAction("Details", "Resellers", new { id = resultReseller.ResellerID });
                        }
                    }
                    
                }

                catch
                {
                    ModelState.AddModelError("", "Invalid username/password.");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid username/password.");
                return View(model);
            }
        }


        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                var reseller = db.Resellers.Where(p => p.Emails.Contains(model.Email)).ToList();

                foreach (var _res in reseller)
                {
                    try
                    {
                        string newpw = userRepo.GenaratePassword();

                        _mailer.SendForgotPassword(_res.Emails, _res.ResellerName, userRepo.Decrypt(newpw));

                        Users user1 = db.Users.Find(_res.UsersID);
                        user1.password = newpw;
                        user1.Active = false;
                        db.SaveChanges(); // set user as inactive and save new generated password

                    }
                    catch (Exception x)
                    {
                        ModelState.AddModelError("", x.Message.ToString());
                    }
                }

                var user = db.Users.Where(p => p.Email.Contains(model.Email)).ToList();

                foreach (var _user in user)
                {
                    try
                    {
                        string newpw = userRepo.GenaratePassword();

                        _mailer.SendForgotPassword(_user.Email, _user.FirstName+" "+ _user.LastName, userRepo.Decrypt(newpw));
                      

                        Users user1 = db.Users.Find(_user.ID);
                        user1.password = newpw;
                        user1.Active = false;
                        db.SaveChanges(); // set user as inactive and save new generated password

                    }
                    catch (Exception x)
                    {
                        ModelState.AddModelError("", x.Message.ToString());
                    }
                }

                ModelState.AddModelError("", "Please check your email for the code to reset your password.");

            }
            else
            {
                ModelState.AddModelError("", "Invalid Email");
            }

            return View(model);
        }

        public void UpdatePassword(UserDTO userDTO)
        {

            Session["password"] = userRepo.UpdatePassword(userDTO.ID, userDTO.Oldpassword, userDTO.Newpassword);

        }

        public ActionResult Denied()
        {
            return View();
        }

        private void EncryptWebConfig()
        {
            try
            {
                string provider = "RSAProtectedConfigurationProvider";
                string section = "connectionStrings";

                //////encrypt
                //Configuration confg = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                //ConfigurationSection confStrSect = confg.GetSection(section);
                //if (confStrSect != null)
                //{
                //    confStrSect.SectionInformation.ProtectSection(provider);
                //    confg.Save();
                //}
                ////the encrypted section is automatically decrypted!!

               // decrypt
                Configuration confg = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                ConfigurationSection confStrSect = confg.GetSection(section);
                if (confStrSect != null && confStrSect.SectionInformation.IsProtected)
                {
                    confStrSect.SectionInformation.UnprotectSection();
                    confg.Save();
                }
                // the encrypted section is automatically decrypted!!
                string x = "Configuration Section " + "<b>" +
                      WebConfigurationManager.ConnectionStrings["DBConn"].ConnectionString + "</b>" + " is automatically decrypted";
            }
            catch { }
        }

    }
}