using Microsoft.Store.PartnerCenter.Models.Offers;
using Microsoft.Store.PartnerCenter.Samples.Context;
using Microsoft.Store.PartnerCenter.Samples.Offers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Repositories
{
    public class OfferRepository
    {
        MSIPortalEntities db = new MSIPortalEntities();


        public List<Offers> GetOffers()
        {
            //var context = new ScenarioContext();
            //GetOffers offers = new GetOffers(context);
            //string json = offers.GetOffersAll();
            //JToken token = JObject.Parse(json);
            //var _token = token.SelectToken("Items").ToString();

            //List<Offer> offerObj = JsonConvert.DeserializeObject<List<Offer>>(_token);
            List<Offers> offerObj = new List<Offers>();
            offerObj = db.Offers.Where(o => o.DealersPrice > 0).OrderBy(o=>o.Name).ToList();

            return offerObj;
        }
        //public List<Offer> GetOffersPerCustomer(string _customerID)
        //{
        //    var context = new ScenarioContext();
        //    GetOffers offers = new GetOffers(context);
        //    string json = offers.GetOffersPerCustomerID(_customerID);
        //    JToken token = JObject.Parse(json);
        //    var _token = token.SelectToken("Items").ToString();

        //    List<Offer> offerObj = JsonConvert.DeserializeObject<List<Offer>>(_token);

        //    return offerObj;
        //}


        public List<Catalog> GetCatalog(int specialqualification)
        {
            List<Catalog> catalog = new List<Models.Catalog>();

            var category = db.Offers.Where(c => c.Category != "Trial").Select(c => c.Category).Distinct().ToList();
            if (specialqualification != 2)
                 category = db.Offers.Where(c => c.Category != "Trial" && c.Category != "Education").Select(c => c.Category).Distinct().ToList();


            foreach (var item in category)
            {
                if (item != "")
                {
                    Catalog cat = new Catalog();
                    cat.Category = item;
                    cat.Offers = db.Offers.Where(c => c.Category == item && c.DealersPrice > 0).OrderBy(o=>o.Name).ToList();

                    catalog.Add(cat);
                }
            }

            return catalog;
        }

        public Offer GetOffer(string param_OfferId)
        {
            ScenarioContext context = new ScenarioContext();
            var partnerOperations = context.AppPartnerOperations;

            var offer = partnerOperations.Offers.ByCountry("PH").ById(param_OfferId).Get();

            return offer;
        }

        public List<_Categories> GetCatalogwithOrder(int id, string resellerid)
        {
            List<_Categories> catalog = new List<Models._Categories>();

            string qualification = db.Customers.Find(id).SpecialQualifications.Name;

            var _offer = (from p in db.Offers.Where(o=>o.DealersPrice > 0)
                          select new
                          {
                              OfferID = p.OffersID,
                              Category = p.Category,
                              OfferName = p.Name,
                              OfferDescription = p.Description,
                              MinQuantity = p.MinimumQuantity,
                              OfferUnitType = p.UnitType,
                              OfferDealersPrice = p.DealersPrice,

                          }).ToList();

            if (resellerid == "0" || resellerid == null)
                resellerid = null;
            else
            {
                int _id = Convert.ToInt32(resellerid);
                resellerid = db.Resellers.Find(_id).MPNID.ToString();
            }

            var _order = (from p in db.Subscriptions.Where(o=>o.CustomerID == id && o.PartnerId == resellerid).ToList()
                          select new
                          {
                              OfferID = p.OffersId,
                              Quantity = p.Quantity,
                              CustomerID = p.CustomerID,
                              PartnerRecord = p.PartnerId
                          }).ToList();


            var _result = (from item in _offer
                           from i in _order.Where(i => i.OfferID == item.OfferID).DefaultIfEmpty()
                           select new CatalogwithOrders
                           {
                               OffersID = item.OfferID,
                               category = item.Category,
                               Description = item.OfferDescription,
                               Name = item.OfferName,
                               //ComputedDealersPrice = i == null ? item.OfferDealersPrice : i.Quantity * item.OfferDealersPrice,
                               ComputedDealersPrice = i == null ? item.OfferDealersPrice : i.Quantity * item.OfferDealersPrice,
                               DealersPrice = item.OfferDealersPrice,
                               UnitType = item.OfferUnitType,
                               MinimumQuantity = item.MinQuantity,
                               //Quantity = i == null ? item.MinQuantity : i.Quantity,
                               Quantity = i == null ? item.MinQuantity: item.MinQuantity,

                               CustomerID = i == null ? 0 : i.CustomerID.Value,
                               PartnerOnRecord = i == null ? "none" : i.PartnerRecord
                               //Checked = i == null ? " " : "checked"


                           }).ToList();

            var _category = _result.Where(c => c.category != "Trial").Select(c => c.category).Distinct().ToList();
            if(qualification != "Education")
                _category = _result.Where(c => c.category != "Trial" && c.category != "Education").Select(c => c.category).Distinct().ToList();


            foreach (var item in _category)
            {
                if (item != "")
                {
                    _Categories cat = new _Categories();
                    cat.Category = item;
                    cat.OrderItems = _result.Where(c => c.category == item).OrderBy(o => o.Name).ToList();

                    catalog.Add(cat);
                }
            }

            return catalog;
        }

    }
}