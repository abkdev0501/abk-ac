using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Service.Helpers
{
    public interface IEmailSender
    {
        void SendEmail(string subject, string messageBody, string path);
    }
}
