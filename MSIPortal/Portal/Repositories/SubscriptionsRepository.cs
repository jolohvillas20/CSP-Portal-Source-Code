using Microsoft.Store.PartnerCenter;
using Microsoft.Store.PartnerCenter.Models;
using Microsoft.Store.PartnerCenter.Samples.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Store.PartnerCenter.Models.Subscriptions;

namespace Portal.Repositories
{
    public class SubscriptionsRepository
    {

        public ResourceCollection<Subscription> GetSubscriptionByOrder(string strCustomerId,string strOrderId)
        {
            var context = new ScenarioContext();

            IAggregatePartner partnerOperations = null;

            partnerOperations = context.AppPartnerOperations;


            var customerSubscription = partnerOperations.Customers.ById(strCustomerId).Subscriptions.ByOrder(strOrderId).Get();

            return customerSubscription;

        }

      
    }
}