namespace Arity.Data.Dto
{
    public class TrackingInformation
    {
        public int TrackingId { get; set; }
        public string Comment { get; set; }
        public string CreatedAt { get; set; }
        public int InvoiceId { get; set; }
        public int CreatedBy { get; set; }
    }
}
