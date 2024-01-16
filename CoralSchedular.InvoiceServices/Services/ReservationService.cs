using CoralSchedular.InvoiceServices.Data.Repositories;
using CoralSchedular.InvoiceServices.Mappings;
using CoralSchedular.InvoiceServices.Model.DTO;

namespace CoralSchedular.InvoiceServices.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ReservationMap _reservationMap;

        public ReservationService(IReservationRepository reservationRepository, ReservationMap reservationMap)
        {
            _reservationRepository = reservationRepository;
            _reservationMap = reservationMap;
        }

        public async Task<IEnumerable<ReservationDTO>?> GetReservationsAsync()
        {
            var reservations = await _reservationRepository.GetReservationsAsync();

            if (reservations == null)
                return null;

            var reservationDTOs = reservations.Select(x => _reservationMap.ReservationEntityToDTO(x));

            return reservationDTOs;
        }

        public Task<bool> BulkInvoiceUpdateAsync(List<InvoicePdfModelDTO> invoicePdfModelDTOs)
        {
            return _reservationRepository.BulkInvoiceUpdateAsync(invoicePdfModelDTOs);
        }
    }
}
