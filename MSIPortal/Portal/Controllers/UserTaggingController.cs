using Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Portal.Controllers
{
    public class UserTaggingController : Controller
    {
        private MSIPortalEntities db = new MSIPortalEntities();
        // GET: UserTagging

        [AccessChecker(Action = 1, ModuleID = 6)]
        public ActionResult Tagging(int? ID, string module)
        {
            ViewBag.Action =1;
            Users _users = new Users();
            if (ID != null)
            {
                _users = db.Users.Find(ID);
                ViewBag.Action =2;
            }
           
            ViewBag.Module = "Resellers";
           


            ViewBag.Modules = db.Modules1.Where(p=>p.ID==1);
           


            return View(_users);
          
        }

        [HttpPost]
        public JsonResult GetUser(string user)
        {
            var _user = db.Users.Distinct().Where(x => (x.FirstName.Contains(user) ||  x.LastName.Contains(user)) && x.UserTypeID ==2)
                            .Select(x => new
                            {
                                ID = x.ID,
                                User = x.FirstName + " " + x.LastName
                                
                            });
            return Json(_user, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetReseller(string user)
        {
            var _user = db.Resellers.Distinct().Where(x => x.ResellerName.Contains(user))
                            .Select(x => new
                            {
                                ID = x.ResellerID,
                                User = x.ResellerName

                            });
            return Json(_user, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetOffer(string user)
        {
            var _user = db.Offers.Distinct().Where(x => x.Description.Contains(user))
                            .Select(x => new
                            {
                                ID = x.OffersID,
                                User = x.Name

                            });
            return Json(_user, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetCustomer(string user)
        {
            var _user = db.Customers.Distinct().Where(x => x.FirstName.Contains(user) || x.LastName.Contains(user))
                            .Select(x => new
                            {
                                ID = x.ID,
                                User = x.FirstName + " " + x.LastName

                            });
            return Json(_user, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CheckTagging(int? userID)
        {
            string x = "old";
            Users user = db.Users.Find(userID);
            if (user == null)
            {
                x = "new";
            }
            return Json(x, JsonRequestBehavior.AllowGet);
        }


        public void AddTagging(string Module, int? UsersID, string TaggedID, string TaggedName)
        {
            try
            {
               var _header = db.UserAccessHeader.Where(p => p.Category == Module && p.UserID == UsersID.Value).FirstOrDefault();

               if(_header != null)
                {
                    var _details = db.UserAccessDetails.Where(p => p.ModuleID == _header.ID && p.TaggedID == TaggedID.ToString());

                    if (_details.Count() == 0)
                    {
                        UserAccessDetails _d = new UserAccessDetails();
                        _d.ModuleID = _header.ID;
                        _d.TaggedID = TaggedID.ToString();
                        _d.Name = TaggedName;
                        db.UserAccessDetails.Add(_d);
                        db.SaveChanges();
                    }
                  
                }
               else
                {
                    UserAccessHeader _h = new UserAccessHeader();
                    _h.UserID = UsersID;
                    _h.Category = Module ;
                    db.UserAccessHeader.Add(_h);
                    db.SaveChanges();

                    UserAccessDetails _d = new UserAccessDetails();
                    _d.ModuleID = _h.ID;
                    _d.TaggedID = TaggedID.ToString();
                    _d.Name = TaggedName;
                    db.UserAccessDetails.Add(_d);
                    db.SaveChanges();

                }

            }
            catch (Exception e) { e.ToString(); }
        }

        public void DeleteTagging(int? taggedid)
        {
            try
            {
                UserAccessDetails details = db.UserAccessDetails.Find(taggedid);
                db.UserAccessDetails.Remove(details);
                db.SaveChanges();

            }
            catch { }
        }

    }
}