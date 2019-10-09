using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class ResellerAccount
    {
        public Resellers Reseller { get; set; }
        public Users User { get; set; }
        public AuthorizedRepresentatives AuthorizedRepresentative { get; set; }
    }
}