using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WEB.Models
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, AppRole, Guid, AppUserLogin,
        AppUserRole, AppUserClaim>
    {
        public DbSet<Error> Errors { get; set; }
        public DbSet<ErrorException> ErrorExceptions { get; set; }
        public DbSet<Settings> Settings { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection")
        {
            // auto migrate
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.Configuration>());
            Database.Initialize(false);

            //if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request.IsLocal)
                //Configuration.LazyLoadingEnabled = false;
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureModelBuilder(modelBuilder);

            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

        }
    }
}