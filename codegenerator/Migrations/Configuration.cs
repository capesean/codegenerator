namespace WEB.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Linq;
    using WEB.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        public void Seed()
        {
            // uncomment if/when you want to create a login
            //Seed(new ApplicationDbContext());
        }

        protected override void Seed(ApplicationDbContext context)
        {
            using (var roleStore = new RoleStore<AppRole, Guid, AppUserRole>(context))
            using (var roleManager = new RoleManager<AppRole, Guid>(roleStore))
            using (var userManager = new AppUserManager(new AppUserStore(context)))
            {
                if (userManager.Users.Count() == 0)
                {
                    var email = System.Configuration.ConfigurationManager.AppSettings["DefaultUser:Email"];
                    ApplicationUser user;
                    if (userManager.FindByEmail(email) == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            FirstName = System.Configuration.ConfigurationManager.AppSettings["DefaultUser:FirstName"],
                            LastName = System.Configuration.ConfigurationManager.AppSettings["DefaultUser:LastName"],
                            EmailConfirmed = true
                        };
                        userManager.Create(user, System.Configuration.ConfigurationManager.AppSettings["DefaultUser:Password"]);
                    }

                    user = userManager.FindByEmail(email);
                    foreach (var role in Enum.GetNames(typeof(Roles)))
                    {
                        if (!roleManager.RoleExists(role.ToLower()))
                        {
                            roleManager.Create(new AppRole() { Name = role, Id = Guid.NewGuid() });
                        }

                        if (!userManager.IsInRole(user.Id, role))
                        {
                            userManager.AddToRole(user.Id, role);
                        }
                    }
                }

                if (context.Settings.SingleOrDefault() == null)
                {
                    var settings = new Settings();

                    context.Entry(settings).State = EntityState.Added;

                    context.SaveChanges();
                }
            }
        }
    }
}
