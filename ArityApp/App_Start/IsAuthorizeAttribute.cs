using Arity.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ArityApp.App_Start
{
    public class IsAuthorizeAttribute : AuthorizeAttribute
    {
        public IsAuthorizeAttribute()
        {
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (SessionHelper.UserId <= 0)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            base.OnAuthorization(filterContext);
            var user = filterContext.HttpContext.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
                    CheckForAjaxRequest(filterContext);
                else
                    filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else
            {
                var authCookie = filterContext.RequestContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                var userData = ticket.UserData;
            }
        }

        private void CheckForAjaxRequest(AuthorizationContext filterContext)
        {

            filterContext.Result = new JsonResult
            {
                Data = new { Message = "Your session has died a terrible and gruesome death" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            filterContext.HttpContext.Response.StatusDescription = "Humans and robots must authenticate";
            filterContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
            filterContext.HttpContext.Response.End();
        }
    }
}