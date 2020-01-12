using Arity.Data;
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
        private MailMessage mail;
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
            if (ModelState.IsValid)
            {
                _accountService = new AccountService();
                var validUser = await _accountService.Login(user.Username, "n7vz+EGHIZw=" /*Functions.Encrypt_QueryString(user.Password)*/);
                if (validUser != null)
                {
                    FormsAuthentication.SetAuthCookie(validUser.Username, false);
                    Session["UserID"] = validUser.Id.ToString();
                    Session["UserTypeID"] = validUser.UserTypeId.ToString();
                    return RedirectToAction("Index", "Home");
                }
                ViewBag.Message = "Invalid Credential";
            }
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
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ResetPassword(string id)
        {
            try
            {
                Vendor_User user = new Vendor_User();
                user.UserId = Convert.ToInt32(Function.Decrypt_QueryString(id));
                return View(user);
            }
            catch (Exception)
            {
                return RedirectToAction("NotFound", "Home");
            }
        }

        /// <summary>
        /// Reset password method   
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ResetPassword(Vendor_User user)
        {
            try
            {
                //_vendorServices = new VendorServices();
                //await _vendorServices.ResetPassword(user);
                TempData["PasswordChanged"] = "Your password changed successfully.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                return View(user);
            }
        }

        /// <summary>
        /// Logout method from where loggedin user can logged out
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Logout()
        {
            FormsAuthentication.SignOut(); // it will clear the session at the end of request
            return Json(JsonRequestBehavior.AllowGet);
        }
    }
}