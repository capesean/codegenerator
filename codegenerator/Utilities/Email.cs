using WEB.Models;
using System.Net;
using System.Net.Mail;

namespace WEB.Utilities
{
    public static class Email
    {
        public static void SendMail(MailMessage message, Settings settings)
        {
            if (message.From == null) message.From = new MailAddress(settings.EmailFromAddress, settings.EmailFromName);

            using (var smtp = new SmtpClient(settings.EmailSMTP, settings.EmailPort))
            {
                smtp.Credentials = new NetworkCredential(settings.EmailUserName, settings.EmailPassword);
                smtp.EnableSsl = settings.EmailSSL;
                smtp.Send(message);
            }
        }
    }
}