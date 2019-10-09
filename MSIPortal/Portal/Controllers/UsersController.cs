using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using PagedList;

namespace Portal.Controllers
{
    public class UsersController : Controller
    {
        private MSIPortalEntities db = new MSIPortalEntities();
        UserRepository userRepo = new UserRepository();

        // GET: Users
        [AccessChecker(Action = 1, ModuleID = 6)]
        public async Task<ActionResult> Index(string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortColumn;
            ViewBag.SortOrder = sortOrder == "asc" ? "desc" : "asc";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            try
            {

                IQueryable <Users> _lstUser = userRepo.List().Include(u => u.UserTypes).Where(u => u.UserTypeID != 5).AsQueryable();

                foreach (var u in _lstUser)
                {
                    u.username = userRepo.Decrypt(u.username);
                }

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    _lstUser = _lstUser.Where(s => s.LastName.ToString().ToLower().Contains(searchString)
                                           || s.FirstName.ToString().ToLower().Contains(searchString)
                                           || s.Email.ToLower().Contains(searchString)
                                           || s.UserTypes.UserType.ToLower().Contains(searchString));
                }

                if (String.IsNullOrEmpty(sortColumn))
                {
                    _lstUser = _lstUser.OrderByDescending(p => p.ID);
                }
                else
                {
                    ////if (sortOrder == "usertype")
                    ////{
                    ////    if (sortOrder == "asc")
                    ////        _lstUser = _lstUser.OrderBy(p => p.UserTypes.UserType);
                    ////    else
                    ////        _lstUser = _lstUser.OrderByDescending(p => p.UserTypes.UserType);
                    ////}
                    //else
                        _lstUser = _lstUser.OrderBy(sortColumn + " " + sortOrder);
                }

                int pageSize = 10;
                int pageNumber = (page ?? 1);
                return View(_lstUser.ToPagedList(pageNumber, pageSize));

            }
            catch (Exception e)
            {
                e.ToString();
                TempData["Message"] = "No Assigned Reseller/s";
                return View("Index");

            }

            //ViewBag.CurrentFilter = searchString;
            //var users = db.Users.Include(u => u.UserTypes).Where(u=>u.UserTypeID!=5);
            //return View(await users.ToListAsync());
        }

        // GET: Users/Details/5

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = await db.Users.FindAsync(id);
            users.username = userRepo.Decrypt(users.username);
            users.password = userRepo.Decrypt(users.password);
            if (users == null)
            {
                return HttpNotFound();
            }
            return View(users);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.UserTypeID = new SelectList(db.UserTypes.Where(p => p.id != 5), "id", "UserType");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,username,password,Active,LastName,FirstName,Email,UserTypeID")] Users users)
        {
            if (ModelState.IsValid)
            {
                string username = string.Empty;

                var nameCheck = db.Users.Where(u => u.LastName == users.LastName && u.FirstName == users.FirstName).FirstOrDefault();
                if (nameCheck != null)
                {
                    ModelState.AddModelError("","User already exists.");
                    ViewBag.UserTypeID = new SelectList(db.UserTypes.Where(p => p.id != 5), "id", "UserType", users.UserTypeID);
                    return View(users);
                }

                if (users.username != "")
                {
                    username = userRepo.Encrypt(users.username);
                    var userCheck = db.Users.Where(u => u.username == username).FirstOrDefault();
                    if (userCheck != null)
                    {
                        ModelState.AddModelError("", "Username already exists.");
                        ViewBag.UserTypeID = new SelectList(db.UserTypes.Where(p => p.id != 5), "id", "UserType", users.UserTypeID);
                        return View(users);
                    }
                }

                if (users.Email != "")
                {
                    var userCheck = db.Users.Where(u => u.Email.Trim() == users.Email.Trim()).FirstOrDefault();
                    if (userCheck != null)
                    {
                        ModelState.AddModelError("", "Email already exists.");
                        ViewBag.UserTypeID = new SelectList(db.UserTypes.Where(p => p.id != 5), "id", "UserType", users.UserTypeID);
                        return View(users);
                    }
                }

                users.Active = true;
                users.username = username;
                db.Users.Add(users);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.UserTypeID = new SelectList(db.UserTypes.Where(p => p.id != 5), "id", "UserType", users.UserTypeID);
            return View(users);
        }

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            Users users = await db.Users.FindAsync(id);
            users.username = userRepo.Decrypt(users.username);
            users.password = userRepo.Decrypt(users.password);
            if (users == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.UserTypeID = new SelectList(db.UserTypes.Where(p=>p.id!=5), "id", "UserType", users.UserTypeID);
            return View(users);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,username,password,Active,LastName,FirstName,Email,UserTypeID")] Users users)
        {
            if (ModelState.IsValid)
            {
                string username = string.Empty;
                string password = string.Empty;

                var nameCheck = db.Users.Where(u => u.LastName == users.LastName && u.FirstName == users.FirstName && u.ID != users.ID).FirstOrDefault();
                if (nameCheck != null)
                {
                    ModelState.AddModelError("", "User already exists.");
                    ViewBag.UserTypeID = new SelectList(db.UserTypes.Where(p => p.id != 5), "id", "UserType", users.UserTypeID);
                    return View(users);
                }

                if (users.username != "")
                {
                    username = userRepo.Encrypt(users.username);
                    password = userRepo.Encrypt(users.password);
                    var userCheck = db.Users.Where(u => u.username == username && u.ID != users.ID).FirstOrDefault();
                    if (userCheck != null)
                    {
                        ModelState.AddModelError("", "Username already exists.");
                        ViewBag.UserTypeID = new SelectList(db.UserTypes.Where(p => p.id != 5), "id", "UserType", users.UserTypeID);
                        return View(users);
                    }
                }



                users.username = username;
                users.password = password;
                db.Entry(users).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.UserTypeID = new SelectList(db.UserTypes.Where(p => p.id != 5), "id", "UserType", users.UserTypeID);
            return View(users);
        }

        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            Users users = await db.Users.FindAsync(id);
            users.username = userRepo.Decrypt(users.username);
            users.password = userRepo.Decrypt(users.password);

            if (users == null)
            {
                return RedirectToAction("Index");
            }
            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Users users = await db.Users.FindAsync(id);
            try
            {              
                db.Users.Remove(users);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch 
            {
                //SetAsInactive(id);
                TempData["Error"] = "User cannot be deleted due to connected transactions";
                //ModelState.AddModelError("","User cannot be deleted due to connected transactions");
                return View(users);
            }
           
        }

        public void SetAsInactive(int id)
        {
            Users users =  db.Users.Find(id);
            users.Active = false;
             db.SaveChanges();

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
