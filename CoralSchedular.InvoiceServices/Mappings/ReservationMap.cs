using CoralSchedular.InvoiceServices.Data.Entities;
using CoralSchedular.InvoiceServices.Model.DTO;

namespace CoralSchedular.InvoiceServices.Mappings
{
    public class ReservationMap
    {
        public ReservationDTO ReservationEntityToDTO(Reservation item) =>
            new ReservationDTO
            {
                BookingID = item.BookingID,
                Customer = item.Customer,
                CarrierCode = item.CarrierCode,
                FlightNo = item.FlightNo,
                FlightDate = item.FlightDate,
                Origin = item.Origin,
                Destination = item.Destination,
                Price = item.Price,
                InvoiceNumber = item.InvoiceNumber
            };


        public Reservation ReservationDTOToEntity(ReservationDTO item) =>
            new Reservation
            {
                BookingID = item.BookingID,
                Customer = item.Customer,
                CarrierCode = item.CarrierCode,
                FlightNo = item.FlightNo,
                FlightDate = item.FlightDate,
                Origin = item.Origin,
                Destination = item.Destination,
                Price = item.Price,
                InvoiceNumber = item.InvoiceNumber
            };
    }
}
