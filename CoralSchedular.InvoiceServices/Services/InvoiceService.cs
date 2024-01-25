using CoralSchedular.InvoiceServices.Model.DTO;
using CoralSchedular.InvoiceServices.Utils;

namespace CoralSchedular.InvoiceServices.Services
{
    public class InvoiceService : IInvoiceService
    {

        private readonly IConfiguration _configuration;
        private readonly IReservationService _reservationService;

        public InvoiceService(IConfiguration configuration, IReservationService reservationService)
        {
            _configuration = configuration;
            _reservationService = reservationService;
        }

        public Dictionary<string, List<InvoicePdfModelDTO>> ParseFlightInvoice()
        {
            var invoicePdfFolder = _configuration["FlightInvoicePdfFolder"];
            var invoiceFileType = _configuration["FlightInvoiceFileType"];

            if (invoicePdfFolder == null || invoiceFileType == null)
            {
                //ToDO Log
                throw new Exception("Invoice File Configuration not Exists");
            }

            //Get full file name list from directory
            var invoiceList = FileHelper.GetFileList(invoicePdfFolder, invoiceFileType, SearchOption.TopDirectoryOnly);

            if (invoiceList == null || !invoiceList.Any())
            {
                //ToDO Log
                throw new Exception("No Data Found in Invoice Directory");
            }

            var flightInvoiceParser = new FlightInvoiceParser();

            //This Dictionary Collection provides a list of records for each invoice separately
            var invoiceDictionary = new Dictionary<string, List<InvoicePdfModelDTO>>();

            //iterate for each file, read each invoice pdf file one by one
            //Add invoice record lists to Dictionary using each file's name as KEY
            foreach (var file in invoiceList)
            {
                string fileName = Path.GetFileName(file);

                string pdfInString = PdfHelper.GetPdfInString(file);                    //Read text from PDF file
                var invoicePdfModelDTOs = flightInvoiceParser.Parse(pdfInString);       //Get reservation records from string

                invoiceDictionary.Add(fileName, invoicePdfModelDTOs);
            }

            return invoiceDictionary;
        }

        
        public async Task<List<ReservationDTO>> GetReservationsAsync()
        {
            var reservationDTOs = await _reservationService.GetReservationsAsync();

            if (reservationDTOs == null || !reservationDTOs.Any())
            {
                //ToDO Log
                throw new Exception("No Data Found on Reservation Table");
            }

            return reservationDTOs.ToList();
        }

        public async Task<bool> BulkInvoiceUpdateAsync(List<InvoicePdfModelDTO> invoicePdfModelDTOs)
        {
            return await _reservationService.BulkInvoiceUpdateAsync(invoicePdfModelDTOs);
        }
    }
}
