using Portal.Helpers;
using Portal.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Portal.Controllers
{
    #region User Access Checker
    public class AccessChecker : ActionFilterAttribute
    {
        MSIPortalEntities db = new MSIPortalEntities();

        public int Action { get; set; }
        public int ModuleID { get; set; }

      


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext != null)
            {
                HttpSessionStateBase Session = filterContext.HttpContext.Session;
                if (Session["msiUserTypeID"] == null)
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Account" },
                            { "action", "Login" }
                        });
                }
                else
                {
                    int UserID = System.Convert.ToInt32(Session["msiUserTypeID"]);

                    var access = db.UserAccesses1.FirstOrDefault(u => u.UserTypeID == UserID && u.ModuleID == ModuleID);
                    try
                    {
                        db.Entry(access).Reload();


                        switch (Action)
                        {
                            case 1: //CanView
                                if (!access.CanView.Value)
                                {
                                    filterContext.Result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                    { "controller", "Home" },
                                    { "action", "Denied" }
                                        });
                                }
                                break;

                            case 2: //CanEdit
                                if (!access.CanEdit.Value)
                                {
                                    filterContext.Result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                    { "controller", "Home" },
                                    { "action", "Denied" }
                                        });
                                }
                                break;

                            case 3: //CanDelete
                                if (!access.CanDelete.Value)
                                {
                                    filterContext.Result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                    { "controller", "Home" },
                                    { "action", "Denied" }
                                        });
                                }
                                break;


                            case 4: //CanDecide
                                if (!access.CanDecide.Value)
                                {
                                    filterContext.Result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                    { "controller", "Home" },
                                    { "action", "Denied" }
                                        });
                                }
                                break;

                            default:
                                filterContext.Result = new RedirectToRouteResult(
                                        new RouteValueDictionary
                                        {
                                    { "controller", "Home" },
                                    { "action", "Denied" }
                                        });
                                break;
                        }
                    }
                    catch
                    {
                        filterContext.Result = new RedirectToRouteResult(
                                    new RouteValueDictionary
                                    {
                                    { "controller", "Home" },
                                    { "action", "Denied" }
                                    });

                    }

                }

            }

        }
    }

    //PM FILTER - RESELLER
    public class AuthorizationFilterPM : System.Web.Mvc.ActionFilterAttribute
    {
        public string ModuleName { get; set; }
        public string ViewName { get; set; }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext != null)
            {
                HttpSessionStateBase Session = filterContext.HttpContext.Session;
                if (ModuleName == "Reseller" && Sessions.UserType == "PM" && ViewName != "PendingAccounts")
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Resellers" },
                            { "action", "PendingAccounts" }
                        });
                }
            }
        }
    }
    #endregion
}