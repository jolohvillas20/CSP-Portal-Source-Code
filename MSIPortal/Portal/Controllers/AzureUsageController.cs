using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{
    public class AzureUsageController : Controller
    {
        // GET: AzureUsage
        public ActionResult Index()
        {
            ViewBag.Title = "";
            return View();
        }
    }
}