using CoralSchedular.InvoiceServices.Model.DTO;

namespace CoralSchedular.InvoiceServices.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationDTO>?> GetReservationsAsync();

        Task<bool> BulkInvoiceUpdateAsync(List<InvoicePdfModelDTO> invoicePdfModelDTOs);
    }
}
