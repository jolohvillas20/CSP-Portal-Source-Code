using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
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
using Microsoft.Store.PartnerCenter.Models.Subscriptions;
using Microsoft.Store.PartnerCenter.Exceptions;
using Newtonsoft.Json;
using Portal.Models.Class;

namespace Portal.Controllers
{
    [AuthorizationFilter]
    public class CustomersController : Controller
    {
        //private ApplicationDbContext db = new ApplicationDbContext();
        private MSIPortalEntities db = new MSIPortalEntities();

        Mailer _mailer = new Mailer();
        CustomerRepository _customerRepo = new CustomerRepository();
        ResellerRepository _resellerRepo = new ResellerRepository();
        SubscriptionsRepository _subcriptionRepo = new SubscriptionsRepository();
        OrderRepository _orderRepo = new OrderRepository();
        OfferRepository _offerRepo = new OfferRepository();

        //public static List<Customer> _customer;
        static string _Session = "";


        // GET: Customers
        [AccessChecker(Action = 1, ModuleID = 1)]
        public ActionResult Index(string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
        {
            //List<string> lst = new List<string>();

            //lst.Add("82231f81-0954-4d9e-983b-d6957eef0937");
            //lst.Add("3d41b34b-3e44-4c8e-be30-ae91f11b9ab2");
            //lst.Add("7c173d3a-da70-4d42-8524-c70a2fc92bb5");
            //lst.Add("c965f581-b930-464c-aa6d-d491f5a00e6d");
            //lst.Add("dda2b2aa-d04f-45c8-b2ba-2b6fd024f0c4");
            //lst.Add("00e140bb-d833-4a29-8a0a-38af3724aad0");
            //lst.Add("7f44df3a-5776-4aa5-8c6e-e98033a40343");
            //lst.Add("840e5a5e-f842-49cd-a0ad-7ee16672aca6");
            //lst.Add("3d602492-c422-42f1-a045-ee5f7e92ffe6");
            //lst.Add("1e6b8610-2aa0-4bc2-b10b-619a959fdd27");
            //lst.Add("a9958799-5d30-472d-8c14-6ecc28f2a277");
            //lst.Add("88c448f8-6583-4ca3-a93d-c6478802dacb");
            //lst.Add("d9fd2b6c-132e-449d-94aa-cb667552a320");
            //lst.Add("76c8f7d7-9348-45c6-bfee-dc8e1e4721d4");
            //lst.Add("4cc732e8-2876-42e9-a4fc-dd6343195680");
            //lst.Add("1d116653-4b32-4046-a4af-448168855bec");
            //lst.Add("24d0fdcc-84dc-4bef-8457-4d2ded044da3");
            //lst.Add("47bf7a73-4a55-4259-bc3c-6b231e8646d6");
            //lst.Add("808dc9d0-4c84-43e0-a0cb-55e58bc619c0");
            //lst.Add("a60a645b-125d-4a0f-a1bd-5be4284ca2f4");
            //lst.Add("8e19220f-df1c-4117-bf23-235fa1026016");
            //lst.Add("4c8234bb-260a-4e07-9b5e-67a331948c33");
            //lst.Add("76f38bce-0bda-4ec3-962f-ffd04147d31a");
            //lst.Add("10d3ef72-f593-4562-b507-67ce299611a3");
            //lst.Add("04a86009-3a2f-4d92-a0f5-02ecf45f3e73");
            //lst.Add("7ad9b18b-0e78-485e-9697-acae9741562f");
            //lst.Add("538b9d29-92a6-4bf2-8d33-92b6af13871c");
            //lst.Add("45eecd20-cc9d-4ae9-ad22-78f7699e0ac9");
            //lst.Add("486d79a6-3306-4529-af01-5788df60391e");
            //lst.Add("672c43f8-1234-4643-896d-2863025a691d");
            //lst.Add("193946d3-027f-47a7-9c44-f61daab64687");
            //lst.Add("9a8476f1-fb49-4a05-91e7-e4a56cc8c700");
            //lst.Add("7f487989-6c2a-4cd1-8d96-0a7709a794b5");
            //lst.Add("3fc35277-8d44-41d8-a2ed-52a75584d525");
            //foreach (var l in lst)
            //{
            //    try
            //    {
            //        _customerRepo.DeleteCustomer(l);
            //    }
            //    catch { }
            //}
            if (Sessions.IsActive == false)
                return RedirectToAction("Login", "Home");
            //if (_user != Sessions.UserID.ToString())
            //{
            //    _customer = null;
            //    _user = Sessions.UserID.ToString();
            //}
            sortColumn = "DomainName";

            ViewBag.CurrentSort = sortColumn;
            ViewBag.SortOrder = sortOrder == "asc" ? "desc" : "asc";
            int pageSize = 10;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }


            ViewBag.CurrentFilter = searchString;

            List<Customers> _customers = new List<Customers>();
            _customers = _customerRepo.GetCustomers();

            IQueryable<Customers> _lstCustomer = _customers.AsQueryable();

            int pageNumber = (page ?? 1);

            try
            {
                         
                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    _lstCustomer = _lstCustomer.Where(s => s.CompanyName.ToLower().Contains(searchString)
                                           || s.DomainName.ToLower().Contains(searchString)
                                           || s.MicrosoftID.ToString().ToLower().Contains(searchString));
                }


             
                if (String.IsNullOrEmpty(sortColumn))
                {
                    _lstCustomer = _lstCustomer.OrderByDescending(p => p.ID);
                }
                else
                {
                    //_lstCustomer = _lstCustomer.OrderBy(sortColumn + " " + sortOrder);
                    _lstCustomer = _lstCustomer.OrderBy(sortColumn);

                }

                if (Sessions.UserType == "Sales AE")
                {
                    var __reseller = db.UserAccessDetails.Where(a => a.UserAccessHeader.UserID == Sessions.UserID).Select(a => a.TaggedID);
                    var _res = db.Orders.Where(p => __reseller.Contains(p.ResellerID.ToString())).Select(q => q.CustomersID.ToString()).Distinct();
                    _lstCustomer = _lstCustomer.Where(p => _res.Contains(p.ID.ToString()));
                }
                else if (Sessions.UserType == "Reseller")
                {
                    List<int> _resellerid = db.Resellers.Where(r => r.MPNID.ToString() == Sessions.ResellerMPNID).Select(r => r.ResellerID).ToList();

                    var _reseller = db.Orders.Where(p => _resellerid.Contains(p.ResellerID)).Select(q => q.CustomersID.ToString());
                    var _reseller1 = db.Customers.Where(p => p.AddedBy == Sessions.UserID).Select(q => q.ID.ToString());
                    var _reseller2 = db.Subscriptions.Where(p => p.PartnerId.Contains(Sessions.ResellerMPNID)).Select(p => p.Customers.ID.ToString());
                    var result = (from p in _reseller select p).Union(from q in _reseller1 select q);
                    var result1 = (from p in result select p).Union(from q in _reseller2 select q);

                    _lstCustomer = _lstCustomer.Where(p => result1.Contains(p.ID.ToString()));

                }
             
                return View(_lstCustomer.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception x)
            {
                _lstCustomer = null;
                return View(_lstCustomer.ToPagedList(pageNumber, pageSize));
            }

        }

        public ActionResult DeleteCustomer()
        {
            string message = "";
            var _x = db.Customers.Select(p => p.MicrosoftID);
            foreach (var a in _x)
            {
                try
                {
                    _customerRepo.DeleteCustomer(a);

                    Customers cus = db.Customers.FirstOrDefault(c => c.MicrosoftID == a);
                    db.Customers.Remove(cus);
                    db.SaveChanges();
                }
                catch(Exception e)
                {
                    message += "error in deleting customer in partner center reference Microsoft ID:  " 
                                   + a.ToString() + "    " + e.Message;
                }
            }
            ViewBag.Msg = "complete"+ Environment.NewLine + message;
            return View();
        }

        public ActionResult Refresh()
        {
            //_customer = new List<Customer>();
            //_customer = _customerRepo.GetCustomers();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult getSpecialQua()
        {
            Session["Change"] = "change";
            _Session = Session["Change"].ToString();
            return Json("", JsonRequestBehavior.AllowGet);
        }
        // GET: Customers/Create
        [AccessChecker(Action = 2, ModuleID = 1)]
        public ActionResult Create()
        {
            try
            {
                if (Session["Create"].ToString() == "done")
                {
                    ViewBag.Msg = "Successfully Created Customer";
                    Session["Create"] = "";
                }
            }
            catch { }
            ViewBag.SpecialQualificationsID = new SelectList(db.SpecialQualifications, "ID", "Name");
            ViewBag.IndirectResellerID = GetReseller();
            if (Sessions.UserType == "Reseller")
                ViewBag.IReseller = _resellerRepo.GetResellerSubs_ID(Convert.ToInt32(Sessions.ResellerID));
            else
            { ViewBag.IReseller = _resellerRepo.GetResellerSubs(); }


            List<Catalog> catalog = new List<Models.Catalog>();

            int x = ViewBag.SpecialQ != null ? Convert.ToInt32(ViewBag.SpecialQ) : 0;
           catalog = _offerRepo.GetCatalog(x);

            CustomerOffers cus = new Models.CustomerOffers();
            cus.Customers = new Customers();
            cus.Catalog = catalog;

            return View(cus);
        }

        [HttpPost]
        public ActionResult GetPagedCustomer()
        {
            var customers = _customerRepo.GetPageCustomers();

            try
            {
                foreach (var item in customers.Items)
                {
                    var customer = db.Customers.Where(c => c.MicrosoftID == item.Id).FirstOrDefault();

                    if (customer != null)
                    {
                        customer.MicrosoftID = item.Id;
                        //var customerSpecialQualifications = _customerRepo.GetCustomerQualification(item.Id);

                        //var tblcustomerSpecialQualification = db.SpecialQualifications.Where(s => s.Name == customerSpecialQualifications.ToString()).FirstOrDefault();
                        customer.SpecialQualificationsID = 1;

                        var customerAgreement = _customerRepo.GetCustomerAgreements(item.Id);

                        string strDateAgreed = "1/1/2000";
                        string strPhoneNumber = "0";

                        if (customer.PhoneNumber == "" || customer.PhoneNumber == null)
                        {
                            customer.PhoneNumber = strPhoneNumber;
                        }

                        if (customerAgreement.TotalCount >= 0)
                        {
                            customer.AgreedToMCA = true;
                        }
                        else
                        {
                            customer.AgreedToMCA = false;
                        }

                        if (customerAgreement.TotalCount >= 1)
                        {
                            strDateAgreed = customerAgreement.Items.LastOrDefault().DateAgreed.ToString();
                        }

                        customer.AgreementDate = strDateAgreed;
                        customer.DomainName = customer.DomainName.Replace(".onmicrosoft.com","");


                        db.SaveChanges();
                    }
                    else
                    {
                     

                        try
                        {
                            var newCustomer = new Customers();
                            newCustomer.MicrosoftID = item.Id;
                            newCustomer.CompanyName = item.CompanyProfile.CompanyName;
                            newCustomer.DomainName = item.CompanyProfile.Domain.Replace(".onmicrosoft.com", "");
                            //newCustomer.DomainName = "wordtext";


                            //var customerDetails = _customerRepo.GetCustomerDetails(item.Id)
                            //var customerSpecialQualifications = _customerRepo.GetCustomerQualification(item.Id);

                            //var tblcustomerSpecialQualification = db.SpecialQualifications.Where(s => s.Name == customerSpecialQualifications.ToString()).FirstOrDefault();
                            //newCustomer.SpecialQualificationsID = tblcustomerSpecialQualification.ID;

                            string strLastName = "--";
                            string strFirstName = "--";
                            string strCountry = "--";
                            string strAddress1 = "--";
                            string strCity = "--'";
                            string strPostalCode = "--";
                            string strPhoneNumber = "0";
                            string strEmailAddress = "default@wsiphil.com.ph";


                            //if (customerDetails != null)
                            //{
                            //    strLastName = customerDetails.BillingProfile.DefaultAddress.LastName;
                            //    strFirstName = customerDetails.BillingProfile.DefaultAddress.FirstName;
                            //    strCountry = customerDetails.BillingProfile.DefaultAddress.Country;
                            //    strAddress1 = customerDetails.BillingProfile.DefaultAddress.AddressLine1;
                            //    strCity = customerDetails.BillingProfile.DefaultAddress.City;
                            //    strPostalCode = customerDetails.BillingProfile.DefaultAddress.PostalCode;
                            //    strPhoneNumber = customerDetails.BillingProfile.DefaultAddress.PhoneNumber;
                            //    strEmailAddress = customerDetails.BillingProfile.Email;
                            //}

                            var customerAgreement = _customerRepo.GetCustomerAgreements(item.Id);

                            newCustomer.LastName = strLastName;
                            newCustomer.FirstName = strFirstName;
                            newCustomer.Country = strCountry;
                            newCustomer.AddressLine1 = strAddress1;
                            newCustomer.City = strCity;
                            newCustomer.ZipPostal = strPostalCode;
                            newCustomer.PhoneNumber = strPhoneNumber;
                            newCustomer.EmailAddress = strEmailAddress;
                            newCustomer.SpecialQualificationsID = 1;



                            newCustomer.AgreementDate = "01/01/2000";
                            newCustomer.AgreedToMCA = false;

                            db = new MSIPortalEntities();
                            db.Customers.Add(newCustomer);
                            db.SaveChanges();
                        }
                        catch (Exception ex )
                        {
                            Console.WriteLine(ex.Message);
                        }

                      
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

           

            return View();
        }


     
        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessChecker(Action = 2, ModuleID = 1)]
        public ActionResult Create(Customers _customer, Catalog _catalog, CustomerOffers _offer)
        {
            Customer customerToCreate = new Customer();
            Customer customerCreated = new Customer();

           
            if (_customer.SpecialQualificationsID == 2)
            {
                ViewBag.SpecialQ = _customer.SpecialQualificationsID;
            }
            if (_Session != "change")
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        CustomerCompanyProfile CompanyProfile = new CustomerCompanyProfile();
                        CompanyProfile.Domain = _customer.DomainName;

                      
                    Address DefaultAddress = new Address();
                    DefaultAddress.FirstName = _customer.FirstName;
                    DefaultAddress.LastName = _customer.LastName;
                    DefaultAddress.AddressLine1 = _customer.AddressLine1;
                    DefaultAddress.City = _customer.City;
                    DefaultAddress.State = _customer.State;
                    DefaultAddress.Country = "PH";
                    DefaultAddress.PostalCode = _customer.ZipPostal;
                    DefaultAddress.PhoneNumber = _customer.PhoneNumber;

                    CustomerBillingProfile BillingProfile = new CustomerBillingProfile();
                    BillingProfile.Culture = "EN-PH";
                    BillingProfile.Email = _customer.EmailAddress;
                    BillingProfile.Language = "En";
                    BillingProfile.CompanyName = _customer.CompanyName;
                    BillingProfile.DefaultAddress = DefaultAddress;

                    //UserCredentials oUserCredentials = new UserCredentials();
                    //oUserCredentials.UserName = "rjbdc";
                    //oUserCredentials.Password = "123456";


                    customerToCreate.BillingProfile = BillingProfile;
                    customerToCreate.CompanyProfile = CompanyProfile;
                    //customerToCreate.UserCredentials = oUserCredentials;
                       

                    customerCreated = _customerRepo.CreateCustomer(customerToCreate, _customer.SpecialQualificationsID);


                    var json = new JavaScriptSerializer().Serialize(customerCreated);

                    //SAVE TO SDS DATABASE
                    Customers _cus = new Customers();
                    _cus = _customer;
                    _cus.DomainName = _customer.DomainName.Replace(".onmicrosoft.com", "");
                     try
                     { _cus.MicrosoftID = customerCreated.Id; }
                     catch { _cus.MicrosoftID = ""; }                  
                    _cus.AddedBy = Sessions.UserID;
                        _cus.Username = customerCreated.UserCredentials.UserName;
                        _cus.Password = customerCreated.UserCredentials.Password;
                    db.Customers.Add(_cus);

                        //SAVE TO AUDIT TRAIL
                     AuditTrails _audit = new AuditTrails();
                    _audit.Module = "Create Customer";
                    _audit.TaskDesc = json;
                    _audit.AuditDate = DateTime.Now;
                    _audit.UserID = Sessions.UserID;
                    db.AuditTrails.Add(_audit);
                    db.SaveChanges();

                        try
                        {


                            if (Sessions.ResellerID > 0)
                            {
                                var _email = db.Resellers.FirstOrDefault(p => p.ResellerID == Sessions.ResellerID).Emails;

                                int id = db.Resellers.FirstOrDefault(p => p.ResellerID == Sessions.ResellerID).AuthorizedRepresentative.Value;

                                var _authorizedemail = db.AuthorizedRepresentatives.Find(id);

                                _mailer.SendEmailToAllAdmin("Customer", "Created Customer: " + _customer.FirstName + " " + _customer.LastName, Sessions.ResellerName);
                                _mailer.SendNewCustomer("New Customer", "Created date" + DateTime.Now.ToString() + _customer.FirstName + " " + _customer.LastName, Sessions.ResellerName, _email);
                                _mailer.SendNewCustomer("New Customer Created by Reseller", "Created date" + DateTime.Now.ToString() + _customer.FirstName + " " + _customer.LastName, Sessions.ResellerName, _authorizedemail.EmailAddress);
                            }

                            //_mailer.SendCustomerCredentials(_customer.EmailAddress, _customer.FirstName + " " + _customer.LastName, customerCreated.UserCredentials.UserName, customerCreated.UserCredentials.Password);
                        }
                        catch (Exception e) { string msg = e.Message; }
                  

                    }
                    catch (Exception x)
                    {
                        string test = x.Message.ToString();

                        ViewBag.SpecialQualificationsID = new SelectList(db.SpecialQualifications, "ID", "Name", _customer.SpecialQualificationsID);
                        ViewBag.IndirectResellerID = GetReseller();
                       
                        if (Sessions.UserType == "Reseller")
                            ViewBag.IReseller = _resellerRepo.GetResellerSubs_ID(Convert.ToInt32(Sessions.ResellerID));
                        else
                        { ViewBag.IReseller = _resellerRepo.GetResellerSubs(); }

                        List<Catalog> catalog2 = new List<Models.Catalog>();

                        catalog2 = _offerRepo.GetCatalog(_customer.SpecialQualificationsID);

                        CustomerOffers cus2 = new Models.CustomerOffers();
                        cus2.Customers = new Customers();
                        cus2.Catalog = catalog2;
                        ModelState.AddModelError("", x.Message.ToString());
                        return View(cus2);
                        // return View(_customer);
                    }


                    Session["Create"] = "done";
                    return RedirectToAction("Create");
                    //return View(cus1);
                }
            }
            else
            {
                Session["Change"] = "";
                _Session = "";
            }
            ViewBag.SpecialQualificationsID = new SelectList(db.SpecialQualifications, "ID", "Name", _customer.SpecialQualificationsID);
            ViewBag.IndirectResellerID = GetReseller();

            if (Sessions.UserType == "Reseller")
                ViewBag.IReseller = _resellerRepo.GetResellerSubs_ID(Convert.ToInt32(Sessions.ResellerID));
            else
            { ViewBag.IReseller = _resellerRepo.GetResellerSubs(); }

            List<Catalog> catalog = new List<Models.Catalog>();
           
            catalog = _offerRepo.GetCatalog(_customer.SpecialQualificationsID);

            CustomerOffers cus = new Models.CustomerOffers();
            cus.Customers = _customer;
            cus.Catalog = catalog;

            return View(cus);
        }

        [AccessChecker(Action = 1, ModuleID = 1)]
        public ActionResult Details(string MicrosoftID, int Id)
        {
            Customers _customer = new Customers();

            try
            {
                _customer = _customerRepo.GetCustomerDetails(MicrosoftID, Id);
                var getCustomerAgreement = _customerRepo.GetCustomerAgreements(MicrosoftID);
                ViewBag.CID = _customer.ID;

                if (getCustomerAgreement.TotalCount >= 1)
                {
                    var AgreementInfo = getCustomerAgreement.Items.FirstOrDefault();

                    ViewBag.MCADateAgreed = AgreementInfo.DateAgreed.ToString("MMMM dd, yyyy");
                    ViewBag.MCAName = AgreementInfo.PrimaryContact.FirstName + " " + AgreementInfo.PrimaryContact.LastName;
                    ViewBag.MCAEmail = AgreementInfo.PrimaryContact.Email;
                    ViewBag.MCAPhoneNumber = AgreementInfo.PrimaryContact.PhoneNumber;
                }
            
            }
            catch (Exception x)
            {
                string test = x.Message.ToString();
                ModelState.AddModelError("", x.Message.ToString());

                return RedirectToAction("Index");
            }

            return View(_customer);

        }


        public ActionResult Confirmation()
        {
            return View();
        }

        public string ChangeOrderBillingCycle(string CustomerId,string OrderId,string BillingCycle)
        {
            string errorMessage = "";

            OrderRepository order = new OrderRepository();

            string strCustomerID = CustomerId;
            string strOrderID = OrderId;
            

            order.UpdateOrderBillingCycle(strCustomerID,strOrderID,out errorMessage, BillingCycle);

            return errorMessage;
        }

        public String ChangeBilling(string CustomerId, string OrderId)
        {
            SubscriptionsRepository subscriptionsRepository = new SubscriptionsRepository();

            var subscriptionDetails = subscriptionsRepository.GetSubscriptionByOrder(CustomerId, OrderId);

            List<clsSubscriptionByOrder> lsubscriptionOrders = new List<clsSubscriptionByOrder>();

            foreach (var item in subscriptionDetails.Items)
            {
                clsSubscriptionByOrder oSubscriptionByOrder = new clsSubscriptionByOrder();
                oSubscriptionByOrder.Id = item.Id;
                oSubscriptionByOrder.FriendlyName = item.FriendlyName;
                lsubscriptionOrders.Add(oSubscriptionByOrder);
            }

            return JsonConvert.SerializeObject(lsubscriptionOrders);
        }

        [HttpPost]
        public ActionResult MicrosoftCloudAgreement(string FirstName,string LastName,string EmailAddress,string PhoneNumber,string AgreementDate,string CID)
        {
            try
            {
                var strName = FirstName + ' ' + LastName + ' ' + EmailAddress + ' ' + PhoneNumber + ' ' + AgreementDate;


                int iCID = int.Parse(CID);

                var _cus = db.Customers.First(c => c.ID == iCID);
                _cus.FirstName = FirstName;
                _cus.LastName = LastName;
                _cus.EmailAddress = EmailAddress;
                _cus.PhoneNumber = PhoneNumber;
                _cus.AgreementDate = AgreementDate;

                _customerRepo.CreateCustomerAgreement(_cus.MicrosoftID,_cus.ID.ToString(),FirstName,LastName,EmailAddress,PhoneNumber,AgreementDate);

                var isuccess = db.SaveChanges();

                if (isuccess == 1)
                {
                    return RedirectToAction("Subscriptions");
                }
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }

            return View();
        }   

        public ActionResult Subscriptions(string MicrosoftID, int? Id, string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (Id == null || !Id.HasValue)
            {
                return RedirectToAction("Index");
            }


            var subs = ViewData["SubscriptionVM"];

            ViewBag.CurrentSort = sortColumn;
            ViewBag.SortOrder = sortOrder == "asc" ? "desc" : "asc";
            int pageSize = 10;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

          

            ViewBag.CurrentFilter = searchString;
            List<Subscriptions> _sub = new List<Subscriptions>();
            _sub = _orderRepo.GetSubscriptionPerCustomer(Id.Value);

            IQueryable<Subscriptions> _lstSub = _sub.AsQueryable();
            List<SubscriptionVM> subscriptionVMs = new List<SubscriptionVM>();

            ResourceCollection<Subscription> getCustomerSubscription = null;

            string strErrorMsg = "";

            try
            {
                if (MicrosoftID != string.Empty && MicrosoftID  != null)
                {
                    getCustomerSubscription = _customerRepo.GetCustomersSubscriptions(MicrosoftID, out strErrorMsg);
                }

            }
           catch (PartnerException ex)
            {
                if (ex.ErrorCategory == PartnerErrorCategory.Forbidden)
                {
                    ViewBag.ErrorCategory = "Forbidden";
                }
            }

            if (getCustomerSubscription != null)
            {
                foreach (var item in getCustomerSubscription.Items)
                {
                    SubscriptionVM subscription = new SubscriptionVM();
                    subscription.OfferName = item.OfferName;
                    subscription.Quantity = item.Quantity;
                    subscription.UnitType = item.UnitType;
                    subscription.CustomerID = Id.Value;
                    subscription.OfferID = item.OfferId;
                    subscription.Status = item.Status.ToString();

                    subscriptionVMs.Add(subscription);

                    //Save the subscription in DB
                    var _subscriptions = db.Subscriptions.Where(s => s.CustomerID == Id && s.SubscriptionID == item.EntitlementId).FirstOrDefault();

                    if (_subscriptions != null)
                    {
                        //Update
                        _subscriptions.OfferName = item.OfferName;
                        _subscriptions.Quantity = item.Quantity;
                        _subscriptions.SubscriptionID = item.EntitlementId;
                        _subscriptions.OffersId = item.OfferId;
                        _subscriptions.CreationDate = item.CreationDate;
                        _subscriptions.EffectiveStartDate = item.EffectiveStartDate;
                        _subscriptions.CommitmentEndDate = item.CommitmentEndDate;
                        _subscriptions.Status = item.Status.ToString();
                        _subscriptions.BillingCycle = item.BillingCycle.ToString();
                        _subscriptions.OrderId = item.OrderId;
                        _subscriptions.PartnerId = item.PartnerId;

                        db.SaveChanges();
                    }
                    else
                    {
                        //Insert
                        Subscriptions newSubsriptions = new Subscriptions();
                        newSubsriptions.CustomerID = Id.Value;
                        newSubsriptions.OfferName = item.OfferName;
                        newSubsriptions.SubscriptionID = item.EntitlementId;
                        newSubsriptions.OffersId = item.OfferId;
                        newSubsriptions.Quantity = item.Quantity;
                        newSubsriptions.CreationDate = item.CreationDate;
                        newSubsriptions.EffectiveStartDate = item.EffectiveStartDate;
                        newSubsriptions.CommitmentEndDate = item.CommitmentEndDate;
                        newSubsriptions.Status = item.Status.ToString();
                        newSubsriptions.BillingCycle = item.BillingCycle.ToString();
                        newSubsriptions.OrderId = item.OrderId.ToString();
                        newSubsriptions.PartnerId = item.PartnerId;

                        db.Subscriptions.Add(newSubsriptions);
                        db.SaveChanges();
                    }

                }
            }
            else
            {
                if (strErrorMsg == "Customer not found")
                {
                    TempData["CustomerNotFoundName"] = db.Customers.Where(c => c.ID == Id.Value).FirstOrDefault().CompanyName;
                    TempData["CustomerNotFoundMicrosoftId"] = MicrosoftID;
                    //Redirect to customer not found page
                    return RedirectToAction("CustomerNotFound");
                }
                else
                {
                    TempData["CustomerError"] = strErrorMsg;
                    return RedirectToAction("Error");
                }
               
            }

            

            var __lstSub = from p in _lstSub
                           group p by p.OffersId into g
                           select new SubscriptionVM
                           {
                               OfferID = g.Key,
                               OfferName = g.FirstOrDefault().OfferName,
                               Quantity = g.Sum(p => p.Quantity.Value),
                               UnitType = g.FirstOrDefault().Offers.UnitType,
                               CustomerID = Id.Value,
                               Count = g.Count(),
                               FirstName = ""
                           };

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                 subscriptionVMs = subscriptionVMs.Where(v => v.OfferName.ToLower().Contains(searchString)
                                    || v.Quantity.ToString().ToLower().Contains(searchString)
                                    || v.Quantity.ToString().ToLower().Contains(searchString)).ToList<SubscriptionVM>();

                __lstSub = __lstSub.Where(s =>  s.OfferName.ToLower().Contains(searchString)
                                       || s.Quantity.ToString().ToLower().Contains(searchString)
                                       || s.UnitType.ToString().ToLower().Contains(searchString));
            }


            if (String.IsNullOrEmpty(sortColumn))
            {
                subscriptionVMs = subscriptionVMs.OrderByDescending(v => v.OfferName).ToList<SubscriptionVM>();
                __lstSub = __lstSub.OrderByDescending(p => p.OfferName);
            }
            else
            {
                subscriptionVMs = subscriptionVMs.OrderBy(sortColumn + " " + sortOrder).ToList<SubscriptionVM>();
                __lstSub = __lstSub.OrderBy(sortColumn + " " + sortOrder);
            }

            ViewBag.CID = Id.Value;
            Session["CustomerID"] = Id.Value;
            ViewData["SubscriptionVM"] = _sub;
            ViewBag.CustomerName = db.Customers.Where(c => c.ID == Id.Value).FirstOrDefault().CompanyName;

            var strMicrosoftId = db.Customers.Where(c => c.ID == Id.Value).FirstOrDefault().MicrosoftID;

            var getCustomerAgreement = _customerRepo.GetCustomerAgreements(strMicrosoftId);

            var iisMCAAgreed = getCustomerAgreement.TotalCount;

            if (iisMCAAgreed >= 1)
            {
                ViewBag.IsMCAAgreed = true ;
            }
            else
            {
                ViewBag.IsMCAAgreed = false;
            }


            int pageNumber = (page ?? 1);
            return View(subscriptionVMs.ToPagedList(pageNumber, pageSize));


            // return View(_sub);
        }

        public ActionResult Error()
        {
            ViewData["CustomerError"] = TempData["CustomerError"];
            return View();
        }

        public ActionResult CustomerNotFound()
        {
            ViewData["CustomerNotFoundName"] = TempData["CustomerNotFoundName"];
            ViewData["CustomerNotFoundMicrosoftId"] = TempData["CustomerNotFoundMicrosoftId"];

            return View();
        }

        public ActionResult Subscription(int? Id, string OfferID, string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
        {


            if (Id == null || !Id.HasValue)
            {
                return RedirectToAction("Index");
            }

            ViewBag.CurrentSort = sortColumn;
            ViewBag.SortOrder = sortOrder == "asc" ? "desc" : "asc";
            int pageSize = 10;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }


            ViewBag.CurrentFilter = searchString;
            List<Subscriptions> _sub = new List<Subscriptions>();
            _sub = _orderRepo.GetSubscriptionPerCustomer(Id.Value);

            IQueryable<Subscriptions> _lstSub = _sub.Where(s=>s.OffersId == OfferID ).AsQueryable();


            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                _lstSub = _lstSub.Where(s => s.SubscriptionID.ToLower().Contains(searchString)
                                       || s.OfferName.ToLower().Contains(searchString)
                                       || s.PartnerId.ToString().ToLower().Contains(searchString)
                                       || s.CreationDate.ToString().ToLower().Contains(searchString));
            }


            if (String.IsNullOrEmpty(sortColumn))
            {
                _lstSub = _lstSub.OrderByDescending(p => p.ID);
            }
            else
            {
                _lstSub = _lstSub.OrderBy(sortColumn + " " + sortOrder);
            }

            ViewBag.CID = Id.Value;
            Session["CustomerID"] = Id.Value;
            ViewBag.CustomerName = db.Customers.Where(c=>c.ID == Id.Value).FirstOrDefault().CompanyName;

            int pageNumber = (page ?? 1);
            return View(_lstSub.ToPagedList(pageNumber, pageSize));

           
           // return View(_sub);
        }

        public List<SelectListItem> GetReseller()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem
            {
                Text = "No indirect reseller",
                Value = "0"
            });

            return listItems;
        }


        [HttpPost]
        public JsonResult GetCustomersName(string Prefix)
        {
            if (Prefix == "" || Prefix == null)
                Prefix = "";

            //if (_customer == null)
            //{
            //    //List<Customer> _customer = new List<Customer>();
            //    _customer = new List<Customer>();
            //    _customer = _customerRepo.GetCustomers();
            //    //_customer = _customerRepo.GetCustomers(searchString, page.Value, pageSize);
            //}
            List<Customers> _customer = new List<Customers>();
            _customer = _customerRepo.GetCustomers();

            var cusName = (from c in _customer
                           where (c.CompanyName.ToUpper()).Contains(Prefix.ToUpper())
                           select new { CustomerName = c.CompanyName + " ("+ c.DomainName.Replace(".onmicrosoft.com", "") + ".onmicrosoft.com)", ID = c.ID })
                              .Distinct().ToList();

            return Json(cusName, JsonRequestBehavior.AllowGet);
        }

        //CheckDeclaredValue
        public JsonResult CheckDomain(string domainName)
        {
            var exists = "false";

            var checkExists = db.Customers.Where(c => c.DomainName.Replace(".onmicrosoft.com", "") == domainName.Replace(".onmicrosoft.com", "")).FirstOrDefault();
            if (checkExists == null)
            {
                domainName = domainName + ".onmicrosoft.com";
                _customerRepo = new Repositories.CustomerRepository();
                exists = _customerRepo.CheckDomain(domainName);
            }

            return Json(exists, JsonRequestBehavior.AllowGet);

        }

        public JsonResult VerifyMPNID(string mpnID)
        {
            var exists = "";

            _customerRepo = new Repositories.CustomerRepository();
            exists = _customerRepo.VerifyMPNID(mpnID);

            return Json(exists, JsonRequestBehavior.AllowGet);

        }


        public JsonResult Save(CustomerSubscriptionDTO custSubs)
        {
            string id = "0";

            Customers customer = new Customers();
            customer.CompanyName = custSubs.CompanyName;
            customer.DomainName = custSubs.DomainName;
            customer.AddressLine1 = custSubs.AddressLine1;
            customer.AddressLine2 = custSubs.AddressLine2;
            customer.City = custSubs.City;
            customer.State = custSubs.State;
            customer.ZipPostal = custSubs.ZipPostal;
            customer.SpecialQualificationsID = custSubs.SpecialQualificationsID;

            customer.FirstName = custSubs.FirstName;
            customer.LastName = custSubs.LastName;
            customer.EmailAddress = custSubs.EmailAddress;
            customer.PhoneNumber = custSubs.PhoneNumber;
            //customer.IndirectResellerID = custSubs.IndirectResellerID;
            customer.IndirectResellerID = 0;


            var custobj = new CustomerObj();
            try
            {
                custobj = CreateCustomers(customer);
            }
            catch(Exception e)
            { 
            
                id = e.Message;
            }
            if (custobj.customerID > 0)
            {
                List<OrderItemsDTO.OItems> _items = new List<OrderItemsDTO.OItems>();

                OrderItemsDTO order = new OrderItemsDTO();
                order.OrdersID = custobj.customerID.ToString();
                order.CustomerID = custobj.customerID;
                order.CreationDate = DateTime.Now;
                order.TotalPrice = custSubs.TotalPrice;
                order.TotalVat = custSubs.TotalVat;
                order.TotalAmount = custSubs.TotalAmount;
                order.ResellerID = custSubs.IndirectResellerID;
               // order.BillingCycle = custSubs.Billing; 
               
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
                        oItems.PartnerIdOnRecord = custSubs.IndirectResellerID.ToString();
                    }


                    _items.Add(oItems);
                    ctr++;
                }
                order.OItem = _items;

                //SAVE TO ORDER TABLE
                int successOrder = SaveOrder(order, custobj);
                if (successOrder > 0)
                {
                    id ="Success";
                }
            }

            //int id = SaveShipping(book, "SHIP FREIGHT");
            return Json(id, JsonRequestBehavior.AllowGet);
        }

        private CustomerObj CreateCustomers(Customers _customer)
        {
            Customer customerToCreate = new Customer();
            Customer customerCreated = new Customer();
            CustomerObj custObj = new CustomerObj();

            int customerID = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    CustomerCompanyProfile CompanyProfile = new CustomerCompanyProfile();
                    CompanyProfile.Domain = _customer.DomainName;

                    Address DefaultAddress = new Address();
                    DefaultAddress.FirstName = _customer.FirstName;
                    DefaultAddress.LastName = _customer.LastName;
                    DefaultAddress.AddressLine1 = _customer.AddressLine1;
                    DefaultAddress.City = _customer.City;
                    DefaultAddress.State = _customer.State;
                    DefaultAddress.Country = "PH";
                    DefaultAddress.PostalCode = _customer.ZipPostal;
                    DefaultAddress.PhoneNumber = _customer.PhoneNumber;

                    CustomerBillingProfile BillingProfile = new CustomerBillingProfile();
                    BillingProfile.Culture = "EN-PH";
                    BillingProfile.Email = _customer.EmailAddress;
                    BillingProfile.Language = "En";
                    BillingProfile.CompanyName = _customer.CompanyName;
                    BillingProfile.DefaultAddress = DefaultAddress;


                    customerToCreate.BillingProfile = BillingProfile;
                    customerToCreate.CompanyProfile = CompanyProfile;


                    //SEND TO MICROSOFT API
                    customerCreated =  _customerRepo.CreateCustomer(customerToCreate, _customer.SpecialQualificationsID);

                    var json = new JavaScriptSerializer().Serialize(customerCreated);


                    //SAVE TO SDS DATABASE
                    Customers _cus = new Customers();
                    _cus = _customer;
                    _cus.DomainName = _customer.DomainName.Replace(".onmicrosoft.com", "");
                    _cus.MicrosoftID = customerCreated.Id;
                    _cus.AddedBy = Sessions.UserID;
                    db.Customers.Add(_cus);

                    //SAVE TO AUDIT TRAIL
                    AuditTrails _audit = new AuditTrails();
                    _audit.Module = "Create Customer";
                    _audit.TaskDesc = json;
                    _audit.AuditDate = DateTime.Now;
                    _audit.UserID = Sessions.UserID;
                    db.AuditTrails.Add(_audit);



                    db.SaveChanges();

                    customerID = _cus.ID;

                    try
                    {
                        var _email = db.Resellers.FirstOrDefault(p => p.ResellerID == Sessions.ResellerID).Emails;

                        int id = db.Resellers.FirstOrDefault(p => p.ResellerID == Sessions.ResellerID).AuthorizedRepresentative.Value;

                        var _authorizedemail = db.AuthorizedRepresentatives.Find(id);

                        _mailer.SendNewCustomer("New Customer", "Created date" + DateTime.Now.ToString() + _customer.FirstName + " " + _customer.LastName, Sessions.ResellerName, _email);
                        _mailer.SendNewCustomer("New Customer Created by Reseller", "Created date" + DateTime.Now.ToString() + _customer.FirstName + " " + _customer.LastName, Sessions.ResellerName, _authorizedemail.EmailAddress);
                    }
                    catch { }
                }

             
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }

                    string test = e.Message.ToString();
                    ModelState.AddModelError("", e.Message.ToString());

                    customerID = 0;

                    custObj.cust = customerCreated;
                    custObj.customerID = 0;
                   
                    return custObj;
                }

            }
            else
            {

                var errors = ModelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
                                       .ToList();
            }
            custObj.cust = customerCreated;
            custObj.customerID = customerID;

            return custObj;
        }

        private int SaveOrder(OrderItemsDTO _order, CustomerObj _custObj )
        {
            int order = 0;
            try
            {
                order = _orderRepo.SaveOrder(_order, _custObj);

            }
            catch (Exception e)
            {
            }

            return order;
        }


        public JsonResult SendRequest(RequestRelationshipDTO req)
        {
            int id = 0;

            try
            {
                //SEND EMAIL
                Mailer _mail = new Mailer();
                _mail.SendRequest("Request a partnership", req.EmailMessage, req.EmailAddress);
                id = 1;

                var json = new JavaScriptSerializer().Serialize(req);
                //LOGS
                //SAVE TO AUDIT TRAIL
                AuditTrails _audit = new AuditTrails();
                _audit.Module = "Request a partnership";
                _audit.TaskDesc = json;
                _audit.AuditDate = DateTime.Now;
                _audit.UserID = Sessions.UserID;
                db.AuditTrails.Add(_audit);
                db.SaveChanges();
            }
            catch 
            {

            }
          

            return Json(id, JsonRequestBehavior.AllowGet);

        }

        public ActionResult SubscriptionDetails(string sID)
        {
            if (sID != "")
            {
                var sub = db.Subscriptions.Where(s => s.SubscriptionID == sID).FirstOrDefault();

                int customers = Convert.ToInt32(Session["CustomerID"]); //sub.CustomerID.Value;

                if (sub.PartnerId != null)
                {
                    var order = db.Orders.FirstOrDefault(o => o.CustomersID == customers);
                    int partnerid = order != null ? order.ResellerID : 0;

                    ViewBag.IReseller = _resellerRepo.GetResellerSubs(partnerid, sub.PartnerId);
                }
                else
                {
                    ViewBag.IReseller = _resellerRepo.GetResellerSubsNo();
                }
                return View(sub);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public JsonResult UpdateSubscription(UpdateSubDTO custSubs)
        {
            int id = 0;
            try
            {
                Subscriptions sub = new Models.Subscriptions();
                sub = db.Subscriptions.Where(s => s.SubscriptionID == custSubs.SubscriptionID).FirstOrDefault();
                sub.Quantity = custSubs.Quantity;
                sub.Status = custSubs.Status;

                Subscription item = new Subscription();
                _orderRepo = new Repositories.OrderRepository();
                item =  _orderRepo.UpdateSubscription(sub);


                //UPDATE SUBSCRIPTION
                Subscriptions exists = db.Subscriptions.Where(o => o.SubscriptionID == item.Id).FirstOrDefault();
                exists.SubscriptionID = item.Id;
                exists.OffersId = item.OfferId;
                exists.OfferName = item.OfferName;
                exists.Quantity = item.Quantity;
                exists.CreationDate = item.CreationDate;
                exists.EffectiveStartDate = item.EffectiveStartDate;
                exists.CommitmentEndDate = item.CommitmentEndDate;
                exists.Status = item.Status.ToString();
                exists.BillingCycle = item.BillingCycle.ToString();
                exists.OrderId = item.OrderId;
                exists.PartnerId = item.PartnerId;
                exists.Status = item.Status.ToString();

                //var checkIfSame = Compare(sub, exists, "Subscriptions");
                //if (checkIfSame == false)
                //{
                //    db.Entry(exists).State = EntityState.Modified;
                //    db.SaveChanges();
                //}

                int orderid = db.Orders.FirstOrDefault(p => p.OrdersID == item.OrderId).ID;
                _orderRepo.SendEmail(orderid);
                db.Entry(exists).State = EntityState.Modified;
                db.SaveChanges();

                id = exists.CustomerID.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return Json(id, JsonRequestBehavior.AllowGet);
        }

        static bool Compare<T>(T Object1, T object2, string _tableName)
        {
            //Get the type of the object
            Type type = typeof(T);

            //return false if any of the object is false
            if (Object1 == null || object2 == null)
                return false;

            //Loop through each properties inside class and get values for the property from both the objects and compare
            foreach (System.Reflection.PropertyInfo property in type.GetProperties())
            {
               if (_tableName == "Subscriptions")
                {
                    if (property.Name != "ID" && property.Name != "Customers" && property.Name != "Offers" && property.Name != "CustomerID")
                    {
                        string Object1Value = string.Empty;
                        string Object2Value = string.Empty;
                        if (type.GetProperty(property.Name).GetValue(Object1, null) != null)
                            Object1Value = type.GetProperty(property.Name).GetValue(Object1, null).ToString();
                        if (type.GetProperty(property.Name).GetValue(object2, null) != null)
                            Object2Value = type.GetProperty(property.Name).GetValue(object2, null).ToString();
                        if (Object1Value.Trim() != Object2Value.Trim())
                        {
                            return false;
                        }
                    }
                }

            }
            return true;
        }

        protected override void Dispose(bool disposing)
        {

            if (_Session != "change")
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}
