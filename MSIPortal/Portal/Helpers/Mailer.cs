using Microsoft.Store.PartnerCenter.Models.Orders;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Portal.Helpers
{
    public class Mailer
    {
        private MSIPortalEntities db = new MSIPortalEntities();
        string email = ConfigurationManager.AppSettings["wsi_email"].ToString();
        string password = ConfigurationManager.AppSettings["wsi_password"];
        string sender = ConfigurationManager.AppSettings["wsi_sender"].ToString();

        Logger logger = new Logger();
        public string EmailLayout(string Title, string Content, string Variable)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Mail/Email.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{Title}", Title);
            body = body.Replace("{Content}", Content);
            body = body.Replace("{Variable}", Variable);

            return body;
        }

        public string MicrosoftCredentialsEmailContent(List<OrderLineItem> item, Microsoft.Store.PartnerCenter.Models.Customers.Customer cust)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Mail/MicrosoftCredentialsEmailContent.html")))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{DOMAIN NAME}", cust.CompanyProfile.Domain);
            body = body.Replace("{USERNAME}", cust.UserCredentials.UserName);
            body = body.Replace("{PASSWORD}", cust.UserCredentials.Password);

            //body = body.Replace("{USERNAME}", "rjbdc");
            //body = body.Replace("{PASSWORD}", "password");

            body = body.Replace("{CUSTOMER NAME}", cust.BillingProfile.DefaultAddress.FirstName);

            string _details = "", _qty = "", _product = "";
            foreach (var i in item)
            {
                string desc = i.FriendlyName;
                string qty = i.Quantity.ToString();
                _product += desc + " ,";
                _details += desc + "<br />";
                _qty += qty + "<br />";
            }
            // body = body.Replace("{PURCHASE PRODUCT}", _product);
            body = body.Replace("{SUBSCRIPTION}", _details);
            body = body.Replace("{QUANTITY}", _qty);

            return body;
        }

        public void SendCustomerCredentials(string emails, List<OrderLineItem> item, Microsoft.Store.PartnerCenter.Models.Customers.Customer customer) // for newly added Customer
        {
            try
            {
                WebClient client = new WebClient();
                NameValueCollection values = new NameValueCollection();
                values.Add("username", email);
                values.Add("api_key", password);
                values.Add("from", email);
                values.Add("from_name", sender);
                values.Add("subject", "Customer Credentials from WSI");
                values.Add("body_html", MicrosoftCredentialsEmailContent(item, customer));
                values.Add("to", emails);
                //byte[] response = 
                client.UploadValues("https://api.elasticemail.com/mailer/send", values);

                //MailMessage mailMessage = new MailMessage(new MailAddress(email, sender), new MailAddress(emails));
                //mailMessage.Subject = "Customer Credentials from WSI";
                //mailMessage.Body = MicrosoftCredentialsEmailContent(item, customer);
                //mailMessage.IsBodyHtml = true;
                //SmtpClient smtp = new SmtpClient();
                //smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                //smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                //System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                //NetworkCred.UserName = email;
                //NetworkCred.Password = password;
                //smtp.UseDefaultCredentials = false;
                //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //smtp.Credentials = NetworkCred;
                //smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                //smtp.Send(mailMessage);

                //WSIExchange.WSIEmailExchange exchange = new WSIExchange.WSIEmailExchange();

                //List<string> lSentTo = new List<string>();
                //lSentTo.Add("Robert.DelaCruz@wsiphil.com.ph");

                //exchange.SendExchangeEmailNotification("Robert.DelaCruz@wsiphil.com.ph", "Test12345", MicrosoftCredentialsEmailContent(item, customer), lSentTo);


            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(c);
            }

        }
        public void SendConfirmEmail(string emails, string resellername, string generatedpassword) // for newly added reseller
        {
            try
            {
                string[] reselleremail = emails.Split(',');
                for (int i = 0; i < reselleremail.Length; i++)
                {
                    reselleremail[i] = reselleremail[i].Trim();
                    WebClient client = new WebClient();
                    NameValueCollection values = new NameValueCollection();
                    values.Add("username", email);
                    values.Add("api_key", password);
                    values.Add("from", email);
                    values.Add("from_name", sender);
                    values.Add("subject", "Temporary password for WSI MS CSP Portal");
                    values.Add("body_html", EmailLayout("Hi " + resellername, "Here is your Password", generatedpassword));
                    values.Add("to", reselleremail[i]);
                    //byte[] response = 
                    client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                    //string result = Encoding.UTF8.GetString(response);

                    //MailMessage mailMessage = new MailMessage(new MailAddress(email, sender), new MailAddress(reselleremail[i]));
                    //mailMessage.Subject = "Temporary password for WSI MS CSP Portal";
                    //mailMessage.Body = EmailLayout("Hi " + resellername, "Here is your Password", generatedpassword);
                    //mailMessage.IsBodyHtml = true;
                    //SmtpClient smtp = new SmtpClient();
                    //NetworkCredential NetworkCred = new NetworkCredential
                    //{
                    //    UserName = email,
                    //    Password = password
                    //};
                    //smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                    //smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);                   
                    //smtp.UseDefaultCredentials = false;
                    //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //smtp.Credentials = NetworkCred;
                    //smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                    //smtp.Send(mailMessage);
                }
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }

        }


        public void SendAccountForApproval(string emails, string resellername) // notify reseller that account is for approval
        {
            try
            {
                string[] reselleremail = emails.Split(',');
                for (int i = 0; i < reselleremail.Length; i++)
                {
                    reselleremail[i] = reselleremail[i].Trim();

                    WebClient client = new WebClient();
                    NameValueCollection values = new NameValueCollection();
                    values.Add("username", email);
                    values.Add("api_key", password);
                    values.Add("from", email);
                    values.Add("from_name", sender);
                    values.Add("subject", "For Approval");
                    values.Add("body_html", EmailLayout("Hi " + resellername, "Please wait for your account to be approved ", "An email with your password will be sent to you "));
                    values.Add("to", reselleremail[i]);
                    //byte[] response = 
                    client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                    //string result = Encoding.UTF8.GetString(response);

                    ////using (MailMessage mailMessage = new MailMessage())
                    ////{
                    ////    //mailMessage.From = new MailAddress(email, sender);
                    ////    //mailMessage.Subject = "For Approval";
                    ////    //mailMessage.Body = EmailLayout("Hi " + resellername, "Please wait for your account to be approved ", "An email with your password will be sent to you ");
                    ////    //mailMessage.IsBodyHtml = true;
                    ////    //mailMessage.To.Add(new MailAddress(reselleremail[i]));
                    ////    //SmtpClient smtp = new SmtpClient();
                    ////    //smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                    ////    //smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                    ////    //System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    ////    //NetworkCred.UserName = email;
                    ////    //NetworkCred.Password = password;
                    ////    //smtp.UseDefaultCredentials = false;
                    ////    //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    ////    //smtp.Credentials = NetworkCred;
                    ////    //smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                    ////    //smtp.Send(mailMessage);
                    ////}

                }
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }

        }

        public void SendForgotPassword(string _email, string resellername, string generatedpassword) // Forgot Password
        {
            try
            {
                WebClient client = new WebClient();
                NameValueCollection values = new NameValueCollection();
                values.Add("username", email);
                values.Add("api_key", password);
                values.Add("from", email);
                values.Add("from_name", sender);
                values.Add("subject", "Forgot Password");
                values.Add("body_html", EmailLayout("Hi " + resellername, "use this as your temporary password: ", generatedpassword));
                values.Add("to", _email);
                //byte[] response = 
                client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                //string result = Encoding.UTF8.GetString(response);

                //using (MailMessage mailMessage = new MailMessage())
                //{
                //    mailMessage.From = new MailAddress(email, sender);
                //    mailMessage.Subject = "Forgot Password";
                //    mailMessage.Body = EmailLayout("Hi " + resellername, "use this as your temporary password: ", generatedpassword);
                //    mailMessage.IsBodyHtml = true;
                //    mailMessage.To.Add(new MailAddress(_email));
                //    SmtpClient smtp = new SmtpClient();
                //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                //    NetworkCred.UserName = email;
                //    NetworkCred.Password = password;
                //    smtp.UseDefaultCredentials = false;
                //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //    smtp.Credentials = NetworkCred;
                //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                //    smtp.Send(mailMessage);
                //}
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }

        }

        public void SendNewPassword(string emails, string resellername, string generatedpassword) // for new password
        {
            try
            {
                string[] reselleremail = emails.Split(',');
                for (int i = 0; i < reselleremail.Length; i++)
                {
                    WebClient client = new WebClient();
                    NameValueCollection values = new NameValueCollection();
                    values.Add("username", email);
                    values.Add("api_key", password);
                    values.Add("from", email);
                    values.Add("from_name", sender);
                    values.Add("subject", "New Password");
                    values.Add("body_html", EmailLayout("Hi " + resellername, "Here is your new password", generatedpassword));
                    values.Add("to", reselleremail[i].Trim());
                    //byte[] response = 
                    client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                    //string result = Encoding.UTF8.GetString(response);
                }

                //    reselleremail[i] = reselleremail[i].Trim();

                //    using (MailMessage mailMessage = new MailMessage())
                //    {
                //        mailMessage.From = new MailAddress(email, sender);
                //        mailMessage.Subject = "New Password";
                //        mailMessage.Body = EmailLayout("Hi " + resellername, "Here is your new password", generatedpassword);
                //        mailMessage.IsBodyHtml = true;
                //        mailMessage.To.Add(new MailAddress(reselleremail[i]));
                //        SmtpClient smtp = new SmtpClient();
                //        smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                //        smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                //        System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                //        NetworkCred.UserName = email;
                //        NetworkCred.Password = password;
                //        smtp.UseDefaultCredentials = false;
                //        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //        smtp.Credentials = NetworkCred;
                //        smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                //        smtp.Send(mailMessage);
                //    }

                //}
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }

        }

        public void SendTicket(string Title, string _message, string resellername) // email in admin
        {
            try
            {
                foreach (var emails in db.Users.Where(p => p.UserTypeID == 1 && p.Email != null).Select(p => p.Email))
                {
                    WebClient client = new WebClient();
                    NameValueCollection values = new NameValueCollection();
                    values.Add("username", email);
                    values.Add("api_key", password);
                    values.Add("from", email);
                    values.Add("from_name", sender);
                    values.Add("subject", Title);
                    values.Add("body_html", EmailLayout("from " + resellername, "Ticket details:", _message));
                    values.Add("to", emails);
                    //byte[] response = 
                    client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                    //string result = Encoding.UTF8.GetString(response);

                    //using (MailMessage mailMessage = new MailMessage())
                    //{
                    //    mailMessage.From = new MailAddress(email, sender);
                    //    mailMessage.Subject = Title;
                    //    mailMessage.Body = EmailLayout("from " + resellername, "Ticket details:", _message);
                    //    mailMessage.IsBodyHtml = true;
                    //    mailMessage.To.Add(new MailAddress(emails, "Ticket"));
                    //    SmtpClient smtp = new SmtpClient();
                    //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                    //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                    //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    //    NetworkCred.UserName = email;
                    //    NetworkCred.Password = password;
                    //    smtp.UseDefaultCredentials = false;
                    //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //    smtp.Credentials = NetworkCred;
                    //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                    //    smtp.Send(mailMessage);
                    //}
                }
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }
        }

        public void SendReplyTicket(string Title, string _message, string resellername, string emails) // email reseller (ticket)
        {
            try
            {

                string[] reselleremail = emails.Split(',');
                for (int i = 0; i < reselleremail.Length; i++)
                {
                    reselleremail[i] = reselleremail[i].Trim();

                    WebClient client = new WebClient();
                    NameValueCollection values = new NameValueCollection();
                    values.Add("username", email);
                    values.Add("api_key", password);
                    values.Add("from", email);
                    values.Add("from_name", sender);
                    values.Add("subject", Title);
                    values.Add("body_html", EmailLayout("Hi " + resellername, "from WSI PORTAL:", _message));
                    values.Add("to", reselleremail[i]);
                    //byte[] response = 
                    client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                    //string result = Encoding.UTF8.GetString(response);

                    //using (MailMessage mailMessage = new MailMessage())
                    //{
                    //    mailMessage.From = new MailAddress(email, sender);
                    //    mailMessage.Subject = Title;
                    //    mailMessage.Body = EmailLayout("Hi " + resellername, "from WSI PORTAL:", _message);
                    //    mailMessage.IsBodyHtml = true;
                    //    mailMessage.To.Add(new MailAddress(reselleremail[i]));
                    //    SmtpClient smtp = new SmtpClient();
                    //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                    //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                    //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    //    NetworkCred.UserName = email;
                    //    NetworkCred.Password = password;
                    //    smtp.UseDefaultCredentials = false;
                    //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //    smtp.Credentials = NetworkCred;
                    //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                    //    smtp.Send(mailMessage);
                    //}

                }
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }
        }

        public void SendRequestToAdminToEdit_AuthorizedRep(string MPNID, string resellername, string emails) // email reseller (ticket)
        {
            try
            {
                string[] reselleremail = emails.Split(',');
                for (int i = 0; i < reselleremail.Length; i++)
                {
                    reselleremail[i] = reselleremail[i].Trim();

                    WebClient client = new WebClient();
                    NameValueCollection values = new NameValueCollection();
                    values.Add("username", email);
                    values.Add("api_key", password);
                    values.Add("from", email);
                    values.Add("from_name", sender);
                    values.Add("subject", "Request to Edit Authorized Representative");
                    values.Add("body_html", EmailLayout("Reseller, " + resellername + " is requesting to update Authorized Representative." + "<br/>"
                                                        + "Reseller MPNID: " + MPNID + " (For Admin's Reference)",
                                                        "from WSI PORTAL", ""));
                    values.Add("to", reselleremail[i]);
                    //byte[] response = 
                    client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                    //string result = Encoding.UTF8.GetString(response);

                    //using (MailMessage mailMessage = new MailMessage())
                    //{
                    //    mailMessage.From = new MailAddress(email, sender);
                    //    mailMessage.Subject = "Request to Edit Authorized Representative";
                    //    mailMessage.Body = EmailLayout("Reseller, " + resellername + " is requesting to update Authorized Representative." + "<br/>"
                    //                                    + "Reseller MPNID: " + MPNID + " (For Admin's Reference)",
                    //                                    "from WSI PORTAL", "");
                    //    mailMessage.IsBodyHtml = true;
                    //    mailMessage.To.Add(new MailAddress(reselleremail[i]));
                    //    SmtpClient smtp = new SmtpClient();
                    //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                    //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                    //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    //    NetworkCred.UserName = email;
                    //    NetworkCred.Password = password;
                    //    smtp.UseDefaultCredentials = false;
                    //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //    smtp.Credentials = NetworkCred;
                    //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                    //    smtp.Send(mailMessage);
                    //}

                }
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }
        }

        public void SendEmail_ForTicketRemarks(string TicketTitle, string TicketRemarks, string editedby, string emails) // email reseller (ticket)
        {
            try
            {
                string[] reselleremail = emails.Split(',');
                for (int i = 0; i < reselleremail.Length; i++)
                {
                    reselleremail[i] = reselleremail[i].Trim();

                    WebClient client = new WebClient();
                    NameValueCollection values = new NameValueCollection();
                    values.Add("username", email);
                    values.Add("api_key", password);
                    values.Add("from", email);
                    values.Add("from_name", sender);
                    values.Add("subject", TicketTitle);
                    values.Add("body_html", EmailLayout("There is an update in ticket " + TicketTitle + "from"
                                                        + editedby + " " + TicketRemarks,
                                                        "from WSI PORTAL", ""));
                    values.Add("to", reselleremail[i]);
                    //byte[] response = 
                    client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                    //string result = Encoding.UTF8.GetString(response);

                    //using (MailMessage mailMessage = new MailMessage())
                    //{
                    //    mailMessage.From = new MailAddress(email, sender);
                    //    mailMessage.Subject = TicketTitle;
                    //    mailMessage.Body = EmailLayout("There is an update in ticket " + TicketTitle + "from"
                    //                                    + editedby + " " + TicketRemarks,
                    //                                    "from WSI PORTAL", "");
                    //    mailMessage.IsBodyHtml = true;
                    //    mailMessage.To.Add(new MailAddress(reselleremail[i]));
                    //    SmtpClient smtp = new SmtpClient();
                    //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                    //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                    //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    //    NetworkCred.UserName = email;
                    //    NetworkCred.Password = password;
                    //    smtp.UseDefaultCredentials = false;
                    //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //    smtp.Credentials = NetworkCred;
                    //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                    //    smtp.Send(mailMessage);
                    //}

                }
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }
        }



        public void SendRequest(string subject, string emailMsg, string emailAddress) // for new password
        {
            try
            {
                WebClient client = new WebClient();
                NameValueCollection values = new NameValueCollection();
                values.Add("username", email);
                values.Add("api_key", password);
                values.Add("from", email);
                values.Add("from_name", sender);
                values.Add("subject", subject);
                values.Add("body_html", EmailLayout("Hi,", emailMsg, ""));
                values.Add("to", emailAddress);
                //byte[] response = 
                client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                //string result = Encoding.UTF8.GetString(response);

                //using (MailMessage mailMessage = new MailMessage())
                //{
                //    mailMessage.From = new MailAddress(email, sender);
                //    mailMessage.Subject = subject;
                //    mailMessage.Body = EmailLayout("Hi,", emailMsg, "");
                //    mailMessage.IsBodyHtml = true;
                //    mailMessage.To.Add(new MailAddress(emailAddress));
                //    SmtpClient smtp = new SmtpClient();
                //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                //    NetworkCred.UserName = email;
                //    NetworkCred.Password = password;
                //    smtp.UseDefaultCredentials = false;
                //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //    smtp.Credentials = NetworkCred;
                //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                //    smtp.Send(mailMessage);
                //}
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }
        }


        public void SendEmailToAllAdmin(string Module, string Action, string editedby) // email reseller (admin)
        {
            try
            {
                foreach (var emails in db.Users.Where(p => p.UserTypeID == 1 && p.Email != null).Select(p => p.Email))
                {
                    string[] reselleremail = emails.Split(',');
                    for (int i = 0; i < reselleremail.Length; i++)
                    {
                        reselleremail[i] = reselleremail[i].Trim();

                        try
                        {
                            WebClient client = new WebClient();
                            NameValueCollection values = new NameValueCollection();
                            values.Add("username", email);
                            values.Add("api_key", password);
                            values.Add("from", email);
                            values.Add("from_name", sender);
                            values.Add("subject", Module);
                            values.Add("body_html", EmailLayout(Action, " ", "Created By: " + editedby));
                            values.Add("to", reselleremail[i]);
                            //byte[] response = 
                            client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                            //string result = Encoding.UTF8.GetString(response);

                            //using (MailMessage mailMessage = new MailMessage())
                            //{
                            //    mailMessage.From = new MailAddress(email, sender);
                            //    mailMessage.Subject = Module;
                            //    mailMessage.Body = EmailLayout(Action,
                            //                                    " ", "Created By: " + editedby);
                            //    mailMessage.IsBodyHtml = true;
                            //    mailMessage.To.Add(new MailAddress(reselleremail[i]));
                            //    SmtpClient smtp = new SmtpClient();
                            //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                            //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                            //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                            //    NetworkCred.UserName = email;
                            //    NetworkCred.Password = password;
                            //    smtp.UseDefaultCredentials = false;
                            //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            //    smtp.Credentials = NetworkCred;
                            //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                            //    smtp.Send(mailMessage);
                            //}
                        }
                        catch { }

                    }
                }
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }
        }

        public void SendEmailToAuthorized(string Module, string Action, string editedby, string _email) // email reseller (authorized rep)
        {
            try
            {
                WebClient client = new WebClient();
                NameValueCollection values = new NameValueCollection();
                values.Add("username", email);
                values.Add("api_key", password);
                values.Add("from", email);
                values.Add("from_name", sender);
                values.Add("subject", Module);
                values.Add("body_html", EmailLayout(Action, " ", "Created By: " + editedby));
                values.Add("to", _email);
                //byte[] response = 
                client.UploadValues("https://api.elasticemail.com/mailer/send", values);
                //string result = Encoding.UTF8.GetString(response);

                //using (MailMessage mailMessage = new MailMessage())
                //{
                //    mailMessage.From = new MailAddress(email, sender);
                //    mailMessage.Subject = Module;
                //    mailMessage.Body = EmailLayout(Action,
                //                                    " ", "Created By:" + editedby);
                //    mailMessage.IsBodyHtml = true;
                //    mailMessage.To.Add(new MailAddress(_email));
                //    SmtpClient smtp = new SmtpClient();
                //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                //    NetworkCred.UserName = email;
                //    NetworkCred.Password = password;
                //    smtp.UseDefaultCredentials = false;
                //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //    smtp.Credentials = NetworkCred;
                //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                //    smtp.Send(mailMessage);
                //}
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }
        }

        public void SendNewOrder(string Module, string Action, string editedby, string _email)
        {

            try
            {
                WebClient client = new WebClient();
                NameValueCollection values = new NameValueCollection();
                values.Add("username", email);
                values.Add("api_key", password);
                values.Add("from", email);
                values.Add("from_name", sender);
                values.Add("subject", Module);
                values.Add("body_html", EmailLayout("Date " + DateTime.Now.ToString() + "Action:" + Action + " ", "from WSI PORTAL", "USer:" + editedby));
                values.Add("to", _email);
                //byte[] response = 
                client.UploadValues("https://api.elasticemail.com/mailer/send", values);

                //string result = Encoding.UTF8.GetString(response);
                //using (MailMessage mailMessage = new MailMessage())
                //{
                //    mailMessage.From = new MailAddress(email, sender);
                //    mailMessage.Subject = Module;
                //    mailMessage.Body = EmailLayout("Date " + DateTime.Now.ToString() + "Action:"
                //                                    + Action + " ",
                //                                    "from WSI PORTAL", "USer:" + editedby);
                //    mailMessage.IsBodyHtml = true;
                //    mailMessage.To.Add(new MailAddress(_email));
                //    SmtpClient smtp = new SmtpClient();
                //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                //    NetworkCred.UserName = email;
                //    NetworkCred.Password = password;
                //    smtp.UseDefaultCredentials = false;
                //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //    smtp.Credentials = NetworkCred;
                //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                //    smtp.Send(mailMessage);
                //}
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }


        }

        public void SendNewCustomer(string Module, string Action, string editedby, string _email)
        {

            try
            {
                WebClient client = new WebClient();
                NameValueCollection values = new NameValueCollection();
                values.Add("username", email);
                values.Add("api_key", password);
                values.Add("from", email);
                values.Add("from_name", sender);
                values.Add("subject", Module);
                values.Add("body_html", EmailLayout("Date " + DateTime.Now.ToString() + "Action:" + Action + " ", "from WSI PORTAL", "USer:" + editedby));
                values.Add("to", _email);
                //byte[] response = 
                client.UploadValues("https://api.elasticemail.com/mailer/send", values);

                //using (MailMessage mailMessage = new MailMessage())
                //{
                //    mailMessage.From = new MailAddress(email, sender);
                //    mailMessage.Subject = Module;
                //    mailMessage.Body = EmailLayout("Date " + DateTime.Now.ToString() + "Action:"
                //                                    + Action + " ",
                //                                    "from WSI PORTAL", "USer:" + editedby);
                //    mailMessage.IsBodyHtml = true;
                //    mailMessage.To.Add(new MailAddress(_email));
                //    SmtpClient smtp = new SmtpClient();
                //    smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                //    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                //    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                //    NetworkCred.UserName = email;
                //    NetworkCred.Password = password;
                //    smtp.UseDefaultCredentials = false;
                //    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //    smtp.Credentials = NetworkCred;
                //    smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                //    smtp.Send(mailMessage);
                //}

            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }

        }




        public void SendEmail(List<string> to, string subject, string message, List<string> bcc) // send email when order
        {
            try
            {
                string sto = "";
                string sbcc = "";

                foreach (string item in to)
                {
                    sto = sto + ";" + item;
                }
                foreach (string item in bcc)
                {
                    sbcc = sbcc + ";" + sbcc;
                }

                sto = sto + ";";
                sbcc = sbcc + ";";

                WebClient client = new WebClient();
                NameValueCollection values = new NameValueCollection();
                values.Add("username", email);
                values.Add("api_key", password);
                values.Add("from", email);
                values.Add("from_name", sender);
                values.Add("subject", subject);
                values.Add("body_html", message);
                values.Add("to", sto);
                values.Add("bcc", sbcc);
                //byte[] response = 
                client.UploadValues("https://api.elasticemail.com/mailer/send", values);

                //using (MailMessage mailMessage = new MailMessage())
                //{
                //MailMessage mailMessage = new MailMessage();
                //mailMessage.From = new MailAddress(email, sender);
                //mailMessage.Subject = subject;
                //mailMessage.Body = message;
                //mailMessage.IsBodyHtml = true;
                //SmtpClient smtp = new SmtpClient();
                //smtp.Host = ConfigurationManager.AppSettings["wsi_Host"];
                //smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["wsi_EnableSsl"]);
                //System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                //NetworkCred.UserName = email;
                //NetworkCred.Password = password;
                //smtp.UseDefaultCredentials = false;
                //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                //smtp.Credentials = NetworkCred;
                //smtp.Port = int.Parse(ConfigurationManager.AppSettings["wsi_Port"]);
                //smtp.Send(mailMessage);
                //}
            }
            catch (Exception m)
            {
                string c = m.Message;
                logger.log(m.Message);
            }

        }
    }
}