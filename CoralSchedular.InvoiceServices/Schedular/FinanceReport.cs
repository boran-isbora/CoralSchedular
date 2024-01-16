using CoralSchedular.InvoiceServices.Model.DTO;
using CoralSchedular.InvoiceServices.Services;
using CoralSchedular.InvoiceServices.Utils;
using System.Text;

namespace CoralSchedular.InvoiceServices.Schedular
{
    public class FinanceReport : IFinanceReport
    {
        private readonly IConfiguration _configuration;
        private readonly IInvoiceService _invoiceService;

        public FinanceReport(IConfiguration configuration, IInvoiceService invoiceService)
        {
            _configuration = configuration;
            _invoiceService = invoiceService;
        }

        private List<GroupReservationDTO>? GetGroupOfReservations(List<ReservationDTO> reservations)
        {
            if(reservations == null || reservations.Count == 0)
                throw new Exception("No Data Found on Reservation Table");

            return reservations
                    .GroupBy(x => new { x.FlightDate, x.CarrierCode, x.FlightNo, x.InvoiceNumber })
                    .Select(x =>
                        new GroupReservationDTO
                        {
                            FlightDate = x.Key.FlightDate,
                            CarrierCode = x.Key.CarrierCode,
                            FlightNo = x.Key.FlightNo,
                            InvoiceNumber = x.Key.InvoiceNumber,
                            TotalPrice = x.Sum(x => x.Price)
                        })
                    .ToList();
        }


        private string GetInvoicePdfModelInString(List<InvoicePdfModelDTO> records)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in records)
            {
                sb.AppendLine(item.InvoiceNumber + ", " + item.FlightDate.ToString("dd.MM.yyyy") + ", " + item.CarrierCode + ", " + item.FlightNo + ", " + item.TotalPrice.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        private string GetMailAttachment(List<InvoicePdfModelDTO> unmatchedRecords, 
                                            List<InvoicePdfModelDTO> duplicateInvoice, 
                                            List<InvoicePdfModelDTO> differentPrice)
        {
            StringBuilder mailAttachment = new StringBuilder();
            mailAttachment.AppendLine("Unmatched Records");
            mailAttachment.AppendLine("InvoiceNumber,FlightDate,CarrierCode,FlightNo,TotalPrice");
            mailAttachment.AppendLine(GetInvoicePdfModelInString(unmatchedRecords));

            mailAttachment.AppendLine("");
            mailAttachment.AppendLine("Duplicate Invoice");
            mailAttachment.AppendLine("InvoiceNumber,FlightDate,CarrierCode,FlightNo,TotalPrice");
            mailAttachment.AppendLine(GetInvoicePdfModelInString(duplicateInvoice));

            mailAttachment.AppendLine("");
            mailAttachment.AppendLine("Different Price");
            mailAttachment.AppendLine("InvoiceNumber,FlightDate,CarrierCode,FlightNo,TotalPrice");
            mailAttachment.AppendLine(GetInvoicePdfModelInString(differentPrice));

            return mailAttachment.ToString();
        }

        private string GetMailBody(int totalProcessedRecords, int invalidRecords)
        {
            StringBuilder mailBody = new StringBuilder();
            mailBody.AppendFormat("<br />");
            mailBody.AppendFormat("<h2>Coral Flight Invoice Check Report</h2>");
            mailBody.AppendFormat("<p>Total processed records: {0}</p>", totalProcessedRecords);
            mailBody.AppendFormat("<p>Successful records: {0}</p>", totalProcessedRecords - invalidRecords);
            mailBody.AppendFormat("<p>Invalid records: {0}</p>", invalidRecords);

            return mailBody.ToString();
        }

        private void SendFlightInvoiceMail(string invoiceName, 
            int totalProcessedRecords, 
            List<InvoicePdfModelDTO> unmatchedRecords, 
            List<InvoicePdfModelDTO> duplicateInvoice, 
            List<InvoicePdfModelDTO> differentPrice)
        {

            var smtpClient = _configuration["EmailSettings:SmtpClient"];
            var SmtpClientPort = _configuration["EmailSettings:SmtpClientPort"];
            var fromAddress = _configuration["EmailSettings:From"];
            var password = _configuration["EmailSettings:Password"];
            var toAddress = _configuration["EmailSettings:To"];

            if (smtpClient == null || SmtpClientPort == null || fromAddress == null || password == null || toAddress == null)
            {
                //ToDO Log
                throw new Exception("Email Settings Configuration not Exists");
            }

            var subject = "Coral Flight Invoice Check Report [" + invoiceName + "] [" + DateTime.Now + "]";

            var invalidRecords = unmatchedRecords.Count + duplicateInvoice.Count + differentPrice.Count;
            var mailBody = GetMailBody(totalProcessedRecords, invalidRecords);

            var mailAttachment = GetMailAttachment(unmatchedRecords, duplicateInvoice, differentPrice);

            EmailHelper.Send(smtpClient, int.Parse(SmtpClientPort), fromAddress, password, toAddress, subject, mailBody, mailAttachment);
        }

        public async Task CheckFlightInvoice()
        {
            try
            {
                //read data from database
                var reservations = await _invoiceService.GetReservationsAsync();

                //This method groups the reservation list by FlightDate, CarrierCode, FlightNo and InvoiceNumber. Sum Price value for similar records.
                var groupReservationDTOs = GetGroupOfReservations(reservations);

                //read data from pdf file
                var parsedRecordFromPDFDictionary = _invoiceService.ParseFlightInvoice();

                foreach (var item in parsedRecordFromPDFDictionary )
                {
                    var successfulRecords = new List<InvoicePdfModelDTO>();
                    var unmatchedRecords = new List<InvoicePdfModelDTO>();
                    var duplicateInvoice = new List<InvoicePdfModelDTO>();
                    var differentPrice = new List<InvoicePdfModelDTO>();
                    
                    foreach (var record in item.Value)
                    {
                        var foundedReservation = groupReservationDTOs
                            .Where(x => x.FlightDate == record.FlightDate && x.CarrierCode == record.CarrierCode && x.FlightNo == record.FlightNo)
                            .ToList();

                        if (foundedReservation.Count == 0)
                        {
                            unmatchedRecords.Add(record);
                            continue;
                        }

                        //If there is more than one record, its means this flight has been invoiced more than once or a part of the flight reservation has been invoiced.
                        if (foundedReservation.Count > 1)
                        {
                            duplicateInvoice.Add(record);
                            continue;
                        }

                        //Count == 0 and Count > 1 already checked so list size is 1 for the following cases.

                        //If it is registered with a different invoice number
                        if (foundedReservation[0].InvoiceNumber != null && foundedReservation[0].InvoiceNumber != record.InvoiceNumber)
                        {
                            duplicateInvoice.Add(record);
                            continue;
                        }

                        if (foundedReservation[0].TotalPrice != record.TotalPrice)
                        {
                            differentPrice.Add(record);
                            continue;
                        }

                        successfulRecords.Add(record);
                    }

                    //Update Invoice Number
                    if(successfulRecords.Count > 0)
                        await _invoiceService.BulkInvoiceUpdateAsync(successfulRecords);

                    var invoiceName = item.Key;
                    var totalProcessedRecords = item.Value.Count;

                    SendFlightInvoiceMail(invoiceName, totalProcessedRecords, unmatchedRecords, duplicateInvoice, differentPrice);
                }

            }
            catch (Exception e)
            {
                //ToDO Log
                throw new Exception("CheckFlightInvoice Operation Exception! " + e.Message);
            }
        }
    }
}
