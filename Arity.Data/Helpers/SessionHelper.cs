using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Arity.Data.Helpers
{
    public static class SessionHelper
    {
        public static long UserId
        {
            get
            {
                return SessionVariables.Get<long>("UserID");
            }
            set
            {
                SessionVariables.Set("UserID", value);
            }
        }

        public static long UserTypeId
        {
            get
            {
                return SessionVariables.Get<long>("UserTypeId");
            }
            set
            {
                SessionVariables.Set("UserTypeId", value);
            }
        }
        
        public static string UserName
        {
            get
            {
                return SessionVariables.Get<string>("UserName");
            }
            set
            {
                SessionVariables.Set("UserName", value);
            }
        }

        public static bool IsKeepMeSignedIn
        {
            get
            {
                return SessionVariables.Get<bool>("RememberMe");
            }
            set
            {
                SessionVariables.Set("RememberMe", value);
            }
        }

        private static class SessionVariables
        {
            public static T Get<T>(string sessionName)
            {
                if (HttpContext.Current.Session != null && HttpContext.Current.Session[sessionName] != null)
                    return (T)HttpContext.Current.Session[sessionName];
                return default(T);
            }

            public static void Set(string sessionName, object value)
            {
                HttpContext.Current.Session[sessionName] = value;
            }
        }
    }
}
