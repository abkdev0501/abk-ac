using Arity.Data.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arity.Infra.Interface
{
    public interface IMasterRepository
    {
        Task<List<NotificationDTO>> GetAllNotificationList(int recordFrom, int pageSize, string sortColumn, string sortOrder, Dictionary<string, object> filterParams);
        Task<List<NotificationDTO>> GetAllNotification(int userId, int userType, int type);
        Task<NotificationDTO> GetNotification(int id);
        Task<List<UsersDto>> GetClientList(int userId, int userTypeId, int recordFrom, int pageSize, string sortColumn, string sortOrder, Dictionary<string, object> filterParams);
    }
}
