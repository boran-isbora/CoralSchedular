namespace CoralSchedular.InvoiceServices.Model.DTO
{
    public class InvoicePdfModelDTO
    {
        public int InvoiceNumber { get; set; }
        public DateTime FlightDate { get; set; }
        public string CarrierCode { get; set; }
        public string FlightNo { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
