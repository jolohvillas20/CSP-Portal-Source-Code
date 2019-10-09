using Microsoft.Store.PartnerCenter.Models.Customers;
using Microsoft.Store.PartnerCenter.Models.Offers;
using Microsoft.Store.PartnerCenter.Models.Orders;
using Microsoft.Store.PartnerCenter.Models.Subscriptions;
using PagedList;
using Portal.Helpers;
using Portal.Models;
//using Portal.Models.DTO;
using Portal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using AttributeRouting;
using AttributeRouting.Web.Mvc;
using Portal.Models;
using Newtonsoft.Json;

namespace Portal.Controllers
{
    [AuthorizationFilter]


    [AttributeRouting.RoutePrefix("ActivityLog")]
    public class OrdersController : Controller
    {
        OrderRepository _orderRepo = new OrderRepository();
        OfferRepository _offerRepo = new OfferRepository();
        CustomerRepository _customerRepo = new CustomerRepository();
        ResellerRepository _resellerRepo = new ResellerRepository();


        Mailer _mailer = new Mailer();
        //static List<Customer> _customer;
        //static List<Order> _orders;
        static List<Order> _orderList;

        static string _user = "";

        static List<Offer> _offer;
        static List<Offer> _offerPerCustomer;
        static string _CustomerOffer;

        private MSIPortalEntities db = new MSIPortalEntities();


        [AttributeRouting.Web.Mvc.Route]
        // GET: Orders
        [AccessChecker(Action = 1, ModuleID = 1)]
        public ActionResult Index(string _customerID, string sortColumn, string sortOrder, string currentFilter, string searchString, int? page, string dateFrom, string dateTo)
        {
            if (Sessions.IsActive == false)
                return RedirectToAction("Login", "Home");
            //if (_user != Sessions.UserID.ToString())
            //{
            //    _customer = null;
            //    _user = Sessions.UserID.ToString();
            //}

            ViewBag.CurrentSort = sortColumn;
            ViewBag.SortOrder = sortOrder == "asc" ? "desc" : "asc";

            DateTime dtDateFrom = DateTime.Now.Date;
            DateTime dtDateTo = DateTime.Now;

            if (!String.IsNullOrEmpty(dateTo))
            {
                dtDateTo = Convert.ToDateTime(dateTo);
            }

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            List<CustomerOrders> _orderList = new List<CustomerOrders>();
            IQueryable<CustomerOrders> _lstOrder = _orderList.AsQueryable();

            try
            {
              

                var orderList = db.Orders.ToList();
                                //.Where(o => o.OrderStatus != "successful")

                foreach (var item in orderList)
                {
                    CustomerOrders orders = new CustomerOrders();

                    Orders order = new Orders();
                    order.ID = item.ID;
                    order.OrdersID = item.OrdersID;
                    // order.CustomersID = item.CustomersID;
                    order.MicrosoftID = item.Customers.CompanyName;
                    order.CreationDate = item.CreationDate;
                    order.BillingCycle = item.BillingCycle;
                    order.OrderStatus = item.OrderStatus;
                    
                    ////var items = db.OrderItems.Where(o => o.OrdersID == item.ID).ToList();
                    List<OrderItems> items = new List<OrderItems>();

                    orders.Orders = order;
                    orders.OrderItems = items;

                    _orderList.Add(orders);
                }



                if (!String.IsNullOrEmpty(dateFrom))
                {
                    dtDateFrom = Convert.ToDateTime(dateFrom);
                    _lstOrder = _orderList.AsQueryable().Where(p => p.Orders.CreationDate.Value.Date >= dtDateFrom.Date && p.Orders.CreationDate.Value.Date <= dtDateTo.Date);
                }
                else
                {
                    _lstOrder = _orderList.AsQueryable().Where(p => p.Orders.CreationDate.Value.Date <= dtDateTo.Date);
                }


                if (Sessions.UserType == "Sales AE")
                {
                    var __reseller = db.UserAccessDetails.Where(a => a.UserAccessHeader.UserID == Sessions.UserID).Select(a => a.TaggedID).ToList();
                    var _res = db.Orders.Where(p => __reseller.Contains(p.ResellerID.ToString())).Select(q => q.ID.ToString()).Distinct();
                    _lstOrder = _lstOrder.Where(p => _res.Contains(p.Orders.ID.ToString()));
                }
                else if (Sessions.UserType == "Reseller")
                {
                    var _reseller = db.OrderItems.Where(p => p.PartnerIdOnRecord == Sessions.ResellerMPNID).Select(q => q.Orders.ID.ToString()).ToList();
                    _lstOrder = _lstOrder.Where(p => _reseller.Contains(p.Orders.ID.ToString()));
                }

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    _lstOrder = _lstOrder.Where(s => s.Orders.MicrosoftID.ToLower().Contains(searchString)
                                           || s.OrderItems.Select(p => p.FriendlyName).ToString().ToLower().Contains(searchString)
                                           //|| s.Orders.BillingCycle.ToLower().Contains(searchString)
                                           || s.Orders.CreationDate.ToString().ToLower().Contains(searchString));
                                          

                }
               

                ViewBag.DateFrom = dtDateFrom.ToShortDateString();
                ViewBag.DateTo = dtDateTo.ToShortDateString();

                if (String.IsNullOrEmpty(sortColumn))
                {
                    _lstOrder = _lstOrder.OrderByDescending(p => p.Orders.ID);
                }
                else
                {
                    if (sortOrder == "asc")
                    {
                        if(sortColumn =="microsoftid")
                            _lstOrder = _lstOrder.OrderBy(p => p.Orders.MicrosoftID);
                        else if(sortColumn == "creationdate")
                            _lstOrder = _lstOrder.OrderBy(p => p.Orders.CreationDate);
                        else if (sortColumn == "orderstatus")
                            _lstOrder = _lstOrder.OrderBy(p => p.Orders.OrderStatus);
                    }
                    else if (sortOrder == "desc")
                    {
                        if (sortColumn == "microsoftid")
                            _lstOrder = _lstOrder.OrderByDescending(p => p.Orders.MicrosoftID);
                        else if (sortColumn == "creationdate")
                            _lstOrder = _lstOrder.OrderByDescending(p => p.Orders.CreationDate);
                        else if (sortColumn == "orderstatus")
                            _lstOrder = _lstOrder.OrderByDescending(p => p.Orders.OrderStatus);
                    }
                }

                
                //_lstOrder = _lstOrder.OrderByDescending(o => o.Orders.CreationDate);

            
                return View(_lstOrder.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception x)
            {
                _lstOrder = null;
                return View(_lstOrder.ToPagedList(pageNumber, pageSize));
            }


        }


        [AttributeRouting.Web.Mvc.Route]
        // GET: Orders/Create
        [AccessChecker(Action = 2, ModuleID = 1)]
        public ActionResult Create(int? id, string resellerid, string show,SubscriptionVM subscriptionVM)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            ViewBag.cID = id.Value;
            ViewBag.Show = show;
            ViewBag.ResellerID = resellerid;

            try
            {
                if (Sessions.UserType == "Reseller")
                {
                    ViewBag.IReseller = _resellerRepo.GetResellerSubs_ID(Convert.ToInt32(Sessions.ResellerID));
                    ViewBag.ResellerID = Sessions.ResellerID.ToString();
                    resellerid = Sessions.ResellerID.ToString();
                }
                else
                { ViewBag.IReseller = _resellerRepo.GetResellerSubs(); }
                //if (show == "show")
                //{ ViewBag.IResellerI = _resellerRepo.GetResellerSubs_ID(Convert.ToInt32(resellerid)); }
            }
            catch { ViewBag.IReseller = _resellerRepo.GetResellerSubs(); }
            //List<Catalog> catalog = new List<Models.Catalog>();
            List<_Categories> catalog = new List<Models._Categories>();

            catalog = _offerRepo.GetCatalogwithOrder(id.Value, resellerid);

            if (Sessions.UserType != "Admin")
            {
                ViewBag.RName = Sessions.ResellerName;
                ViewBag.RID = Sessions.ResellerID;
            }

            if (id.HasValue)
            {
                try
                {
                    var cName = db.Customers.Where(c => c.ID == id.Value).FirstOrDefault();
                    if (cName != null)
                    {
                        ViewBag.CName = cName.CompanyName;
                        ViewBag.CID = id.Value;
                        ViewBag.MicrosoftID = cName.MicrosoftID;
                    }
                }
                catch {}
             
            }
            //if (_customer == null)
            //{
            //    //List<Customer> _customer = new List<Customer>();
            //    _customer = new List<Customer>();
            //    _customer = _customerRepo.GetCustomers();
            //    //_customer = _customerRepo.GetCustomers(searchString, page.Value, pageSize);
            //}
            //if (_offer == null)
            //{
            //    _offer = new List<Offer>();
            //    _offer = _offerRepo.GetOffers();
            //}

            return View(catalog);
        }

        // POST: Orders/Create
        [HttpPost]
        [AccessChecker(Action = 2, ModuleID = 1)]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public string Save(ItemsOrder itm)
        {
            try
            {
                List<OrderDTO> getItems = new List<OrderDTO>();
                getItems = itm.Items;

                List<Order> orders = new List<Order>();
                int ctr = 0;
                List<OrderLineItem> _list = new List<OrderLineItem>();

                var _email = db.Resellers.FirstOrDefault(p => p.ResellerID == Sessions.ResellerID).Emails;

                int id = db.Resellers.FirstOrDefault(p => p.ResellerID == Sessions.ResellerID).AuthorizedRepresentative.Value;

                var _authorizedemail = db.AuthorizedRepresentatives.Find(id);


                //GET items per customer
                foreach (OrderDTO ord in getItems)
                {
                    var cusID = db.Resellers.Where(r => r.ResellerID == ord.ResellerID).Select(r => r.MPNID).FirstOrDefault();
                    OrderLineItem item = new OrderLineItem()
                    {
                        LineItemNumber = ctr,
                        OfferId = ord.OfferID,
                        FriendlyName = ord.FriendlyName,
                        Quantity = ord.Quantity,
                        PartnerIdOnRecord = cusID.ToString() //MPNID
                    };

                    _list.Add(item);
                    ctr++;

                    //
                    Order order = new Order();
                    order.ReferenceCustomerId = ord.CustomerID;
                    order.LineItems = _list;

                    orders.Add(order);

                }

                //CALL API
                _orderRepo.CreateOrders(orders);

                var json = new JavaScriptSerializer().Serialize(orders);

                //SAVE TO AUDIT TRAIL
                AuditTrails _audit = new AuditTrails();
                _audit.Module = "Create Order";
                _audit.TaskDesc = json;
                _audit.AuditDate = DateTime.Now;
                _audit.UserID = Sessions.UserID;
                db.AuditTrails.Add(_audit);
                db.SaveChanges();


                try
                {
                    _mailer.SendEmailToAllAdmin("New Order", "Create Order" + json, Sessions.ResellerName);
                    _mailer.SendNewOrder("New Order", "Order date" + DateTime.Now.ToString() + json, Sessions.ResellerName, _email);
                    //send to authorized representative
                    _mailer.SendNewOrder("New Order From Reseller", "Order date" + DateTime.Now.ToString() + json, Sessions.ResellerName, _authorizedemail.EmailAddress);
                }
                catch { }

                Refresh();
                //AUDIT TRAIL

                return "success";



            }
            catch (Exception x)
            {
                //throw new System.InvalidOperationException(x.Message.ToString());
                return x.Message.ToString();

            }
        }

        [AttributeRouting.Web.Mvc.Route]
        public ActionResult Refresh()
        {
            //_customer = null;
            return RedirectToAction("Index");
        }

        public ActionResult ReviewOrder()
        {
            ViewData["ReviewOrders"] = TempData["ReviewOrders"];
            ViewData["r_MicrosoftId"] = TempData["r_MicrosoftId"];
            ViewData["r_IndirectResellerId"] = TempData["r_IndirectResellerId"];

            return View("Review");
        }

        public ActionResult OrderConfirmation()
        {
            ViewData["lOrdersId"] = TempData["lOrdersId"];

            return View("Confirmation");
        }


        public string GetBillingCycle(string OfferID)

        {
            string strReturnValue = "";
            var offer = db.Offers.Where(o => o.OffersID == OfferID).FirstOrDefault();
            List<BillingCycle> lBillingFrequency = new List<BillingCycle>();

            if (offer != null)
            {

                OfferRepository offerRepo = new OfferRepository();

                var offerBillingCycle = offerRepo.GetOffer(OfferID);

                var lBillingCylcle = offerBillingCycle.SupportedBillingCycles;

                foreach (var item in lBillingCylcle)
                {
                    BillingCycle oBillingCycle = new BillingCycle();
                    oBillingCycle.billingFrequency = item.ToString();
                    lBillingFrequency.Add(oBillingCycle);
                }

            }

            return JsonConvert.SerializeObject(lBillingFrequency);
        }

        public struct BillingCycle
        {
            public string billingFrequency { get; set; }
        }

        [HttpPost]
        public string Review(CustomerSubscriptionWithIDDTO custSubs)
        {

            string strReturnValue = "Success";

            List<Models.Class.clsReviewOrder> lOrder = new List<Models.Class.clsReviewOrder>();

            if (custSubs.CustomerSubscriptions != null)
            {

                foreach (var item in custSubs.CustomerSubscriptions)
                {
                    Models.Class.clsReviewOrder oOffer = new Models.Class.clsReviewOrder();
                    oOffer.OfferId = item.OfferId;
                    oOffer.Quantity = item.LicenseCount.ToString();

                    var offer = db.Offers.Where(s => s.OffersID == item.OfferId).FirstOrDefault();

                    oOffer.OfferName = offer.Name;
                    oOffer.Category = offer.Category;
                    oOffer.UnitType = offer.UnitType;
                    oOffer.OfferShortInfo = oOffer.Category + ", " + oOffer.UnitType;
                    oOffer.Price = item.Price.ToString();


                    lOrder.Add(oOffer);
                    
                }

            }

            TempData["ReviewOrders"] = lOrder;
            TempData["r_MicrosoftId"] = custSubs.MicrosoftID;
            TempData["r_IndirectResellerId"] = custSubs.IndirectResellerID;


            return strReturnValue;
        }


        public JsonResult SaveOrder(CustomerSubscriptionWithIDDTO custSubs)
         {
            int id = 0;

           
            Models.Class.clsOrderConfirmation oOrderConfirmation = new Models.Class.clsOrderConfirmation();

            var lOrderItem = new List<Portal.Models.Class.OrderItems>();
            List<String> lErrorMsg = new List<string>();


            try
            {
                var cID = db.Customers.Where(c => c.ID.ToString() == custSubs.MicrosoftID).FirstOrDefault();

                var oAnnual = new CustomerSubscriptionWithIDDTO();
                var oMonthly = new CustomerSubscriptionWithIDDTO();
                var oOneTime = new CustomerSubscriptionWithIDDTO();
                var oNone = new CustomerSubscriptionWithIDDTO();
                var oUnknown = new CustomerSubscriptionWithIDDTO();

                var lMonthlyCustSubs = new List<CustomerSubscriptionWithIDDTO.CustomerSubscription>();
                var lAnnualCustSubs = new List<CustomerSubscriptionWithIDDTO.CustomerSubscription>();
                var lOneTimeCustSubs = new List<CustomerSubscriptionWithIDDTO.CustomerSubscription>();
                var lNoneCustSubs = new List<CustomerSubscriptionWithIDDTO.CustomerSubscription>();
                var lUnknownCustSubs = new List<CustomerSubscriptionWithIDDTO.CustomerSubscription>();

                var iCountError = 0;

                decimal fMonthlyTotalPrice = 0;
                decimal fAnnualTotalPrice = 0;
                decimal fOneTimeTotalPrice = 0;
                decimal fNoneTotalPrice = 0;
                decimal fUnknownTotalPrice = 0;

                decimal fMonthlyTotalVat = 0;
                decimal fAnnualTotalVat = 0;
                decimal fOneTimeTotalVat = 0;
                decimal fNoneTotalVat = 0;
                decimal fUnkownTotalVat = 0;

                decimal fMonthlyTotalAmount = 0;
                decimal fAnnualTotalAmount = 0;
                decimal fOneTimeTotalAmount = 0;
                decimal fNoneTotalAmount = 0;
                decimal fUnkownTotalAmount = 0;



                foreach (var item in custSubs.CustomerSubscriptions)
                {

                    var offerDetails = db.Offers.Where(c => c.OffersID == item.OfferId).FirstOrDefault();

                    if (item.BillingFrequency == "Monthly")
                    {
                        fMonthlyTotalPrice += (offerDetails.DealersPrice * item.LicenseCount);

                        lMonthlyCustSubs.Add(item);

                    }
                    else if (item.BillingFrequency == "Annual")
                    {
                        fAnnualTotalPrice += (offerDetails.DealersPrice * item.LicenseCount);

                        lAnnualCustSubs.Add(item);
                    }else if (item.BillingFrequency == "One Time")
                    {
                        fOneTimeTotalPrice += (offerDetails.DealersPrice * item.LicenseCount);
                        lOneTimeCustSubs.Add(item);
                    }else if (item.BillingFrequency == "None")
                    {
                        fNoneTotalPrice += (offerDetails.DealersPrice * item.LicenseCount);
                        lNoneCustSubs.Add(item);
                    }
                    else
                    {
                        fUnknownTotalPrice += (offerDetails.DealersPrice * item.LicenseCount);
                        lUnknownCustSubs.Add(item);
                    }

                }

                if (lMonthlyCustSubs.Count != 0)
                {
                    fMonthlyTotalVat = decimal.Parse((double.Parse(fMonthlyTotalPrice.ToString()) * 0.12).ToString());
                    fMonthlyTotalAmount = fMonthlyTotalPrice + fMonthlyTotalVat;

                    oMonthly.Billing = custSubs.Billing;
                    oMonthly.IndirectResellerID = custSubs.IndirectResellerID;
                    oMonthly.MicrosoftID = custSubs.MicrosoftID;
                    oMonthly.ResellerID = custSubs.ResellerID;
                    oMonthly.TotalAmount = fMonthlyTotalAmount;
                    oMonthly.TotalPrice = fMonthlyTotalPrice;
                    oMonthly.TotalVat = fMonthlyTotalVat;
                    oMonthly.CustomerSubscriptions = lMonthlyCustSubs;
                    oMonthly.BillingCylcle = "Monthly";
                }

                if (lAnnualCustSubs.Count != 0)
                {

                    fAnnualTotalVat = decimal.Parse((double.Parse(fAnnualTotalPrice.ToString()) * 0.12).ToString());
                    fAnnualTotalAmount = fAnnualTotalPrice + fAnnualTotalVat;

                    oAnnual.Billing = custSubs.Billing;
                    oAnnual.IndirectResellerID = custSubs.IndirectResellerID;
                    oAnnual.MicrosoftID = custSubs.MicrosoftID;
                    oAnnual.ResellerID = custSubs.ResellerID;
                    oAnnual.TotalAmount = fAnnualTotalAmount;
                    oAnnual.TotalPrice = fAnnualTotalPrice;
                    oAnnual.TotalVat = fAnnualTotalVat;
                    oAnnual.CustomerSubscriptions = lAnnualCustSubs;
                    oAnnual.BillingCylcle = "Annual";
                }

                if (lOneTimeCustSubs.Count != 0)
                {
                    fOneTimeTotalVat = decimal.Parse((double.Parse(fOneTimeTotalPrice.ToString()) * 0.12).ToString());
                    fOneTimeTotalAmount = fOneTimeTotalPrice + fOneTimeTotalVat;

                    oOneTime.Billing = custSubs.Billing;
                    oOneTime.IndirectResellerID = custSubs.IndirectResellerID;
                    oOneTime.MicrosoftID = custSubs.MicrosoftID;
                    oOneTime.ResellerID = custSubs.ResellerID;
                    oOneTime.TotalAmount = fOneTimeTotalAmount;
                    oOneTime.TotalPrice = fOneTimeTotalPrice;
                    oOneTime.TotalVat = fOneTimeTotalVat;
                    oOneTime.CustomerSubscriptions = lOneTimeCustSubs;
                    oOneTime.BillingCylcle = "OneTime";

                }

                if (lNoneCustSubs.Count != 0)
                {
                    fNoneTotalVat = decimal.Parse((double.Parse(fNoneTotalPrice.ToString()) * 0.12).ToString());
                    fNoneTotalAmount = fNoneTotalPrice + fNoneTotalVat;

                    oOneTime.Billing = custSubs.Billing;
                    oOneTime.IndirectResellerID = custSubs.IndirectResellerID;
                    oOneTime.MicrosoftID = custSubs.MicrosoftID;
                    oOneTime.ResellerID = custSubs.ResellerID;
                    oOneTime.TotalAmount = fNoneTotalAmount;
                    oOneTime.TotalPrice = fNoneTotalPrice;
                    oOneTime.TotalVat = fNoneTotalVat;
                    oOneTime.CustomerSubscriptions = lNoneCustSubs;
                    oOneTime.BillingCylcle = "None";
                }

                if (lUnknownCustSubs.Count != 0)
                {
                    fUnkownTotalVat = decimal.Parse((double.Parse(fUnknownTotalPrice.ToString()) * 0.12).ToString());
                    fUnkownTotalAmount = fUnknownTotalPrice + fUnkownTotalVat;

                    oOneTime.Billing = custSubs.Billing;
                    oOneTime.IndirectResellerID = custSubs.IndirectResellerID;
                    oOneTime.MicrosoftID = custSubs.MicrosoftID;
                    oOneTime.ResellerID = custSubs.ResellerID;
                    oOneTime.TotalAmount = fUnkownTotalAmount;
                    oOneTime.TotalPrice = fUnknownTotalPrice;
                    oOneTime.TotalVat = fUnkownTotalVat;
                    oOneTime.CustomerSubscriptions = lUnknownCustSubs;
                    oOneTime.BillingCylcle = "Unknown";
                }

                string strOrdersId = "";
                string strOrderDate = DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt");

                

                //Monthly Billing Order Items
                if (oMonthly.CustomerSubscriptions != null)
                {
                    var listItemErrorMessage = new List<string>();

                    CreateOrder(cID, oMonthly, out id, out strOrdersId, out listItemErrorMessage);

                    foreach (var item in listItemErrorMessage)
                    {
                        lErrorMsg.Add(item);
                        iCountError++;
                    }   


                    if ((strOrdersId != string.Empty || strOrdersId != null) && listItemErrorMessage.Count <= 0)
                    {
                        var oOrderItem = new Portal.Models.Class.OrderItems();

                        var lOrderLineItem = new List<Portal.Models.Class.OrderItems.cLineItems>();

                        var result = from a in db.OrderItems
                                     join b in db.Orders on a.OrdersID equals b.ID
                                     join c in db.Offers on a.OffersID equals c.OffersID
                                     where b.OrdersID == strOrdersId
                                     select new { c.Name, a.Quantity, c.Category, c.UnitType };

                        foreach (var item in result)
                        {
                            var cLineItem = new Portal.Models.Class.OrderItems.cLineItems();

                            cLineItem.OfferName = item.Name;
                            cLineItem.Quantity = item.Quantity.ToString();
                            cLineItem.Category = item.Category;
                            cLineItem.UnitType = item.UnitType;

                            lOrderLineItem.Add(cLineItem);
                        }

                        oOrderItem.OrderId = strOrdersId;
                        oOrderItem.LineItems = lOrderLineItem;


                        lOrderItem.Add(oOrderItem);
                    }

                 

                }

                //Annual Billing Order Items
                if (oAnnual.CustomerSubscriptions != null)
                {
                    List<string> listItemErrorMessage = new List<string>();
                    CreateOrder(cID, oAnnual, out id,out strOrdersId, out listItemErrorMessage);
                    foreach (var item in listItemErrorMessage)
                    {
                        lErrorMsg.Add(item);
                        iCountError++;
                    }
                    if ((strOrdersId != string.Empty || strOrdersId != null) && listItemErrorMessage.Count <= 0)
                    {
                        var oOrderItem = new Portal.Models.Class.OrderItems();

                        var lOrderLineItem = new List<Portal.Models.Class.OrderItems.cLineItems>();

                        var result = from a in db.OrderItems
                                     join b in db.Orders on a.OrdersID equals b.ID
                                     join c in db.Offers on a.OffersID equals c.OffersID
                                     where b.OrdersID == strOrdersId
                                     select new { c.Name, a.Quantity, c.Category, c.UnitType };

                        foreach (var item in result)
                        {
                            var cLineItem = new Portal.Models.Class.OrderItems.cLineItems();

                            cLineItem.OfferName = item.Name;
                            cLineItem.Quantity = item.Quantity.ToString();
                            cLineItem.Category = item.Category;
                            cLineItem.UnitType = item.UnitType;

                            lOrderLineItem.Add(cLineItem);
                        }

                        oOrderItem.OrderId = strOrdersId;
                        oOrderItem.LineItems = lOrderLineItem;


                        lOrderItem.Add(oOrderItem);
                    }
                }

                //OneTime Billing Order Items
                if (oOneTime.CustomerSubscriptions != null)
                {
                    List<string> listItemErrorMessage = new List<string>();
                    CreateOrder(cID, oOneTime, out id,out strOrdersId, out listItemErrorMessage);
                    foreach (var item in listItemErrorMessage)
                    {
                        lErrorMsg.Add(item);
                        iCountError++;
                    }
                    if ((strOrdersId != string.Empty || strOrdersId != null) && listItemErrorMessage.Count <= 0)
                    {
                        var oOrderItem = new Portal.Models.Class.OrderItems();

                        var lOrderLineItem = new List<Portal.Models.Class.OrderItems.cLineItems>();

                        var result = from a in db.OrderItems
                                     join b in db.Orders on a.OrdersID equals b.ID
                                     join c in db.Offers on a.OffersID equals c.OffersID
                                     where b.OrdersID == strOrdersId
                                     select new { c.Name, a.Quantity, c.Category, c.UnitType };

                        foreach (var item in result)
                        {
                            var cLineItem = new Portal.Models.Class.OrderItems.cLineItems();

                            cLineItem.OfferName = item.Name;
                            cLineItem.Quantity = item.Quantity.ToString();
                            cLineItem.Category = item.Category;
                            cLineItem.UnitType = item.UnitType;

                            lOrderLineItem.Add(cLineItem);
                        }

                        oOrderItem.OrderId = strOrdersId;
                        oOrderItem.LineItems = lOrderLineItem;


                        lOrderItem.Add(oOrderItem);
                    }
                }

                //None Billing Order Items
                if (oNone.CustomerSubscriptions != null)
                {
                    List<string> listItemErrorMessage = new List<string>();
                    CreateOrder(cID, oNone, out id, out strOrdersId, out listItemErrorMessage);
                    foreach (var item in listItemErrorMessage)
                    {
                        lErrorMsg.Add(item);
                        iCountError++;
                    }
                    if ((strOrdersId != string.Empty || strOrdersId != null) && listItemErrorMessage.Count <= 0)
                    {
                        var oOrderItem = new Portal.Models.Class.OrderItems();

                        var lOrderLineItem = new List<Portal.Models.Class.OrderItems.cLineItems>();

                        var result = from a in db.OrderItems
                                     join b in db.Orders on a.OrdersID equals b.ID
                                     join c in db.Offers on a.OffersID equals c.OffersID
                                     where b.OrdersID == strOrdersId
                                     select new { c.Name, a.Quantity, c.Category, c.UnitType };

                        foreach (var item in result)
                        {
                            var cLineItem = new Portal.Models.Class.OrderItems.cLineItems();

                            cLineItem.OfferName = item.Name;
                            cLineItem.Quantity = item.Quantity.ToString();
                            cLineItem.Category = item.Category;
                            cLineItem.UnitType = item.UnitType;

                            lOrderLineItem.Add(cLineItem);
                        }

                        oOrderItem.OrderId = strOrdersId;
                        oOrderItem.LineItems = lOrderLineItem;


                        lOrderItem.Add(oOrderItem);
                    }
                }

                //Unknown Billing Order Items
                if (oUnknown.CustomerSubscriptions != null)
                {
                    List<string> listItemErrorMessage = new List<string>();
                    CreateOrder(cID, oUnknown, out id,out strOrdersId, out listItemErrorMessage);
                    foreach (var item in listItemErrorMessage)
                    {
                        lErrorMsg.Add(item);
                        iCountError++;
                    }

                    if ((strOrdersId != string.Empty || strOrdersId != null) && listItemErrorMessage.Count <= 0)
                    {
                        var oOrderItem = new Portal.Models.Class.OrderItems();

                        var lOrderLineItem = new List<Portal.Models.Class.OrderItems.cLineItems>();

                        var result = from a in db.OrderItems
                                     join b in db.Orders on a.OrdersID equals b.ID
                                     join c in db.Offers on a.OffersID equals c.OffersID
                                     where b.OrdersID == strOrdersId
                                     select new { c.Name, a.Quantity, c.Category, c.UnitType };

                        foreach (var item in result)
                        {
                            var cLineItem = new Portal.Models.Class.OrderItems.cLineItems();

                            cLineItem.OfferName = item.Name;
                            cLineItem.Quantity = item.Quantity.ToString();
                            cLineItem.Category = item.Category;
                            cLineItem.UnitType = item.UnitType;

                            lOrderLineItem.Add(cLineItem);
                        }

                        oOrderItem.OrderId = strOrdersId;
                        oOrderItem.LineItems = lOrderLineItem;


                        lOrderItem.Add(oOrderItem);
                    }
                }

           

                var Reseller = db.Resellers.Where(c => c.ResellerID == custSubs.IndirectResellerID).FirstOrDefault();

                oOrderConfirmation.CustomerId = cID.MicrosoftID;
                oOrderConfirmation.CustomerName = cID.FirstName + " " + cID.LastName;
                oOrderConfirmation.OrderProcessed = lOrderItem.Count().ToString();
                oOrderConfirmation.lOrderId = lOrderItem;
                oOrderConfirmation.IndirectReseller = Reseller.ResellerName;
                oOrderConfirmation.OrderDate = strOrderDate;
                oOrderConfirmation.Failed = iCountError.ToString();

                Console.WriteLine(oOrderConfirmation);

            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var oConfirmation = new Models.Class.clsReviewOrder();

            TempData["lOrdersId"] = oOrderConfirmation;

            if (lErrorMsg.Count >= 1)
            {
                return Json(lErrorMsg, JsonRequestBehavior.AllowGet);
            }

            return Json(id, JsonRequestBehavior.AllowGet);


        }

        private void CreateOrder(Customers cID,CustomerSubscriptionWithIDDTO custSubs,out int return_id,out string strOrdersId,out List<string> lErrorMessage)
        {

            strOrdersId = null;
            lErrorMessage = null;
            

            return_id = 0;
            Customers customer = new Customers();
            customer.MicrosoftID = cID.MicrosoftID;
            customer.IndirectResellerID = custSubs.IndirectResellerID;


            List<OrderItemsDTO.OItems> _items = new List<OrderItemsDTO.OItems>();


            OrderItemsDTO order = new OrderItemsDTO();
            order.OrdersID = cID.ID.ToString();
            order.CustomerID = cID.ID;
            order.CreationDate = DateTime.Now;
            order.TotalPrice = custSubs.TotalPrice;
            order.TotalVat = custSubs.TotalVat;
            order.TotalAmount = custSubs.TotalAmount;
            order.ResellerID = custSubs.IndirectResellerID;
             order.BillingCycle = custSubs.BillingCylcle;

            /* July 6, 2018
               * request for adding billing cycle in order
               * currently for phase 2

              */


            int ctr = 1;

            foreach (var item in custSubs.CustomerSubscriptions)
            {
                OrderItemsDTO.OItems oItems = new OrderItemsDTO.OItems();
                oItems.OrdersID = order.ID;
                oItems.LineItemNumber = ctr;
                oItems.OffersId = item.OfferId;
                oItems.Quantity = item.LicenseCount;
                oItems.Price = item.Price;

                if (custSubs.IndirectResellerID != 0)
                {
                    string mpnid = null;
                    mpnid = db.Resellers.Find(custSubs.IndirectResellerID).MPNID.ToString();

                    oItems.PartnerIdOnRecord = mpnid;
                }


                _items.Add(oItems);
                ctr++;
            }
            order.OItem = _items;


            //SAVE TO ORDER TABLE

            try
            {
                return_id = _orderRepo.SaveOrder_sub(order,out strOrdersId,out lErrorMessage);

                //if (strErrorMessage != null)
                //{
                //    RedirectToAction("Error");
                //}

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }




            // id = SaveShipping(book, "SHIP FREIGHT");
        }

        public ActionResult Error()
        {
            return View();
        }

        [AttributeRouting.Web.Mvc.Route]
        [AccessChecker(Action = 1, ModuleID = 1)]
        public ActionResult Details(int ID)
        {
            try
            {
                var details = db.OrderItems.Where(o => o.OrdersID == ID).ToList();
                return View(details);

            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
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

                if (_CustomerOffer != customerID)
                {
                    _offerPerCustomer = new List<Offer>();
                    //_offerPerCustomer = _offerRepo.GetOffersPerCustomer(customerID);
                    _CustomerOffer = customerID;
                }
                else
                {
                    if (_offerPerCustomer == null || _offerPerCustomer.Count == 0)
                    {
                        _offerPerCustomer = new List<Offer>();
                        //_offerPerCustomer = _offerRepo.GetOffersPerCustomer(customerID);
                    }
                }

                var cusName = (from c in _offerPerCustomer
                               where (c.Name.ToUpper()).Contains(OffersName.ToUpper())
                               select new { OfferName = c.Name + " (" + c.Category.Name + ")", ID = c.Id.Trim().ToUpper(), SalesGroupID = c.SalesGroupId.ToUpper() })
                                  .Distinct().ToList();

                return Json(cusName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }

        }

        public JsonResult CheckIfVatable(string resellerID)
        {
            bool vat = false;

            try
            {
                var check = db.Resellers.Where(r => r.MPNID.ToString() == resellerID).FirstOrDefault();
                vat = check.IsVatExempted;

                return Json(vat, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json("error", JsonRequestBehavior.AllowGet);

            }

        }


    }
}
