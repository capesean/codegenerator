using System;
using WEB.Models;
using System.Data.Entity;
using System.Net.Mail;
using System.IO;

namespace WEB.Utilities
{
    public static class ErrorLogger
    {
        public static Guid ProcessExceptions(ApplicationDbContext dbContext, Error error, Exception exc)
        {
            var InnerExceptionId = (Guid?)null;

            if (exc.InnerException != null)
                InnerExceptionId = ProcessExceptions(dbContext, error, exc.InnerException);

            var Exception = new ErrorException
            {
                Id = Guid.NewGuid(),
                Message = exc.Message,
                StackTrace = exc.StackTrace,
                InnerExceptionId = InnerExceptionId
            };

            dbContext.Entry(Exception).State = EntityState.Added;
            dbContext.SaveChanges();

            return Exception.Id;
        }

        public static void Log(Exception exc, System.Web.HttpRequest request, string url, string userName)
        {
            string form = string.Empty;
            foreach (var key in request.Form.AllKeys)
                form += key + ":" + request.Form[key] + Environment.NewLine;

            if (request.RequestType == "POST" && string.IsNullOrWhiteSpace(form))
            {
                using (StreamReader sr = new StreamReader(request.InputStream))
                {
                    form = sr.ReadToEnd();
                }
            }

            using (var dbContext = new ApplicationDbContext())
            {
                var error = new Error
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.Now,
                    Message = exc.Message,
                    Url = url,
                    UserName = userName,
                    Form = form
                };

                error.ExceptionId = ProcessExceptions(dbContext, error, exc);

                try
                {
                    dbContext.Entry(error).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
                catch { }

                var settings = new Settings(dbContext);
                if (!string.IsNullOrWhiteSpace(settings.EmailToErrors))
                {
                    var body = string.Empty;
                    body += "URL: " + error.Url + Environment.NewLine;
                    body += "DATE: " + error.Date.ToString("dd MMMM yyyy, HH:mm:ss") + Environment.NewLine;
                    body += "USER: " + error.UserName + Environment.NewLine;
                    body += "MESSAGE: " + error.Message + Environment.NewLine;
                    body += Environment.NewLine;
                    body += settings.RootUrl + "api/errors/" + error.Id + Environment.NewLine;

                    using (var mail = new MailMessage())
                    {
                        mail.To.Add(new MailAddress(settings.EmailToErrors));
                        mail.Subject = settings.SiteName + " Error";
                        mail.Body = body;
                        try
                        {
                            Email.SendMail(mail, settings);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
