using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using Portal.Repositories;
using Portal.Helpers;

namespace Portal.Controllers
{
    [AuthorizationFilter]
    public class EmailTemplatesController : Controller
    {
        private MSIPortalEntities db = new MSIPortalEntities();
        OrderRepository _orderRepo = new OrderRepository();

        // GET: EmailTemplates
        public async Task<ActionResult> Index()
        {
            ViewBag.EmailTemplate = db.EmailTemplates.Select(f => new SelectListItem
            {
                Value = f.ID.ToString(),
                Text = f.EmailType
            });

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(EmailTemplates email)
        {
            ViewBag.EmailTemplate = db.EmailTemplates.Select(f => new SelectListItem
            {
                Value = f.ID.ToString(),
                Text = f.EmailType
            });

            return View();
        }
        public JsonResult GetEmailContent(int? ID)
        {
            var exists = db.EmailTemplates.Where(e=>e.ID == ID.Value).FirstOrDefault();

            return Json(exists, JsonRequestBehavior.AllowGet);

        }
        public JsonResult SaveEmail(EmailTemplates email)
        {
            int id = 0;

            EmailTemplates temp = new Models.EmailTemplates();
            temp = email;

            db.Entry(temp).State = EntityState.Modified;
            db.SaveChanges();


            id = temp.ID;

            return Json(id, JsonRequestBehavior.AllowGet);

        }


        public ActionResult SendTemplate()
        {
            _orderRepo = new OrderRepository();
            _orderRepo.SendEmail(22);
            return View();
        }

        #region Comment
        //// GET: EmailTemplates/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    EmailTemplates emailTemplates = await db.EmailTemplates.FindAsync(id);
        //    if (emailTemplates == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(emailTemplates);
        //}

        //// GET: EmailTemplates/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: EmailTemplates/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "ID,EmailType,EmailContent")] EmailTemplates emailTemplates)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.EmailTemplates.Add(emailTemplates);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(emailTemplates);
        //}

        //// GET: EmailTemplates/Edit/5
        //public async Task<ActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    EmailTemplates emailTemplates = await db.EmailTemplates.FindAsync(id);
        //    if (emailTemplates == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(emailTemplates);
        //}

        //// POST: EmailTemplates/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "ID,EmailType,EmailContent")] EmailTemplates emailTemplates)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(emailTemplates).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(emailTemplates);
        //}

        //// GET: EmailTemplates/Delete/5
        //public async Task<ActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    EmailTemplates emailTemplates = await db.EmailTemplates.FindAsync(id);
        //    if (emailTemplates == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(emailTemplates);
        //}

        //// POST: EmailTemplates/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> DeleteConfirmed(int id)
        //{
        //    EmailTemplates emailTemplates = await db.EmailTemplates.FindAsync(id);
        //    db.EmailTemplates.Remove(emailTemplates);
        //    await db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}

        #endregion
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
