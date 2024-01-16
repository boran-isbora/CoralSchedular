using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;

namespace CoralSchedular.InvoiceServices.Utils
{
    public static class PdfHelper
    {
        public static string GetPdfInString(String fileFullName)
        {
            try
            {
                var text = "";

                using (var reader = new PdfReader(fileFullName))
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                    for (var page = 0; page < reader.NumberOfPages; page++)
                    {
                        text = PdfTextExtractor.GetTextFromPage(reader, page + 1, strategy);
                    }

                    //Note: this code reads the entire file correctly. Must be tested before making any changes.
                }

                return text;
            }
            catch (Exception e)
            {
                //ToDO Log
                throw new Exception("Read PDF Error! " + e.ToString());
            }
        }
    }
}
