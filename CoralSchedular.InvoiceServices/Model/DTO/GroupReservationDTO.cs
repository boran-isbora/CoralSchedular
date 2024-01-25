namespace CoralSchedular.InvoiceServices.Model.DTO
{
    public class GroupReservationDTO
    {
        public int BookingID { get; set; }
        public DateTime FlightDate { get; set; }
        public string CarrierCode { get; set; }
        public string FlightNo { get; set; }
        public decimal TotalPrice { get; set; }
        public int? InvoiceNumber { get; set; }
    }
}
