namespace CoralSchedular.InvoiceServices.Utils
{
    public static class FileHelper
    {
        public static List<string> GetFileList(string directoryLocation, string fileType, SearchOption searchOption)
        {
            try
            {
                return new List<string>(Directory.EnumerateFiles(directoryLocation, fileType, searchOption));

            }
            catch (Exception e)
            {
                //ToDO Log
                throw new Exception("Read Directory Error! " + e.ToString());
            }
        }

        public static void GetFileFromSFTP(string serverIP, string username, string password)
        {
            //ToDO
            //SSH.NET
        }
    }
}
