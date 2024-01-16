using CoralSchedular.InvoiceServices.Data.Entities;
using CoralSchedular.InvoiceServices.Model.DTO;

namespace CoralSchedular.InvoiceServices.Data.Repositories
{
    public interface IReservationRepository
    {
        Task<IEnumerable<Reservation>> GetReservationsAsync();
        Task<bool> BulkInvoiceUpdateAsync(List<InvoicePdfModelDTO> invoicePdfModelDTOs);
    }
}
