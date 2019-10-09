using Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Portal.Helpers
{
    public static class Sessions
    {
        public static int? UserID
        {
            get
            {
                if (HttpContext.Current.Session["msiUserID"] == null)
                {
                    return null;
                }
                return Convert.ToInt32(HttpContext.Current.Session["msiUserID"]);
            }
            set
            {
                HttpContext.Current.Session["msiUserID"] = value;
            }
        }

        public static int? UserTypeID
        {
            get
            {
                if (HttpContext.Current.Session["msiUserTypeID"] == null)
                {
                    return null;
                }
                return Convert.ToInt32(HttpContext.Current.Session["msiUserTypeID"]);
            }
            set
            {
                HttpContext.Current.Session["msiUserTypeID"] = value;
            }
        }

        public static int? ResellerStatus
        {
            get
            {
                if (HttpContext.Current.Session["msiResellerStatus"] == null)
                {
                    return null;
                }
                return Convert.ToInt32(HttpContext.Current.Session["msiResellerStatus"]);
            }
            set
            {
                HttpContext.Current.Session["msiResellerStatus"] = value;
            }
        }

        public static string UserType
        {
            get
            {
                if (HttpContext.Current.Session["msiUserType"] == null)
                {
                    return null;
                }
                return HttpContext.Current.Session["msiUserType"].ToString();
            }
            set
            {
                HttpContext.Current.Session["msiUserType"] = value;
            }
        }
        public static string ResellerMPNID
        {
            get
            {
                if (HttpContext.Current.Session["ResellerMPNID"] == null)
                {
                    return null;
                }
                return HttpContext.Current.Session["ResellerMPNID"].ToString();
            }
            set
            {
                HttpContext.Current.Session["ResellerMPNID"] = value;
            }
        }

        public static int? ResellerID
        {
            get
            {
                if (HttpContext.Current.Session["msiResellerID"] == null)
                {
                    return null;
                }
                return Convert.ToInt32(HttpContext.Current.Session["msiResellerID"]);
            }
            set
            {
                HttpContext.Current.Session["msiResellerID"] = value;
            }
        }
        public static string ResellerName
        {
            get
            {
                if (HttpContext.Current.Session["msiResellerName"] == null)
                {
                    return null;
                }
                return HttpContext.Current.Session["msiResellerName"].ToString();
            }
            set
            {
                HttpContext.Current.Session["msiResellerName"] = value;
            }
        }

        public static bool? IsActive
        {
            get
            {
                if (HttpContext.Current.Session["IsActive"] == null)
                {
                    return null;
                }
                return Convert.ToBoolean(HttpContext.Current.Session["IsActive"]);
            }
            set
            {
                HttpContext.Current.Session["IsActive"] = value;
            }
        }

    }

    public class AuthorizationFilter : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext != null)
            {
                HttpSessionStateBase Session = filterContext.HttpContext.Session;
                if (Session["msiUserID"] == null)
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Home" },
                            { "action", "Login" }
                        });
                }
            }
        }
    }
}