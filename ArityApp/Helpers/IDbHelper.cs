using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArityApp.Helpers
{
    public interface IDbHelper
    {
        void DoBackup(string path);
    }
}