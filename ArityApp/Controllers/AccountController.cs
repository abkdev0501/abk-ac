using Arity.Data.Helpers;
using Arity.Service;
using Arity.Service.Contract;
using ArityApp.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ArityApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILoggerService _loggerService;


        public AccountController(IAccountService accountService, ILoggerService loggerService)
        {
            _accountService = accountService;
            _loggerService = loggerService;
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
        public async Task<ActionResult> Login(LoginModel user)
        {
            if (ModelState.IsValid)
            {
                string path = Server.MapPath("~/Content/Logs/UserLog.txt");
                if (!string.IsNullOrEmpty(user.Password) && !string.IsNullOrEmpty(user.Username))
                {
                    var validUser = await _accountService.Login(user.Username, Functions.Encrypt(user.Password));
                    if (validUser != null)
                    {
                        FormsAuthentication.SetAuthCookie(validUser.Username, user.KeepSignedIn);
                        SessionHelper.UserId = validUser.Id;
                        SessionHelper.UserTypeId = validUser.UserTypeId;
                        SessionHelper.UserName = validUser.Username;
                        SessionHelper.IsKeepMeSignedIn = user.KeepSignedIn;
                   
                        // Logging user details
                        Task.Run(() =>
                        {
                            _loggerService.LogAsync(JsonConvert.SerializeObject(new UserDTO
                            {
                                UserId = Convert.ToInt32(validUser.Id),
                                FullName = validUser.FullName,
                                UserType = Enum.GetName(typeof(EnumHelper.UserType), Convert.ToInt32(validUser.UserTypeId)),
                                LoginAt = DateTime.Now,
                                IpAddress = Request.UserHostAddress
                            }), path);
                        });

                        return RedirectToAction("Dashboard", "Home");
                    }
                    ViewBag.Message = "Invalid Credential";
                }
            }
            return View(user);
        }

        /// <summary>
        /// Logout method from where loggedin user can logged out
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut(); // it will clear the session at the end of request
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get user details
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> GetUserProfiles()
        {
            var details = await _accountService.GetUser(Convert.ToInt32(SessionHelper.UserId));
            if (details != null)
                details.Password = string.Empty;

            return PartialView("_UserProfile", details);
        }

        /// <summary>
        /// Add ping route to refresh session
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public void Ping()
        {
        }
    }
}