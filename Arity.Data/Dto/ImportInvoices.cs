namespace Arity.Data.Dto
{
    public class ImportInvoices
    {
        public long ParticularId { get; set; }
        public decimal Amount { get; set; }
        public string Year { get; set; }
        public string ParticularName { get; set; }
        public int Row { get; set; }
    }
}
