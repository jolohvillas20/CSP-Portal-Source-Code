using Microsoft.Store.PartnerCenter.Models.Offers;
using Portal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Portal.Helpers;
using Portal.Models;

namespace Portal.Controllers
{
    [AuthorizationFilter]

    public class OffersController : Controller
    {
        private MSIPortalEntities db = new MSIPortalEntities();
        OfferRepository _offerrepo = new OfferRepository();
        static List<Offer> _offer;
        //static List<Offer> _offerPerCustomer;
        static string _CustomerOffer;

        // GET: Offers
        [AccessChecker(Action = 1, ModuleID = 1)]
        public ActionResult Index(string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (Sessions.IsActive == false)
                return RedirectToAction("Login", "Home");

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
                //if (_offer == null)
                //{
                //    //List<Offer> _offer = new List<Offer>();
                //    _offer = new List<Offer>();
                //    _offer = _offerrepo.GetOffers();
                //}

                List<Offers> _offer = new List<Offers>();
                _offer = _offerrepo.GetOffers();


                IQueryable<Offers> _lstOffer = _offer.AsQueryable();

                //if (Sessions.UserType == "Sales AE")
                if (Sessions.UserType == "Sales AE")
                    _lstOffer = _lstOffer.Where(p => db.UserAccessDetails.Where(a => a.UserAccessHeader.UserID == Sessions.UserID && a.UserAccessHeader.Category == "Offers")
                                         .Select(a => a.TaggedID).Contains(p.OffersID));


                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    _lstOffer = _lstOffer.Where(s => s.Name.ToLower().Contains(searchString)
                                           || s.Description.ToLower().Contains(searchString)
                                           || s.Category.ToLower().Contains(searchString));
                }

                //if (String.IsNullOrEmpty(sortColumn))
                //{
                //    _lstCustomer = _lstCustomer.OrderByDescending(p => p.Id);
                //}
                //else
                //{
                //    _lstCustomer = _lstCustomer.OrderBy(sortColumn + " " + sortOrder);
                //}

                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(_lstOffer.ToPagedList(pageNumber, pageSize));

            }
            catch
            {
                return View("Index", "Home");
            }
        }

        [AccessChecker(Action = 2, ModuleID = 1)]
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult Refresh()
        {
            //_offer = new List<Offer>();
            //_offer = _offerrepo.GetOffers();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public JsonResult GetOffersName(string OffersName, string customerID)
        {
            try
            {
                if (OffersName == "" || OffersName == null)
                    OffersName = "";

                if (customerID == "" || customerID == null)
                    customerID = "";

                //if (_CustomerOffer != customerID)
                //{
                //    _offerPerCustomer = new List<Offer>();
                //    _offerPerCustomer = _offerrepo.GetOffersPerCustomer(customerID);

                //    _CustomerOffer = customerID;
                //}
                //else
                //{
                //    if (_offerPerCustomer == null || _offerPerCustomer.Count == 0)
                //    {
                //        _offerPerCustomer = new List<Offer>();
                //        _offerPerCustomer = _offerrepo.GetOffersPerCustomer(customerID);
                //    }
                //}

                List<Offers> _offerPerCustomer = new List<Offers>();
                _offerPerCustomer = _offerrepo.GetOffers();

                var cusName = (from c in _offerPerCustomer
                               where (c.Name.ToUpper()).Contains(OffersName.ToUpper())
                               select new { OfferName = c.Name + " (" + c.Category + ")", ID = c.OffersID.Trim().ToUpper(), SalesGroupID = c.SalesGroupId.ToUpper() })
                                  .Distinct().ToList();

                return Json(cusName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }

        }
    }
}