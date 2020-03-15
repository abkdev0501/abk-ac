namespace Arity.Data.Dto
{
    public class InvoiceParticularDto
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public long ParticularId { get; set; }
        public decimal Amount { get; set; }
        public string year { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
    }
}
