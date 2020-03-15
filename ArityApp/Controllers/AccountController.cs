using Arity.Data;
using Arity.Data.Entity;
using Arity.Data.Helpers;
using Arity.Service;
using Arity.Service.Contract;
using Arity.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ArityApp.Controllers
{
    public class AccountController : Controller
    {
        IAccountService _accountService;
        public AccountController()
        {
            _accountService = new AccountService();
        }

        // GET: Account
        public ActionResult Login()
        {
            if (TempData["PasswordChanged"] != null)
            {
                ViewBag.PasswordChangedMessage = TempData["PasswordChanged"];
            }
            return View();
        }

        /// <summary>
        /// User Login
        /// </summary>
        /// <param name="vendorUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Login(User user)
        {
            if (!string.IsNullOrEmpty(user.Password) && !string.IsNullOrEmpty(user.Username))
            {
                _accountService = new AccountService();
                var validUser = await _accountService.Login(user.Username, Functions.Encrypt(user.Password));
                if (validUser != null)
                {
                    FormsAuthentication.SetAuthCookie(validUser.Username, false);
                    SessionHelper.UserId = validUser.Id;
                    SessionHelper.UserTypeId = validUser.UserTypeId;
                    return RedirectToAction("Dashboard", "Home");
                }
                ViewBag.Message = "Invalid Credential";
            }
            ViewBag.Message = "Please enter username & password";
            return View(user);
        }

        /// <summary>
        /// Reset password method from where we send email to reset password with link
        /// </summary>
        /// <param name="EmailID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string EmailID)
        {

            try
            {
                //string emailBody = string.Empty;
                //string ResetLink = string.Empty;
                //_vendorServices = new VendorServices();
                //var forgotPasswordData = await _vendorServices.ForgotPassword(EmailID);
                //if (forgotPasswordData != null)
                //{
                //    ResetLink = ConfigurationManager.AppSettings["URL"] + "Account/Resetpassword/" + Functions.Encrypt_QueryString(forgotPasswordData.UserId.ToString());

                //    emailBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/Content/Template/ResetPasswordTemplate.html"));
                //    emailBody = emailBody.Replace("###ResetLink###", ResetLink).Replace("###Name###", forgotPasswordData.FirstName + "  " + forgotPasswordData.LastName);

                //    await _vendorServices.SendEmail(EmailID, ConfigurationManager.AppSettings["ResetPassword"], emailBody);
                //    return Json(new { Status = true }, JsonRequestBehavior.AllowGet);
                //}
                return Json(new { Status = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        /// <summary>
        /// Logout method from where loggedin user can logged out
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Logout()
        {
            FormsAuthentication.SignOut(); // it will clear the session at the end of request
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}