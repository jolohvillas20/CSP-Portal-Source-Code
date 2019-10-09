using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class CustomerOrders
    {
        public Orders Orders { get; set; }
        public List<OrderItems> OrderItems { get; set; }

    }


  
}