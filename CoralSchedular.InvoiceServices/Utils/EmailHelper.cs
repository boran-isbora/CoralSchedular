using System.Net.Mail;
using System.Net;
using System.Text;
using System.Net.Mime;

namespace CoralSchedular.InvoiceServices.Utils
{
    public static class EmailHelper
    {
        public static void Send(string smtpClient, int SmtpClientPort, string fromAddress, string password,
            string toEmail, string subject, string mailBody, string mailAttachment)
        {
            // Set up SMTP client
            SmtpClient client = new SmtpClient(smtpClient, SmtpClientPort);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(fromAddress, password);

            // Create email message
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromAddress);
            mailMessage.To.Add(toEmail);
            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = true;

            mailMessage.Body = mailBody;

            // Create attachment file
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(mailAttachment));
            Attachment attachment = new Attachment(stream, new ContentType("text/csv"));
            attachment.Name = "report.csv";
            mailMessage.Attachments.Add(attachment);

            // Send email
            client.Send(mailMessage);
        }
    }
}
