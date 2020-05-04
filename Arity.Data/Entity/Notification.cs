
namespace Arity.Data.Entity
{
    using System.ComponentModel.DataAnnotations;

    public partial class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        public int? ClientId { get; set; }
        public int Type { get; set; }
        public string Message { get; set; }
        public System.DateTime OnBroadcastDateTime { get; set; }
        public System.DateTime OffBroadcastDateTime { get; set; }
        public bool? IsComplete { get; set; }
        public int CreatedBy { get; set; }

    }
}
