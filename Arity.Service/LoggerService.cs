using Arity.Service.Contract;
using System.IO;
using System.Threading.Tasks;

namespace Arity.Service
{
    public class LoggerService : ILoggerService
    {
        public async Task LogAsync(string user, string path)
        {
            try
            {
                if (!Directory.Exists(path.Replace("\\UserLog.txt", "")))
                    Directory.CreateDirectory(path.Replace("\\UserLog.txt", ""));

                if (!File.Exists(path))
                    File.WriteAllText(path, "");

                File.AppendAllText(path, user + ",");
            }
            catch
            {
            }
        }
    }
}
