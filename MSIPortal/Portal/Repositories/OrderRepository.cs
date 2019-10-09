using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Store.PartnerCenter.Models.Customers;
using Microsoft.Store.PartnerCenter.Models;
using Microsoft.Store.PartnerCenter.Samples.Context;
using Microsoft.Store.PartnerCenter.Samples.Customers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Store.PartnerCenter.Models.Orders;
using Microsoft.Store.PartnerCenter.Samples.Orders;
using Microsoft.Store.PartnerCenter.Models.Subscriptions;
using Portal.Models;
using System.Data.Entity.Validation;
using System.Web.Script.Serialization;
using Portal.Helpers;
using System.Data.Entity;
using System.Text.RegularExpressions;
using Microsoft.Store.PartnerCenter.Models.Offers;

namespace Portal.Repositories
{
    public class OrderRepository
    {
        MSIPortalEntities db = new MSIPortalEntities();
        Mailer mail = new Mailer();
        CustomerRepository custRepo = new CustomerRepository();
        public bool UpdateOrderBillingCycle(string _customerID,string _orderID,out string errorMessage,string _billingCycle)
        {
            bool returnValue = false;

            SubscriptionsRepository subscriptionsRepository = new SubscriptionsRepository();

            var subscriptionDetails = subscriptionsRepository.GetSubscriptionByOrder(_customerID, _orderID);

            var lOrderLineItems = new List<OrderLineItem>();
            int iLineItem = 0;


            foreach (var item in subscriptionDetails.Items)
            {
                
                var oLineOrder = new OrderLineItem();
                oLineOrder.LineItemNumber = iLineItem;
                oLineOrder.OfferId = item.OfferId;
                oLineOrder.SubscriptionId = item.Id;
                oLineOrder.Quantity = item.Quantity;
                lOrderLineItems.Add(oLineOrder);

                var itemSubscription = db.Subscriptions.Where(s => s.SubscriptionID == item.Id).FirstOrDefault();
                itemSubscription.BillingCycle = _billingCycle;
                db.SaveChanges();

                iLineItem++;
            }

            BillingCycleType billingCycleType = new BillingCycleType();

            if (_billingCycle == "Monthly")
            {
                billingCycleType = BillingCycleType.Monthly;
            }else if (_billingCycle == "Annual")
            {
                billingCycleType = BillingCycleType.Annual;
            }

            var order = new Order()
            {
                ReferenceCustomerId = _customerID,
                BillingCycle = billingCycleType,
                LineItems = lOrderLineItems

            };

            var context = new ScenarioContext();

            var partnerOperations = context.AppPartnerOperations;

            try
            {
                var createdOrder = partnerOperations.Customers.ById(_customerID).Orders.ById(_orderID).Patch(order);
                errorMessage = "Success";
                returnValue = true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message.ToString();
            }



            return returnValue;
        }

        public List<Orders> GetOrdersPerCustomerID(string _customerID)
        {
            //var context = new ScenarioContext();
            //GetOrders orders = new GetOrders(context);
            //string json = orders.GetOrdersPerCustomerID(_customerID);
            //JToken token = JObject.Parse(json);
            //var _token = token.SelectToken("Items").ToString();

            //List<Order> orderObj = JsonConvert.DeserializeObject<List<Order>>(_token);
            List<Orders> orderObj = new List<Orders>();
            return orderObj;
        }
        public List<Orders> GetSubscriptionPerCustomerID(string _customerID)
        {
            //var context = new ScenarioContext();
            //GetOrders orders = new GetOrders(context);
            //string json = orders.GetOrdersPerCustomerID(_customerID);
            //JToken token = JObject.Parse(json);
            //var _token = token.SelectToken("Items").ToString();

            //List<Order> orderObj = JsonConvert.DeserializeObject<List<Order>>(_token);
            List<Orders> orderObj = new List<Orders>();
            return orderObj;
        }
        public List<Subscription> GetSubscriptionsPerCustomerID(string _customerID)
        {
            var context = new ScenarioContext();
            GetOrders orders = new GetOrders(context);
            string json = orders.GetSubscriptionsPerCustomerID(_customerID);
            JToken token = JObject.Parse(json);
            var _token = token.SelectToken("Items").ToString();

            List<Subscription> subsObj = JsonConvert.DeserializeObject<List<Subscription>>(_token);

            return subsObj;
        }

      

        public void CreateOrders(List<Order> order)
        {
            var context = new ScenarioContext();
            CreateOrder orders = new CreateOrder(context, order);
            orders.Run();
        }
        public void CreateOrdersAPI(Orders _ordr,CustomerObj custobj)
        {
            List<Order> order = new List<Order>();
            var _orders = db.Orders.Include(p => p.OrderItems).Where(p => p.ID == _ordr.ID).ToList();

            var context = new ScenarioContext();
            CreateOrder orders = new CreateOrder(context, order);


            foreach (var o in _orders)
            {
                int ctr = 0;
                List<OrderLineItem> orderitems = new List<OrderLineItem>();

                Customers _customer = db.Customers.Find(o.CustomersID);
                if (_customer.MicrosoftID != null && _customer.MicrosoftID != "")
                {
                    foreach (var orditem in o.OrderItems)
                    {
                        OrderLineItem item = new OrderLineItem()
                        {
                            LineItemNumber = ctr,
                            OfferId = orditem.OffersID,
                            FriendlyName = orditem.FriendlyName == "" ? db.Offers.FirstOrDefault(of=>of.OffersID == orditem.OffersID).Name : orditem.FriendlyName,
                            Quantity = orditem.Quantity.Value,
                            PartnerIdOnRecord = orditem.PartnerIdOnRecord
                        };

                        orderitems.Add(item);
                        ctr++;
                    }

                    Order orderSend = new Order();
                    orderSend.ReferenceCustomerId = _customer.MicrosoftID;
                    orderSend.LineItems = orderitems;

                    order.Add(orderSend);
                    try
                    {
                        string _createOrder = orders.CreateOrder(_customer.MicrosoftID, orderSend);
                        JToken token = JObject.Parse(_createOrder);
                        string _orderid = token.SelectToken("Id").ToString();

                        Orders __orders = db.Orders.Find(o.ID);
                        __orders.OrdersID = _orderid;
                        __orders.MicrosoftID = _customer.MicrosoftID;
                        __orders.OrderStatus = "Sent";

                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                        string email = db.Customers.Find(custobj.customerID).EmailAddress;
                        mail.SendCustomerCredentials(email, orderSend.LineItems.ToList(), custobj.cust);

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

                        Orders __orders = db.Orders.Find(o.ID);
                        __orders.OrderStatus = "Send Failed - " + e.Message.ToString();
                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                        ErrorLogs _error = new ErrorLogs();
                        _error.ReferenceID = "Order ID: " + o.ID.ToString();
                        _error.Error = "Send Failed - " + e.Message.ToString();
                        db.ErrorLogs.Add(_error);
                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    catch (Exception e)
                    {
                        Orders __orders = db.Orders.Find(o.ID);
                        __orders.OrderStatus = "Send Failed - " + e.Message.ToString();
                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                        ErrorLogs _error = new ErrorLogs();
                        _error.ReferenceID = "Order ID: " + o.ID.ToString();
                        _error.Error = "Send Failed - " + e.Message.ToString();
                        db.ErrorLogs.Add(_error);
                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                    }


                }
            }


            //var context = new ScenarioContext();
            //CreateOrder orders = new CreateOrder(context, order);
            ////orders.Run();
        }
        public void CreateOrdersAPI_sub(Orders _ordr, Customer custobj,out string strOrdersId, out List<string> lErrorMessage)
        {

            strOrdersId = null;
            lErrorMessage = new List<string>();


            List<Order> order = new List<Order>();
            var _orders = db.Orders.Include(p => p.OrderItems).Where(p => p.ID == _ordr.ID).ToList();

            var context = new ScenarioContext();
            CreateOrder orders = new CreateOrder(context, order);

            List<string> lOrderId = new List<string>();

            foreach (var o in _orders)
            {
                int ctr = 0;
                List<OrderLineItem> orderitems = new List<OrderLineItem>();

                Customers _customer = db.Customers.Find(o.CustomersID);
                if (_customer.MicrosoftID != null && _customer.MicrosoftID != "")
                {
                    foreach (var orditem in o.OrderItems)
                    {
                        OrderLineItem item = new OrderLineItem()
                        {
                            LineItemNumber = ctr,
                            OfferId = orditem.OffersID,
                            FriendlyName = orditem.FriendlyName == "" ? db.Offers.FirstOrDefault(of => of.OffersID == orditem.OffersID).Name : orditem.FriendlyName,
                            Quantity = orditem.Quantity.Value,
                            PartnerIdOnRecord = orditem.PartnerIdOnRecord
                        };

                        orderitems.Add(item);
                        ctr++;
                    }

                    Order orderSend = new Order();
                    orderSend.ReferenceCustomerId = _customer.MicrosoftID;
                    orderSend.LineItems = orderitems;

                    var oBillingCycleType = new BillingCycleType();

                    if (_ordr.BillingCycle == "Monthly")
                    {
                        oBillingCycleType = BillingCycleType.Monthly;

                    }else if (_ordr.BillingCycle == "Annual")
                    {
                        oBillingCycleType = BillingCycleType.Annual;
                    }else if (_ordr.BillingCycle == "OneTime")
                    {
                        oBillingCycleType = BillingCycleType.OneTime;
                    }else if (_ordr.BillingCycle == "None")
                    {
                        oBillingCycleType = BillingCycleType.None;
                    }
                    else
                    {
                        oBillingCycleType = BillingCycleType.Unknown;
                    }


                    orderSend.BillingCycle = oBillingCycleType;

                    order.Add(orderSend);
                    try
                    {
                        string _createOrder = orders.CreateOrder(_customer.MicrosoftID, orderSend);
                        JToken token = JObject.Parse(_createOrder);
                        string _orderid = token.SelectToken("Id").ToString();

                        Orders __orders = db.Orders.Find(o.ID);
                        __orders.OrdersID = _orderid;
                        __orders.MicrosoftID = _customer.MicrosoftID;
                        __orders.OrderStatus = "Sent";

                        strOrdersId = _orderid;

                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                        string email = db.Customers.Find(_customer.ID).EmailAddress;
                        mail.SendCustomerCredentials(email, orderSend.LineItems.ToList(), custobj);

                    }
                    catch (DbEntityValidationException e)
                    {

                        lErrorMessage.Add(e.Message.ToString());

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

                        Orders __orders = db.Orders.Find(o.ID);
                        __orders.OrderStatus = "Send Failed - " + e.Message.ToString();
                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                        ErrorLogs _error = new ErrorLogs();
                        _error.ReferenceID = "Order ID: " + o.ID.ToString();
                        _error.Error = "Send Failed - " + e.Message.ToString();
                        db.ErrorLogs.Add(_error);
                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    catch (Exception e)
                    {
                        lErrorMessage.Add(e.Message.ToString());
                        Orders __orders = db.Orders.Find(o.ID);
                        __orders.OrderStatus = "Send Failed - " + e.Message.ToString();
                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                        ErrorLogs _error = new ErrorLogs();
                        _error.ReferenceID = "Order ID: " + o.ID.ToString();
                        _error.Error = "Send Failed - " + e.Message.ToString();
                        db.ErrorLogs.Add(_error);
                        db.Entry(__orders).State = EntityState.Modified;
                        db.SaveChanges();

                    }


                }
            }


            

            //var context = new ScenarioContext();
            //CreateOrder orders = new CreateOrder(context, order);
            ////orders.Run();
        }
        public void UpdateOrderSubscription(int _orderID)
        {
            try
            {
                var oItems = db.OrderItems.Include(o => o.Orders).Where(o => o.OrdersID == _orderID).ToList();
                foreach (var item in oItems)
                {
                    var subs = db.Subscriptions.Where(s => s.OrderId == item.Orders.OrdersID).ToList();
                    foreach (var itm in subs)
                    {
                        if (item.OffersID == itm.OffersId)
                        {
                            OrderItems exists = db.OrderItems.Where(o => o.ID == item.ID).FirstOrDefault();
                            exists.SubscriptionID = itm.SubscriptionID;

                            Orders ord = db.Orders.Where(o => o.ID == exists.OrdersID).FirstOrDefault();
                            ord.OrderStatus = "successful";

                            db.Entry(exists).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                if (oItems.FirstOrDefault().Orders.OrderStatus == "successful")
                {
                    SendEmail(_orderID);
                }

            }
            catch
            {

            }

        }

        public void SendEmail(int _orderID)
        {
            try
            {
                var _order = db.Orders.Where(o => o.ID == _orderID).FirstOrDefault();
                //SEND EMAIL
                var email = db.EmailTemplates.Where(e => e.EmailType == "Successful Order").FirstOrDefault();

                string resellerName = "";
                string _partnerID = _order.OrderItems.FirstOrDefault().PartnerIdOnRecord;
                if (_partnerID != "" && _partnerID != null)
                {
                    resellerName = db.Resellers.FirstOrDefault(r => r.MPNID.ToString() == _partnerID).ResellerName;
                }
                //REPLACE SUBJECT
                string subject = "";
                subject = email.EmailSubject;
                subject = Regex.Replace(subject, "%CustomerName%", _order.Customers.CompanyName);
                subject = Regex.Replace(subject, "%ResellerName%", resellerName);
                subject = Regex.Replace(subject, "%OrderDate%", _order.CreatedDate.ToString());
                subject = Regex.Replace(subject, "%OrderCompletedDate%", _order.CreationDate.ToString());
                subject = Regex.Replace(subject, "%OrderID%", _order.OrdersID);



                //REPLACE DETAILS
                string details = "";
                string _orderDetails = "";
                //var sub = db.Subscriptions.Where(s => s.OrderId == _order.OrdersID);

                foreach (var item in _order.OrderItems)
                {
                    var det = db.Offers.Where(s => s.OffersID == item.OffersID).FirstOrDefault();

                    details = email.DetailsContent;

                    if (det.UnitType == "Usage-based")
                    {
                        details = Regex.Replace(details, "%OrderQty%", "");
                        details = Regex.Replace(details, "%OrderAmount%", "");
                    }
                    else
                    {
                        details = Regex.Replace(details, "%OrderQty%", item.Quantity.ToString());
                        details = Regex.Replace(details, "%OrderAmount%", item.Price.ToString());
                    }

                    details = Regex.Replace(details, "%OfferID%", item.OffersID);
                    details = Regex.Replace(details, "%SubscriptionID%","");
                    details = Regex.Replace(details, "%ProductName%", det.Name);

                    _orderDetails += details + "\r\n";
                }



                //REPLACE CONTENT
                string content = "";
                content = email.EmailContent;
                content = Regex.Replace(content, "%CustomerName%", _order.Customers.CompanyName);
                content = Regex.Replace(content, "%ResellerName%", resellerName);
                content = Regex.Replace(content, "%OrderDate%", _order.CreatedDate.ToString());
                content = Regex.Replace(content, "%OrderCompletedDate%", _order.CreationDate.ToString());
                content = Regex.Replace(content, "%OrderID%", _order.OrdersID);
                content = Regex.Replace(content, "%Details%", _orderDetails);



                List<string> to = new List<string>();
                List<string> bcc = new List<string>();

                //to.Add("lira@sdsolutions.com.ph");

                //ALL ADMIN
                //PA
                var adminPA = db.Users.Where(u => u.UserTypes.UserType == "Admin" || u.UserTypes.UserType == "PA");
                foreach (var item in adminPA)
                {
                    if (item.Email != "" && item.Email != null)
                    {
                        bcc.Add(item.Email);
                    }
                }

                //CUSTOMER
                to.Add(_order.Customers.EmailAddress);



                if (_partnerID != "" && _partnerID != null)
                {
                    //RESELLER

                    var reseller = db.Resellers.Where(r => r.MPNID.ToString() == _partnerID).FirstOrDefault();
                    if (reseller.Emails != "" && reseller.Emails != null)
                    {
                        to.Add(reseller.Emails);
                    }

                    //SALES AE
                    var salesAE = db.UserAccessDetails.Include(u => u.UserAccessHeader).Where(u => u.TaggedID == reseller.ResellerID.ToString());
                    foreach (var item in salesAE)
                    {
                        var userIDEmail = db.Users.Where(u => u.ID == item.UserAccessHeader.UserID).FirstOrDefault().Email;
                        if (userIDEmail != "" && userIDEmail != null)
                        {
                            to.Add(userIDEmail);
                        }
                    }
                }

                //to = new List<string>();
                //to.Add("lira@sdsolutions.com.ph");

                Mailer _mailer = new Mailer();
                _mailer.SendEmail(to, subject, content, bcc);
            }
            catch
            {

            }
        }
       
        public int SaveOrder(OrderItemsDTO _order, CustomerObj custobj)
        {
            int order = 0;
            string _action = "create";
            try
            {

                string mpnid = null;
                if(_order.ResellerID != 0)
                    mpnid = db.Resellers.Find(_order.ResellerID).MPNID.ToString();

                string microsoftid = null;
                     if (String.IsNullOrEmpty(_order.MicrosoftID))
                    microsoftid = db.Customers.Find(_order.CustomerID).MicrosoftID;


                Orders orders = new Orders();
             
                    if (String.IsNullOrEmpty(microsoftid))
                    {
                        microsoftid = "";
                    }
                        var context = new ScenarioContext();
                        var partnerOperations = context.AppPartnerOperations;

                        var customerSubscription = partnerOperations.Customers.ById(microsoftid).Subscriptions.Get();
                    
                        string _json = JsonConvert.SerializeObject(customerSubscription);
                        JToken token = JObject.Parse(_json);
                        var _token = token.SelectToken("Items").ToString();

                        List<Subscription> subsObj = JsonConvert.DeserializeObject<List<Subscription>>(_token);
                  
                        var newList1 = subsObj.Where(m => _order.OItem.Any(z => z.PartnerIdOnRecord == m.PartnerId)).ToList();
                    
                    int _count = 0;
                    foreach (var item in _order.OItem)
                    {
                        var checkIfExists = newList1.Where(x => x.PartnerId == mpnid && x.OfferId == item.OffersId).FirstOrDefault();
                        if (checkIfExists != null)
                        {
                            string _subid = db.Subscriptions.FirstOrDefault(s => s.CustomerID == _order.CustomerID && s.OffersId == item.OffersId && s.PartnerId == mpnid).SubscriptionID;
                            _UpdateSubscription(_subid, item.Quantity);
                            _action = "update";
                        }

                        else
                        {

                            OrderItems items = new OrderItems();
                            _count++;
                            items.OrdersID = orders.ID;
                            items.LineItemNumber = _count;
                            items.OffersID = item.OffersId;
                            items.FriendlyName = "";
                            items.Quantity = item.Quantity;
                            items.PartnerIdOnRecord = mpnid; //item.PartnerIdOnRecord;
                            items.Price = item.Price;
                            
                            db.OrderItems.Add(items);
                        }
                       
                    }

                if (_count > 0)
                {

                    orders.OrdersID = _order.OrdersID;
                    orders.CustomersID = _order.CustomerID;
                    orders.MicrosoftID = microsoftid;
                    orders.CreationDate = DateTime.Now;
                    orders.TotalPrice = _order.TotalPrice;
                    orders.TotalVat = _order.TotalVat;
                    orders.TotalAmount = _order.TotalAmount;
                    orders.CreatedDate = DateTime.Now;
                    orders.ResellerID = _order.ResellerID;
                    //orders.BillingCycle = _order.BillingCycle;
                    /* July 6, 2018
                     * request for adding billing cycle in order
                     * currently for phase 2

                     */

                    db.Orders.Add(orders);
                    db.SaveChanges();
                    CreateOrdersAPI(orders,custobj);
                    SendEmail(orders.ID);
                


                    var json = new JavaScriptSerializer().Serialize(_order);
                    //SAVE TO AUDIT TRAIL
                    AuditTrails _audit = new AuditTrails();
                    _audit.Module = "Create Order";
                    _audit.TaskDesc = json;
                    _audit.AuditDate = DateTime.Now;
                    _audit.UserID = Sessions.UserID;
                    db.AuditTrails.Add(_audit);

                    db.SaveChanges();
                }
                //  }



                _order.OrdersID = orders.ID.ToString();

              

               

                order =1;


                ////CALL API
                //if(_action == "create")
                //    CreateOrdersAPI(orders);
                //else
                //UPDATE SUBSCRIPTIONS
                //if (_action == "update")
                //    UpdateOrderSubscription(orders.ID);

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
            }

            return order;
        }

        public int SaveOrder_sub(OrderItemsDTO _order,out string strOrdersId,out List<string> lErrorMessage)
        {

            strOrdersId = null;
            lErrorMessage = null;

            int order = 0;
            string _action = "create";
            try
            {

                string mpnid = null;
                if (_order.ResellerID != 0)
                    mpnid = db.Resellers.Find(_order.ResellerID).MPNID.ToString();

                string microsoftid = null;
                if (String.IsNullOrEmpty(_order.MicrosoftID))
                    microsoftid = db.Customers.Find(_order.CustomerID).MicrosoftID;


                Orders orders = new Orders();

                if (String.IsNullOrEmpty(microsoftid))
                {
                    microsoftid = "";
                }
                var context = new ScenarioContext();
                var partnerOperations = context.AppPartnerOperations;

                var customerSubscription = partnerOperations.Customers.ById(microsoftid).Subscriptions.Get();

                var customerobj = custRepo.GetCustomer(microsoftid);

                 string _json = JsonConvert.SerializeObject(customerSubscription);
                JToken token = JObject.Parse(_json);
                var _token = token.SelectToken("Items").ToString();

                List<Subscription> subsObj = JsonConvert.DeserializeObject<List<Subscription>>(_token);

                var newList1 = subsObj.Where(m => _order.OItem.Any(z => z.PartnerIdOnRecord == m.PartnerId)).ToList();

                int _count = 0;
                foreach (var item in _order.OItem)
                {
                    //var checkIfExists = newList1.Where(x => x.PartnerId == mpnid && x.OfferId == item.OffersId).FirstOrDefault();
                    //if (checkIfExists != null)
                    //{
                    //    string _subid = db.Subscriptions.FirstOrDefault(s => s.CustomerID == _order.CustomerID && s.OffersId == item.OffersId && s.PartnerId == mpnid).SubscriptionID;
                    //    _UpdateSubscription(_subid, item.Quantity);
                    //    _action = "update";
                    //}

                    //else
                    //{

                    //    OrderItems items = new OrderItems();
                    //    _count++;
                    //    items.OrdersID = orders.ID;
                    //    items.LineItemNumber = _count;
                    //    items.OffersID = item.OffersId;
                    //    items.FriendlyName = "";
                    //    items.Quantity = item.Quantity;
                    //    items.PartnerIdOnRecord = mpnid; //item.PartnerIdOnRecord;
                    //    items.Price = item.Price;

                    //    db.OrderItems.Add(items);
                    //}

                    OrderItems items = new OrderItems();
                    _count++;
                    items.OrdersID = orders.ID;
                    items.LineItemNumber = _count;
                    items.OffersID = item.OffersId;
                    items.FriendlyName = "";
                    items.Quantity = item.Quantity;
                    items.PartnerIdOnRecord = mpnid; //item.PartnerIdOnRecord;
                    items.Price = item.Price;

                    db.OrderItems.Add(items);


                }

                if (_count > 0)
                {
                    orders.OrdersID = _order.OrdersID;
                    orders.CustomersID = _order.CustomerID;
                    orders.MicrosoftID = microsoftid;
                    orders.CreationDate = DateTime.Now;
                    orders.TotalPrice = _order.TotalPrice;
                    orders.TotalVat = _order.TotalVat;
                    orders.TotalAmount = _order.TotalAmount;
                    orders.CreatedDate = DateTime.Now;
                    orders.ResellerID = _order.ResellerID;
                    orders.BillingCycle = _order.BillingCycle;
                    /* July 6, 2018
                     * request for adding billing cycle in order
                     * currently for phase 2

                     */

                    db.Orders.Add(orders);
                    db.SaveChanges();
                    CreateOrdersAPI_sub(orders, customerobj,out strOrdersId,out lErrorMessage);
                    SendEmail(orders.ID);



                    var json = new JavaScriptSerializer().Serialize(_order);
                    //SAVE TO AUDIT TRAIL
                    AuditTrails _audit = new AuditTrails();
                    _audit.Module = "Create Order";
                    _audit.TaskDesc = json;
                    _audit.AuditDate = DateTime.Now;
                    _audit.UserID = Sessions.UserID;
                    db.AuditTrails.Add(_audit);

                    db.SaveChanges();
                }
                //  }



                _order.OrdersID = orders.ID.ToString();





                order = 1;


                ////CALL API
                //if(_action == "create")
                //    CreateOrdersAPI(orders);
                //else
                //UPDATE SUBSCRIPTIONS
                //if (_action == "update")
                //    UpdateOrderSubscription(orders.ID);

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
            }

            return order;
        }

        public void _UpdateSubscription(string subID, int Qty)
        {
            int id = 0;
            try
            {
                Subscriptions sub = new Models.Subscriptions();
                sub = db.Subscriptions.FirstOrDefault(s => s.SubscriptionID == subID);

                if (sub != null)
                {
                    sub.Quantity = Qty;


                    Subscription item = new Subscription();
                    //_orderRepo = new Repositories.OrderRepository();
                    item = UpdateSubscription(sub);

                    //UPDATE SUBSCRIPTION
                    Subscriptions exists = db.Subscriptions.FirstOrDefault(o => o.SubscriptionID == item.Id);
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

                    db.Entry(exists).State = EntityState.Modified;
                    db.SaveChanges();

                    id = exists.CustomerID.Value;
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

           
        }


        public List<Subscriptions> GetSubscriptionPerCustomer(int _customerID)
        {
            List<Subscriptions> subObj = new List<Models.Subscriptions>();
            subObj = db.Subscriptions.Where(c => c.CustomerID == _customerID).ToList();
            return subObj;
        }


        public Subscription UpdateSubscription(Subscriptions _sub)
        {
            var context = new ScenarioContext();
            CreateOrder offers = new CreateOrder(context, null);
            string json = offers.UpdateSubscription(_sub.Customers.MicrosoftID, _sub.SubscriptionID, _sub.Quantity.Value, _sub.Status);
            Subscription sub = JsonConvert.DeserializeObject<Subscription>(json);

            return sub;
        }

    }
}