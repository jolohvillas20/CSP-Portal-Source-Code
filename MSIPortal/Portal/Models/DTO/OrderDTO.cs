using Microsoft.Store.PartnerCenter.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class OrderDTO
    {
        public string CustomerID { get; set; }
        public string OfferID { get; set; }
        public int ResellerID { get; set; }
        public string OfferSalesGroup { get; set; }

        

        public string CustomerName { get; set; }
        public string OfferName { get; set; }
        public string FriendlyName { get; set; }
        public int Quantity { get; set; }
        public string ResellerName { get; set; }
    }
    public class ItemsOrder
    {
        public List<OrderDTO> Items { get; set; }

    }

    public class OrderItemsDTO
    {
        public class OItems
        {
            public int OrdersID { get; set; }
            public int LineItemNumber { get; set; }

            public string OffersId { get; set; }
            public string SubscriptionID { get; set; }
            public string FriendlyName { get; set; }
            public int Quantity { get; set; }
            public string PartnerIdOnRecord { get; set; }
            public decimal Price { get; set; }

        }

        public int ID { get; set; }

        public string OrdersID { get; set; }

        public int CustomerID { get; set; }
        public string MicrosoftID { get; set; }
        public DateTime CreationDate { get; set; }
        public string BillingCycle { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalAmount { get; set; }

        public int ResellerID { get; set; }

        public List<OItems> OItem { get; set; }

    }

    //public class OrderClass
    //{
    //    public string customerId { get; set; }

    //    public Order Order { get; set; }

    //}
}