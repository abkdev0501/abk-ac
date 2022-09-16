using Arity.Data.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arity.Infra.Interface
{
    public interface IMasterRepository
    {
        Task<List<NotificationDTO>> GetAllNotificationList();
        Task<List<NotificationDTO>> GetAllNotification(int userId, int userType, int type);
        Task<NotificationDTO> GetNotification(int id);
    }
}
