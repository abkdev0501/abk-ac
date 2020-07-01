using System.Threading.Tasks;

namespace Arity.Service.Contract
{
    public interface ILoggerService
    {
        Task LogAsync(string user, string message);
    }
}
