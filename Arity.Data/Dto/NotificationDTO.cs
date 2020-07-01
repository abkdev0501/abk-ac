using System.ComponentModel.DataAnnotations;

namespace Arity.Data.Dto
{
    public partial class NotificationDTO
    {
        public int NotificationId { get; set; }
        public int? ClientId { get; set; }
        public string ClientName{ get; set; }

        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
        public System.DateTime OnBroadcastDateTime { get; set; }
        public System.DateTime OffBroadcastDateTime { get; set; }
        public bool IsComplete { get; set; }
        public int CreatedBy { get; set; }
        public int Type { get; set; }
        public string CreatedByName { get; set; }
        public string OffBroadcastDateTimeString { get; set; }
        public string OnBroadcastDateTimeString { get; set; }
        public string TypeString { get; set; }
        public string CreatedOn { get; set; }
        public string CompletedOn { get; set; }
    }
}
