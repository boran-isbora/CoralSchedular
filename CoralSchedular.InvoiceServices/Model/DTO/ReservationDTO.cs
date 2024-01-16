namespace CoralSchedular.InvoiceServices.Model.DTO
{
    public class ReservationDTO
    {
        public int BookingID { get; set; }
        public string Customer { get; set; }
        public string CarrierCode { get; set; }
        public string FlightNo { get; set; }
        public DateTime FlightDate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public decimal Price { get; set; }
        public int? InvoiceNumber { get; set; }
    }
}
