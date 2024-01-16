﻿namespace CoralSchedular.InvoiceServices.Data.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
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
