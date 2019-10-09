using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models.Class
{
    public class clsOrderConfirmation
    {
        public string OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }
        public string OrderProcessed { get; set; }
        public string IndirectReseller { get; set; }
        public string Failed { get; set; }

        public List<OrderItems> lOrderId { get; set; }
       
    }

    public class OrderItems
    {
        public string OrderId { get; set; }
        public List<cLineItems> LineItems { get; set; }

        public class cLineItems
        {
            public string OfferName { get; set; }
            public string Quantity { get; set; }
            public string Category { get; set; }
            public string UnitType { get; set; }
        }
        
    }
}