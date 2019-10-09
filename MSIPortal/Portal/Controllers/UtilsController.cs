using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using Portal.Repositories;
using Microsoft.Store.PartnerCenter.Models.Customers;
using Microsoft.Store.PartnerCenter.Models;
using System.Web.Script.Serialization;
using Portal.Helpers;
using PagedList;
using System.Data.Entity.Validation;
using System.Reflection;

namespace Portal.Controllers
{
    [AuthorizationFilter]
    public class UtilsController : Controller
    {
       
        private MSIPortalEntities db = new MSIPortalEntities();

        [AccessChecker(Action = 1, ModuleID = 6)]
        public ActionResult TermsandConditions()
        {
            TermsAndConditions _terms = db.TermsAndConditions.FirstOrDefault();

            if (_terms == null)
            {
                _terms = new TermsAndConditions();
            }

            return View(_terms);
        }

        public void Utils_Terms(UtilsDTO utils)
        {
            if (utils.ID == 0)
            {
                TermsAndConditions _terms = new TermsAndConditions();
                _terms.TermsAndConditions1 = utils.Terms;
                db.TermsAndConditions.Add(_terms);
                db.SaveChanges();
            }

            else
            {
                TermsAndConditions _terms = db.TermsAndConditions.Find(utils.ID);
                _terms.TermsAndConditions1 = utils.Terms;
                db.SaveChanges();
            }
        }

        [AccessChecker(Action = 1, ModuleID = 6)]
        public ActionResult CreateUserAccess()
        {
           
            return View("UserAccess", new UserTypes());

         
        }

        [AccessChecker(Action = 1, ModuleID = 6)]
        public ActionResult UserAccess(UserTypes usertypes)
        {

            if (ModelState.IsValid)
            {

               // string msg = itemRepo.SaveItem(item);

                //if (msg == "save")
                //{
                //    TempData["message"] = "Item has been saved";
                //}
                //else if (msg == "updated")
                //{
                //    TempData["message"] = "Item has been updated";
                //}
                //else
                //{
                //    TempData["message"] = "Item already exist";
                //}
                return RedirectToAction("Index");
            }
            else
            {
                //ItemCode(item.Id);
                //InitViewBags();

                return View(usertypes);
            }


        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
