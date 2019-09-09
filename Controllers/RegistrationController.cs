using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using TrainingAssessmentProject.Models;

namespace TrainingAssessmentProject.Controllers
{
    public class RegistrationController : Controller
    {
        // GET: Registration
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "VerifyEmail,Activation")] tblEmployee user)
{
        bool Status = false;
        string message = "";
    //
    // Model Validation 
        if (ModelState.IsValid)
         {

                #region //Email is already Exist 
                var isExist = IsEmailExist((user.EmpEmailId));
          if (isExist)
            {
                ModelState.AddModelError("EmailExist", "Email already exist");
                return View(user);
            }
        #endregion
 
            #region Generate Activation Code 
             user.Activation = Guid.NewGuid();
            #endregion
 
            #region  Password Hashing 
             user.Password = Crypto.Hash(user.Password);
             user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); //
             #endregion
             user.VerifyEmail= false;
 
        #region Save to Database
             using (TrainingAssessmentEntities dc = new TrainingAssessmentEntities())
             {
                 dc.tblEmployees.Add(user);
                 dc.SaveChanges();
 
            //Send Email to User
                 SenderVerificationLinkEmail(user.EmpEmailId, user.Activation.ToString());
                 message = "Registration successfully done. Account activation link " + 
                " has been sent to your email id:" + user.EmpEmailId;
                 Status = true;
        }
        #endregion
     }
         else
             {
                 message = "Invalid Request";
             }
 
                  ViewBag.Message = message;
                  ViewBag.Status = Status;
                  return View(user);
}
        //Verify Account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (TrainingAssessmentEntities dc = new TrainingAssessmentEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; // This line I have added here to avoid 
                                                                // Confirm password does not match issue on save changes
                var v = dc.tblEmployees.Where(a => a.Activation == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.VerifyEmail = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }
        //Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(EmpLogin login, string ReturnUrl = "")
        {
            string message = "";
             using (TrainingAssessmentEntities dc = new TrainingAssessmentEntities())
            {
                var v = dc.tblEmployees.Where(a => a.EmpEmailId == login.EmpEmailId).FirstOrDefault();
                if (v != null)
                {
                    //if (!v.VerifyEmail)
                    //{
                    //    ViewBag.Message = "Please verify your email first";
                    //    return View();
                    //}
                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; // 525600 min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.EmpEmailId, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid credential provided";
                    }
                }
                else
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Registration");
        }
        //Forget Password
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string EmailID)
{
    //Verify Email ID
    //Generate Reset password link 
    //Send Email 
    string message = "";
    bool status = false;

            using (TrainingAssessmentEntities dc = new TrainingAssessmentEntities())
            {
        var account = dc.tblEmployees.Where(a => a.EmpEmailId == EmailID).FirstOrDefault();
        if (account != null)
        {
            //Send email for reset password
            string resetCode = Guid.NewGuid().ToString();
            SenderVerificationLinkEmail(account.EmpEmailId, resetCode, "ResetPassword");
            account.ResetPasswordCode = resetCode;
            //This line I have added here to avoid confirm password not match issue , as we had added a confirm password property 
            //in our model class in part 1
            dc.Configuration.ValidateOnSaveEnabled = false;
            dc.SaveChanges();
            message = "Reset password link has been sent to your email id.";
        }
        else
        {
            message = "Account not found";
        }
    }
    ViewBag.Message = message;
    return View();
}

        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            using (TrainingAssessmentEntities dc = new TrainingAssessmentEntities())
            {
                var user = dc.tblEmployees.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
                if (user != null)
                {
                    ChangePassword model = new ChangePassword();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ChangePassword model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                 using (TrainingAssessmentEntities dc = new TrainingAssessmentEntities())
                {
                    var user = dc.tblEmployees.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = Crypto.Hash(model.NewPassword);
                        user.ResetPasswordCode = "";
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        dc.SaveChanges();
                        message = "New password updated successfully";
                    }
                }
            }
            else
            {
                message = "Something invalid";
            }
            ViewBag.Message = message;
            return View(model);
        }


        [NonAction]
        public void SenderVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/Registration/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("soumyasiddhartha18@gmail.com", "Siddhartha");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "***********"; // Replace with actual password

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Your account is successfully created!";
                body = "<br/><br/>We are excited to tell you that your Training Assessment account is" +
                    " successfully created. Please click on the below link to verify your account" +
                    " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi,<br/>br/>We got request for reset your account password. Please click on the below link to reset your password" +
                    "<br/><br/><a href=" + link + ">Reset Password link</a>";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (TrainingAssessmentEntities dc = new TrainingAssessmentEntities())
            {
                var v = dc.tblEmployees.Where(a => a.EmpEmailId == emailID).FirstOrDefault();
                return v != null;
            }
        }
    }
}
