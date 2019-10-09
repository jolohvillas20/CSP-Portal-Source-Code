using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using Portal.Helpers;
using PagedList;

namespace Portal.Controllers
{
    [AuthorizationFilter]
    public class TicketsController : Controller
    {
        private MSIPortalEntities db = new MSIPortalEntities();

        ResellerRepository resellerRepo = new ResellerRepository();
        Mailer _mailer = new Mailer();


        [AccessChecker(Action = 1, ModuleID = 5)]
        public ActionResult Index(string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (Sessions.UserType == "Admin" || Sessions.UserType == "PM" || Sessions.UserType == "PA")
                return RedirectToAction("Admin");

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


            ViewBag.CurrentFilter = searchString;
            int userID = Convert.ToInt32(Session["msiUserID"].ToString());


            try
            {


                var tickets = (from Ticket in db.Tickets.ToList()
                               join Reseller in db.Resellers.ToList()
                               on Ticket.TicketSenderID equals Reseller.UsersID into lst
                               from l in lst
                               select new TicketAdmin
                               {
                                   TicketID = Ticket.TicketID,
                                   TicketSenderID = Ticket.TicketSenderID,
                                   Title = Ticket.Title,
                                   TicketDate = Ticket.TicketDate,
                                   Status = Ticket.Status,
                                   Description = Ticket.Description
                               }).ToList();

                if (Sessions.UserType == "Sales AE")
                {
                    var __reseller = db.UserAccessDetails.Where(a => a.UserAccessHeader.UserID == Sessions.UserID).Select(a => a.TaggedID);
                    var _mpnid = db.Resellers.Where(p => __reseller.Contains(p.ResellerID.ToString())).Select(q => q.UsersID.ToString());
                    tickets = tickets.Where(p => _mpnid.Contains(p.TicketSenderID.ToString())).ToList();
                }

                else
                {
                    tickets = tickets.Where(t => t.TicketSenderID == userID).ToList();
                }

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();

                    tickets = tickets.Where(s => s.Description.ToLower().Contains(searchString)
                                               || s.TicketDate.ToString().ToLower().Contains(searchString)
                                               || s.Title.ToLower().Contains(searchString)
                                               || s.Status.ToLower().Contains(searchString)).ToList();
                }


                if (String.IsNullOrEmpty(sortColumn))
                {
                    tickets = tickets.OrderByDescending(p => p.TicketID).ToList();
                }
                else
                {
                    tickets = tickets.OrderBy(sortColumn + " " + sortOrder).ToList();
                }

                var pager = new Pager(tickets.Count(), page);

                var viewModel = new TicketViewModel
                {
                    Tickets = tickets.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                    Pager = pager
                };

                return View(viewModel);

            }
            catch
            {
                return View("Index", "Home");
            }
        }

        public ActionResult Admin(string sortColumn, string sortOrder, string currentFilter, string searchString, int? page)
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


            ViewBag.CurrentFilter = searchString;


            var tickets = (from Ticket in db.Tickets.ToList()
                           select new TicketAdmin
                           {
                               TicketID = Ticket.TicketID,
                               TicketSenderID = Ticket.TicketSenderID,
                               Title = Ticket.Title,
                               TicketDate = Ticket.TicketDate,
                               Status = Ticket.Status,
                               Description = Ticket.Description
                       
                           }).ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                tickets = tickets.Where(s => s.Description.ToLower().Contains(searchString)
                                           || s.TicketDate.ToString().ToLower().Contains(searchString)
                                           || s.Title.ToLower().Contains(searchString)
                                           || s.Status.ToLower().Contains(searchString)).ToList();
            }


            if (String.IsNullOrEmpty(sortColumn))
            {
                tickets = tickets.OrderByDescending(p => p.TicketID).ToList();
            }
            else
            {
                tickets = tickets.OrderBy(sortColumn + " " + sortOrder).ToList();
            }

            var pager = new Pager(tickets.Count(), page);

            var viewModel = new TicketViewModel
            {
                Tickets = tickets.Skip((pager.CurrentPage - 1) * pager.PageSize).Take(pager.PageSize),
                Pager = pager
            };

            return View(viewModel);
        }


        [AccessChecker(Action = 1, ModuleID = 5)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tickets tickets = db.Tickets.Find(id);
            Resellers resellers = db.Resellers.FirstOrDefault(p=>p.UsersID == tickets.TicketSenderID);

            ResellerTicket resellerticket = new ResellerTicket();
            if (resellers != null)
            {
                resellerticket.SenderName = resellers.ResellerName;
                resellerticket.MPNID = resellers.MPNID.ToString();
            }
            else
            {
                Users user = db.Users.Find(tickets.TicketSenderID);
                resellerticket.SenderName = user.FirstName + " " + user.LastName;
                resellerticket.MPNID = "";
            }

            resellerticket.Ticket = tickets;

            if (resellerticket == null)
            {
                return HttpNotFound();
            }
            return View(resellerticket);
        }

        [AccessChecker(Action = 2, ModuleID = 5)]
        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessChecker(Action = 2, ModuleID = 5)]
        public ActionResult Create([Bind(Include = "TicketID,Title,Description,Status,TicketDate,TicketSenderID")] Tickets tickets)
        {
            if (ModelState.IsValid)
            {
                tickets.TicketDate = DateTime.Now;
                tickets.TicketSenderID =Convert.ToInt32(Session["msiUserID"].ToString());
                tickets.Status = "New";
                db.Tickets.Add(tickets);
                db.SaveChanges();
                if (Sessions.UserType == "Reseller")
                {
                    try
                    {
                        int id = db.Resellers.FirstOrDefault(p => p.ResellerID == Sessions.ResellerID).AuthorizedRepresentative.Value;

                        var _authorizedemail = db.AuthorizedRepresentatives.Find(id);

                        _mailer.SendTicket(tickets.Title, tickets.Description, resellerRepo.GetResellerName(tickets.TicketSenderID) + "( " + Sessions.ResellerMPNID.ToString() + " )");
                        _mailer.SendEmailToAllAdmin("Ticket", "New Ticket" + tickets.Title, Sessions.ResellerName);
                        _mailer.SendEmailToAuthorized("Ticket", "New Ticket" + tickets.Title,Sessions.ResellerName, _authorizedemail.EmailAddress);
                    }
                    catch { }
                }


                //SAVE TO AUDIT TRAIL
                AuditTrails _audit = new AuditTrails();
                _audit.Module = "Creare Ticket";
                _audit.TaskDesc = "Ticket ID:" + tickets.TicketID + " " + "/" + " " + "Reseller name:" + resellerRepo.GetResellerName(tickets.TicketSenderID);
                _audit.AuditDate = DateTime.Now;
                _audit.UserID = Sessions.UserID;
                db.AuditTrails.Add(_audit);
                db.SaveChanges();

                
                return RedirectToAction("Index");
            }

            return View(tickets);
        }

        [AccessChecker(Action = 2, ModuleID = 5)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tickets tickets = db.Tickets.Find(id);
            Resellers resellers = db.Resellers.FirstOrDefault(p=>p.UsersID == tickets.TicketSenderID);

            ResellerTicket resellerticket = new ResellerTicket();
            if (resellers != null)
            {
                resellerticket.SenderName = resellers.ResellerName;
                resellerticket.MPNID = resellers.MPNID.ToString();
                ViewBag.UsersID = new SelectList(db.Users, "ID", "username", resellers.UsersID);
            }
            else
            {
                Users user = db.Users.Find(tickets.TicketSenderID);
                resellerticket.SenderName = user.FirstName +" "+user.LastName;
                resellerticket.MPNID = "";
                ViewBag.UsersID = new SelectList(db.Users, "ID", "username", user.ID);
            }

         
            resellerticket.Ticket = tickets;

            if (resellerticket == null)
            {
                return HttpNotFound();
            }
           
            return View(resellerticket);
        }

        [AccessChecker(Action = 3, ModuleID = 5)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tickets ticket = db.Tickets.Find(id);
            Resellers resellers = db.Resellers.FirstOrDefault(p=>p.UsersID == ticket.TicketSenderID);
           
            ResellerTicket resellerticket = new ResellerTicket();
            if (resellers != null)
            {
                resellerticket.SenderName = resellers.ResellerName;
                resellerticket.MPNID = resellers.MPNID.ToString();
            }
            else
            {
                Users user = db.Users.Find(ticket.TicketSenderID);
                resellerticket.SenderName = user.FirstName + " " + user.LastName;
                resellerticket.MPNID = "";
            }

            if (resellerticket == null)
            {
                return HttpNotFound();
            }
            return View(resellerticket);
        }

        // POST: Resellers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AccessChecker(Action = 3, ModuleID = 5)]
        public ActionResult DeleteConfirmed(int id)
        {
            Tickets tickets = db.Tickets.Find(id);
           
            db.Tickets.Remove(tickets);
            db.SaveChanges();
            return RedirectToAction("Admin");
        }

        public void Save(TicketDTO entry)
        {
            if (entry.TicketID == 0)
            {

            }

            else
            {
                Tickets ticket = db.Tickets.Find(entry.TicketID);

              
                ticket.Status = entry.Status;

                db.SaveChanges();

                _mailer.SendReplyTicket(ticket.Title,"updated", resellerRepo.GetResellerName(ticket.TicketSenderID),resellerRepo.GetResellerEmails(ticket.TicketSenderID));

            }
        }

        public void DeleteTicketDetails(int? ticketid)
        {
            try
            {
                TicketDetails ticketdetails = db.TicketDetails.Find(ticketid);
                db.TicketDetails.Remove(ticketdetails);
                db.SaveChanges();

            }
            catch { }
        }

        public void SendTicketRemarks(string ticketremarks, int? ticketID)
        {
            try
            {
                if (Sessions.UserType == "Reseller")
                {
                    Users _user = db.Users.FirstOrDefault(p => p.ID == Sessions.UserID.Value);
                    Tickets _ticket = db.Tickets.Find(ticketID.Value);


                    TicketDetails ticketdetails = new TicketDetails();
                    ticketdetails.Remarks = ticketremarks;
                    ticketdetails.TicketID = ticketID.Value;
                    ticketdetails.Date = DateTime.Now;
                    ticketdetails.EditedBy = Sessions.UserID.Value;
                    ticketdetails.Users = _user;
                    ticketdetails.Tickets = _ticket;
                    db.TicketDetails.Add(ticketdetails);
                    db.SaveChanges();

                    Tickets ticket = db.Tickets.Find(ticketID.Value);
                    resellerRepo.GetResellerName(ticket.TicketSenderID);


                    var users = db.Users.Where(p => p.UserTypeID == 1).Select(p => p.Email);

                    foreach (var user in users)
                    {
                        _mailer.SendEmail_ForTicketRemarks(ticket.Title + "-from " + resellerRepo.GetResellerName(ticket.TicketSenderID),
                                                       ticketdetails.Remarks, ticketdetails.Users.FirstName, user);
                    }
                }
                else

                {

                    Users _user = db.Users.FirstOrDefault(p => p.ID == Sessions.UserID.Value);
                    Tickets _ticket = db.Tickets.Find(ticketID.Value);


                    TicketDetails ticketdetails = new TicketDetails();
                    ticketdetails.Remarks = ticketremarks;
                    ticketdetails.TicketID = ticketID.Value;
                    ticketdetails.Date = DateTime.Now;
                    ticketdetails.EditedBy = Sessions.UserID.Value;
                    ticketdetails.Users = _user;
                    ticketdetails.Tickets = _ticket;
                    db.TicketDetails.Add(ticketdetails);
                    db.SaveChanges();

                    Tickets ticket = db.Tickets.Find(ticketID.Value);
                    resellerRepo.GetResellerName(ticket.TicketSenderID);


                    var users = db.Users.Where(p => p.UserTypeID == 1).Select(p => p.Email);

                    foreach (var user in users)
                    {
                        _mailer.SendEmail_ForTicketRemarks(ticket.Title + "-from " + resellerRepo.GetResellerName(ticket.TicketSenderID),
                                                       ticketdetails.Remarks, ticketdetails.Users.FirstName+ " "+ ticketdetails.Users.LastName, user);
                    }


                }


            }
            catch(Exception e) { e.ToString(); }
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
