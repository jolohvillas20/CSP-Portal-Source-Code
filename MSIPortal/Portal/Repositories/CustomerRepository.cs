using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Store.PartnerCenter.Models.Customers;
using Microsoft.Store.PartnerCenter.Models;
using Microsoft.Store.PartnerCenter.Samples.Context;
using Microsoft.Store.PartnerCenter.Samples.Customers;
using Microsoft.Store.PartnerCenter.Agreements;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Portal.Models;
using Microsoft.Store.PartnerCenter.Samples.Offers;
using Microsoft.Store.PartnerCenter.Models.Agreements;
using Microsoft.Store.PartnerCenter.Models.Subscriptions;
using Microsoft.Store.PartnerCenter.Models.Query;
using Microsoft.Store.PartnerCenter.Exceptions;
using Microsoft.Store.PartnerCenter;

namespace Portal.Repositories
{
    public class CustomerRepository
    {
      
        MSIPortalEntities db = new MSIPortalEntities();
        public Customer CreateCustomer(Customer _customer, int specialQualificationsID)
        {
            Customer customerToCreate = new Customer();

            CustomerCompanyProfile CompanyProfile = new CustomerCompanyProfile();
            CompanyProfile.Domain = _customer.CompanyProfile.Domain + ".onmicrosoft.com";


            Address DefaultAddress = new Address();
            DefaultAddress.FirstName = _customer.BillingProfile.DefaultAddress.FirstName;
            DefaultAddress.LastName = _customer.BillingProfile.DefaultAddress.LastName;
            DefaultAddress.AddressLine1 = _customer.BillingProfile.DefaultAddress.AddressLine1;
            DefaultAddress.City = _customer.BillingProfile.DefaultAddress.City;
            DefaultAddress.State = _customer.BillingProfile.DefaultAddress.State;
            DefaultAddress.Country = "PH";
            DefaultAddress.PostalCode = _customer.BillingProfile.DefaultAddress.PostalCode;
            DefaultAddress.PhoneNumber = _customer.BillingProfile.DefaultAddress.PhoneNumber;

            CustomerBillingProfile BillingProfile = new CustomerBillingProfile();
            BillingProfile.Culture = "EN-PH";
            BillingProfile.Email = _customer.BillingProfile.Email;
            BillingProfile.Language = "En";
            BillingProfile.CompanyName = _customer.BillingProfile.CompanyName;
            BillingProfile.DefaultAddress = DefaultAddress;


            customerToCreate.BillingProfile = BillingProfile;
            customerToCreate.CompanyProfile = CompanyProfile;

            var context = new ScenarioContext();
            CreateCustomer customers = new CreateCustomer(context, customerToCreate);

            //customers.Run();

            var partnerOperations = context.AppPartnerOperations;
            var newCustomer = partnerOperations.Customers.Create(customerToCreate);


            if (specialQualificationsID == 2)
            {
                var customerQualification = partnerOperations.Customers.ById(newCustomer.Id).Qualification.Update(CustomerQualification.Education);
            }

            return newCustomer;
        }

        //private void DeleteCustomer(string cID)
        //{
        //    var context = new ScenarioContext();
        //    GetOffers offers = new GetOffers(context);
        //    string json = offers.de


        //}

        public List<Customers> GetCustomers()
        {
            //var context = new ScenarioContext();
            //GetPagedCustomers customers = new GetPagedCustomers(context, 25);
            //string json = customers.GetCustomers();
            //JToken token = JObject.Parse(json);
            //var _token = token.SelectToken("Items").ToString();

            //List<Customer> cusObj = JsonConvert.DeserializeObject<List<Customer>>(_token);

            var cusObj = db.Customers.ToList();

            return cusObj;
        }
        public List<Customer> GetCustomers(string searchString, int page, int pageSize)
        {
            var context = new ScenarioContext();
            GetPagedCustomers customers = new GetPagedCustomers(context, 25);
            string json = customers.GetCustomers();
            JToken token = JObject.Parse(json);
            var _token = token.SelectToken("Items").ToString();

            List<Customer> cusObj = JsonConvert.DeserializeObject<List<Customer>>(_token);

            return cusObj;
        }

        public Customers GetCustomerDetails(string MicrosoftID, int _customerID)
        {
            Customers cusObj = new Models.Customers();
            if (MicrosoftID == "" || MicrosoftID == null)
            {
                cusObj = db.Customers.Where(c => c.ID == _customerID).FirstOrDefault();
            }
            else
            {
                cusObj = db.Customers.Where(c => c.MicrosoftID == MicrosoftID).FirstOrDefault();
            }
                return cusObj;
        }

        public string CheckDomain(string _domain)
        {
            //_domain = _domain + ".onmicrosoft.com";
            string _available = "false";
            var context = new ScenarioContext();
            GetPagedCustomers customers = new GetPagedCustomers(context, 25);
            _available = customers.CheckDomainAvailable(_domain);

            return _available;
        }

        public string VerifyMPNID(string _mpnID)
        {
            string _available = "";
            try
            {
                var context = new ScenarioContext();
                GetPagedCustomers customers = new GetPagedCustomers(context, 25);
                var json = customers.VerifyMPNID(_mpnID);
                JToken token = JObject.Parse(json);
                _available = token.SelectToken("PartnerName").ToString();
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }
          
            return _available;
        }

        public void DeleteCustomer(string cID)
        {
            var context = new ScenarioContext();
            var partnerOperations = context.AppPartnerOperations;
            var customerId = cID;
            partnerOperations.Customers.ById(customerId).Delete();
        }

        public Customer GetCustomer(string cID)
        {
            var context = new ScenarioContext();
            var partnerOperations = context.AppPartnerOperations;
            var customerId = cID;
           return partnerOperations.Customers.ById(customerId).Get();
        }

        public ResourceCollection<Agreement> GetCustomerAgreements(string microsoftId)
        {
            var context = new ScenarioContext();
            var partnerOperations = context.AppPartnerOperations;
            ResourceCollection<Agreement> customerAgreements = partnerOperations.Customers.ById(microsoftId).Agreements.Get();


            return customerAgreements;

        }

        public Customer GetCustomerDetails(string microsoftId)
        {
            var context = new ScenarioContext();
            var partnerOperations = context.AppPartnerOperations;

            var customerDetails = partnerOperations.Customers.ById(microsoftId).Get();

            return customerDetails;

        }

        public CustomerQualification GetCustomerQualification(string microsoftId)
        {
            var context = new ScenarioContext();
            var partnerOperations = context.AppPartnerOperations;

            var customerQualification = partnerOperations.Customers.ById(microsoftId).Qualification.Get();

            return customerQualification;

        }

        public SeekBasedResourceCollection<Customer> GetPageCustomers()
        {
            var context = new ScenarioContext();

            IAggregatePartner partnerOperations = null;

            try
            {
                partnerOperations = context.AppPartnerOperations;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }


            var customerPageSize = 100;

            var customersPage = (customerPageSize <= 0) ? partnerOperations.Customers.Get() : partnerOperations.Customers.Query(QueryFactory.Instance.BuildIndexedQuery(customerPageSize));

            return customersPage;

        }

        public ResourceCollection<Subscription> GetCustomersSubscriptions(string microsoftId,out string errorMsg)
        {
            errorMsg = "";
            ResourceCollection<Subscription> customerSubscription = null;
            try
            {
                var context = new ScenarioContext();
                var partnerOperations = context.AppPartnerOperations;

                customerSubscription = partnerOperations.Customers.ById(microsoftId).Subscriptions.Get();
            }
            catch (PartnerException ex)
            {
                errorMsg = ex.Message;
            }


            return customerSubscription;
        }

        public void CreateCustomerAgreement(string microsoftId,string selectedUserId,string firstName,string lastname,string email,string phonenumber,string agreementDate)
        {
            var context = new ScenarioContext();
            var partnerOperations = context.AppPartnerOperations;
            var agreementTemplateId = "998b88de-aa99-4388-a42c-1b3517d49490";

            var agreement = new Agreement
            {
                UserId = selectedUserId,
                DateAgreed = DateTime.Parse(agreementDate),
                Type = AgreementType.MicrosoftCloudAgreement,
                TemplateId = agreementTemplateId,
                PrimaryContact = new Microsoft.Store.PartnerCenter.Models.Agreements.Contact
                {
                    FirstName = firstName,
                    LastName = lastname,
                    Email = email,
                    PhoneNumber = phonenumber
                }
            };

            var newlyCreatedagreement = partnerOperations.Customers.ById(microsoftId).Agreements.Create(agreement);

            Console.Write(newlyCreatedagreement);
            //newlyCreatedagreement.

        }

    }
}