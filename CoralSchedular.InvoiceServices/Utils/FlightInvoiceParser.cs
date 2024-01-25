using CoralSchedular.InvoiceServices.Model.DTO;
using Microsoft.IdentityModel.Tokens;

namespace CoralSchedular.InvoiceServices.Utils
{
    public class FlightInvoiceParser
    {
        private InvoicePdfModelDTO? InvoiceRecordFromString(string text, string invoiceNumber)
        {
            //SAMPLE DATA
            //Season    VT  Flugdatum   Flug Nr.    Routing     Anzahl  Einzelpreis     Betrag in EUR   Summen in EUR
            //2324      2   09.01.2024  XQ 110      AYT BSL     2-      156,00          312,00-         312,00-

            //2324 2 09.01.2024 XQ 110 AYT BSL 2- 156,00 312,00-         312,00-

            InvoicePdfModelDTO invoicePdfModel = new InvoicePdfModelDTO();

            try
            {
                invoicePdfModel.InvoiceNumber = int.Parse(invoiceNumber);

                string[] splitRecord = text.Split(new string[] { " " }, StringSplitOptions.None);

                //Business Rule: Anzahl: Number of sold seats in this plane (if there is a minus after the number, ignore this record)
                if (splitRecord[7].Contains('-')) return null;

                DateTime flightDate;
                string[] format = new string[] { "dd.MM.yyyy" };
                DateTime.TryParseExact(splitRecord[2], format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out flightDate);
                invoicePdfModel.FlightDate = flightDate;

                invoicePdfModel.CarrierCode = splitRecord[3];
                invoicePdfModel.FlightNo = splitRecord[4];
                invoicePdfModel.TotalPrice = Decimal.Parse(splitRecord[9]);
            }
            catch (Exception e)
            {
                //ToDO Log
                throw new Exception("Invoice Record Parse Exception! " + e.Message);
            }

            return invoicePdfModel;
        }


        private bool CheckInvoiceRecordRow(string text)
        {
            try
            {
                //Rule #1: Check empty string
                if (string.IsNullOrEmpty(text)) return false;


                //Rule #2: Check start with number
                bool startWithNum = char.IsDigit(text[0]);
                if (!startWithNum) return false;


                //Rule #3: Check string contains comma
                if (!text.Contains(',')) return false;


                //Rule #4: Check string contains valid date
                //Rule #5: Check string contains 3 letter airport code
                bool containsDate = false;
                bool containsAirportCode = false;
                foreach (var textBlock in text.Split(new string[] { " " }, StringSplitOptions.None))    //separate word by word
                {
                    //Determine contains valid date
                    DateTime datetime;
                    string[] format = new string[] { "dd.MM.yyyy" };
                    if (DateTime.TryParseExact(textBlock, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out datetime))
                        containsDate = true;

                    //Determine contains 3 letter airport code
                    if (textBlock.Length == 3 && textBlock.All(Char.IsLetter))
                        containsAirportCode = true;
                }

                if (!containsDate) return false;

                if (!containsAirportCode) return false;

                return true;
            }
            catch (Exception e)
            {
                //ToDO Log
                throw new Exception("Invoice Record Check Exception! " + e.Message);
            }
        }

        public List<InvoicePdfModelDTO> Parse(string text)
        {
            if(text.IsNullOrEmpty())
            {
                //ToDO LOG
                throw new Exception("Empty String Parse Exception!");
            }

            var InvoicePdfModelDTOs = new List<InvoicePdfModelDTO>();
            var invoiceNumber = "";
            var nextLineContainsInvoiceNumber = false;

            try
            {
                //Check data line by line
                foreach (var line in text.Split(new string[] { "\n" }, StringSplitOptions.None))
                {
                    //Check next data can contain the invoice number
                    if (line.Equals("Nummer"))
                    {
                        nextLineContainsInvoiceNumber = true;
                        continue;
                    }

                    //Get invoice number
                    if (nextLineContainsInvoiceNumber)
                    {
                        nextLineContainsInvoiceNumber = false;
                        invoiceNumber = line;
                        continue;
                    }

                    //Parse reservation records
                    if (CheckInvoiceRecordRow(line))    //Check this data is reservation record
                    {
                        var item = InvoiceRecordFromString(line, invoiceNumber);    //Parse model from string

                        if (item != null)
                            InvoicePdfModelDTOs.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                //ToDO Log
                throw new Exception("Invoice Parse Exception! " + e.Message);
            }

            return InvoicePdfModelDTOs;
        }
    }
}