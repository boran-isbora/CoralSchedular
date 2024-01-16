using CoralSchedular.InvoiceServices.Data.Entities;
using CoralSchedular.InvoiceServices.Model.DTO;
using CoralSchedular.InvoiceServices.Utils;
using Microsoft.EntityFrameworkCore;

namespace CoralSchedular.InvoiceServices.Data.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly CoralDbContext _dbContext;

        public ReservationRepository(CoralDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Reservation>> GetReservationsAsync()
        {
            var reservations = await _dbContext.Reservations.AsNoTracking().ToListAsync();

            return reservations;
        }

        public async Task<bool> BulkInvoiceUpdateAsync(List<InvoicePdfModelDTO> invoicePdfModelDTOs)
        {   
            var predicate = PredicateBuilder.False<Reservation>();
            foreach (var item in invoicePdfModelDTOs)
                predicate = predicate.Or(x => x.FlightDate == item.FlightDate && x.FlightNo == item.FlightNo && x.CarrierCode == item.CarrierCode);

            var reservations = _dbContext.Reservations
                .Where(predicate)
                .ToList();

            var invoiceNumber = invoicePdfModelDTOs.First().InvoiceNumber;

            reservations.ForEach(x => x.InvoiceNumber = invoiceNumber);

            await _dbContext.BulkUpdateAsync(reservations);

            return true;
        }
    }
}
