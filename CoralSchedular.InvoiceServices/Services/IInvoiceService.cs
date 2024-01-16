using CoralSchedular.InvoiceServices.Model.DTO;

namespace CoralSchedular.InvoiceServices.Services
{
    public interface IInvoiceService
    {
        public Dictionary<string, List<InvoicePdfModelDTO>> ParseFlightInvoice();
        public Task<List<ReservationDTO>> GetReservationsAsync();
        public Task<bool> BulkInvoiceUpdateAsync(List<InvoicePdfModelDTO> invoicePdfModelDTOs);
    }
}
