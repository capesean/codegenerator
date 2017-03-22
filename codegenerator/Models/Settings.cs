using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;

namespace WEB.Models
{
    public class Settings
    {
        [Key]
        public int SettingsId { get; set; }

        [NotMapped]
        public string SiteName { get; set; }

        [NotMapped]
        public string RootUrl { get; set; }

        [NotMapped]
        public string EmailFromAddress { get; set; }

        [NotMapped]
        public string EmailFromName { get; set; }

        [NotMapped]
        public string EmailToErrors { get; set; }

        [NotMapped]
        public string EmailPassword { get; set; }

        [NotMapped]
        public int EmailPort { get; set; }

        [NotMapped]
        public string EmailSMTP { get; set; }

        [NotMapped]
        public bool EmailSSL { get; set; }

        [NotMapped]
        public string EmailUserName { get; set; }

        [NotMapped]
        public string SubstitutionAddress { get; set; }

        public Settings() { }

        public Settings(ApplicationDbContext dbContext)
        {
            var settings = dbContext.Settings.Single();
            SettingsId = settings.SettingsId;
            SiteName = ConfigurationManager.AppSettings["SiteName"];
            RootUrl = ConfigurationManager.AppSettings["RootUrl"];
            EmailFromAddress = ConfigurationManager.AppSettings["Email:FromAddress"];
            EmailFromName = ConfigurationManager.AppSettings["Email:FromName"];
            EmailToErrors = ConfigurationManager.AppSettings["Email:ToErrors"];
            EmailSMTP = ConfigurationManager.AppSettings["Email:SMTP"];
            EmailPort = Convert.ToInt32(ConfigurationManager.AppSettings["Email:Port"]);
            EmailPassword = ConfigurationManager.AppSettings["Email:Password"];
            EmailUserName = ConfigurationManager.AppSettings["Email:UserName"];
            EmailSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["Email:SSL"]);
            SubstitutionAddress = ConfigurationManager.AppSettings["Email:SubstitutionAddress"];
        }
    }
}
