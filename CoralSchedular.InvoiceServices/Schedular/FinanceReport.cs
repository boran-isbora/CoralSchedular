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
                    .GroupBy(x => new { x.BookingID, x.FlightDate, x.CarrierCode, x.FlightNo, x.InvoiceNumber })
                    .Select(x =>
                        new GroupReservationDTO
                        {
                            BookingID = x.Key.BookingID,
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

                //This method groups the reservation list by BookingID, FlightDate, CarrierCode, FlightNo and InvoiceNumber. Sum Price value for similar records.
                var groupReservationDTOs = GetGroupOfReservations(reservations);

                //read data from pdf file
                var parsedRecordFromPDFDictionary = _invoiceService.ParseFlightInvoice();


                foreach (var item in parsedRecordFromPDFDictionary)     //Loop for each Incoice PDF file
                {
                    var successfulRecords = new List<InvoicePdfModelDTO>();
                    var unmatchedRecords = new List<InvoicePdfModelDTO>();
                    var duplicateInvoice = new List<InvoicePdfModelDTO>();
                    var differentPrice = new List<InvoicePdfModelDTO>();

                    foreach (var pdfRecord in item.Value)               //Loop through each invoice record in an Invoice PDF file
                    {
                        var foundedReservation = groupReservationDTOs
                            .Where(x => x.FlightDate == pdfRecord.FlightDate && x.CarrierCode == pdfRecord.CarrierCode && x.FlightNo == pdfRecord.FlightNo)
                            .ToList();

                        var pdfRecordFound = false;


                        //Don't change the order of foreach loops!!!
                        //Since TotalPrice and InvoiceNumber should not be included in the above grouping.
                        //So more than one record may be returned.
                        //In cases where more than one record is returned, whether any of them match should be checked in the following order.


                        foreach (var foundedItem in foundedReservation)  //Loop for grouped list of records found in database
                        {
                            //Check Successful Records
                            if (pdfRecord.FlightDate == foundedItem.FlightDate &&
                                pdfRecord.CarrierCode == foundedItem.CarrierCode &&
                                pdfRecord.FlightNo == foundedItem.FlightNo &&
                                Decimal.Compare(pdfRecord.TotalPrice, foundedItem.TotalPrice) == 0 &&
                                (foundedItem.InvoiceNumber == null || pdfRecord.InvoiceNumber == foundedItem.InvoiceNumber))
                            {
                                successfulRecords.Add(pdfRecord);
                                pdfRecordFound = true;
                                break;
                            }
                        }

                        if (pdfRecordFound == true)
                            continue;


                        foreach (var foundedItem in foundedReservation)  //Loop for grouped list of records found in database
                        {
                            //Check Duplicate Invoice
                            if (pdfRecord.FlightDate == foundedItem.FlightDate &&
                                pdfRecord.CarrierCode == foundedItem.CarrierCode &&
                                pdfRecord.FlightNo == foundedItem.FlightNo &&
                                Decimal.Compare(pdfRecord.TotalPrice, foundedItem.TotalPrice) == 0 &&
                                (foundedItem.InvoiceNumber != null && pdfRecord.InvoiceNumber != foundedItem.InvoiceNumber))
                            {
                                duplicateInvoice.Add(pdfRecord);
                                pdfRecordFound = true;
                                break;
                            }
                        }

                        if (pdfRecordFound == true)
                            continue;

                        foreach (var foundedItem in foundedReservation)  //Loop for grouped list of records found in database
                        {
                            //Check Different Price
                            if (pdfRecord.FlightDate == foundedItem.FlightDate &&
                                pdfRecord.CarrierCode == foundedItem.CarrierCode &&
                                pdfRecord.FlightNo == foundedItem.FlightNo &&
                                Decimal.Compare(pdfRecord.TotalPrice, foundedItem.TotalPrice) != 0 &&
                                (foundedItem.InvoiceNumber == null || pdfRecord.InvoiceNumber == foundedItem.InvoiceNumber))
                            {
                                differentPrice.Add(pdfRecord);
                                pdfRecordFound = true;
                                break;
                            }
                        }

                        if (pdfRecordFound == true)
                            continue;

                        //else case
                        if (pdfRecordFound == false)
                            unmatchedRecords.Add(pdfRecord);
                    }
                    

                    //Update Invoice Number
                    if (successfulRecords.Count > 0)
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
