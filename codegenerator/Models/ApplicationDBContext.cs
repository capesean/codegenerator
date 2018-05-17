using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace WEB.Models
{
    public class DemoInterceptor : IDbCommandInterceptor
    {
        private string[] demoIds =
        {
            "02ab706e-ef0e-4884-8a2b-4b57d834397b",
            "0485c39f-7584-4fb5-ad89-ef69b8e29add",
            "072d1811-2a63-4beb-bbf1-4b68c5e752a8",
            "12ac7a82-6a43-4fee-afa9-ba46fb35ed35",
            "12eebb2b-c391-4baa-9c0e-d85322d6e128",
            "1dcd9273-a178-4a1f-badf-5c0f9a6778fa",
            "2562568b-47e7-414d-9400-356c6c661871",
            "2ddead2b-b72c-4cdd-b5fd-2944bf15db89",
            "455979fa-6f52-4b84-8636-8a4e77a191e2",
            "46ee8222-be2c-4de2-84b7-9c41f336c0bc",
            "4791ab7b-1f9d-4078-8694-d7b6df7adf4e",
            "5124f4c0-fe7b-476b-8d2e-e3729bbd2eac",
            "6099f133-1b94-46de-b211-1516c1b7089c",
            "67ce9473-aa8e-4558-9ff5-aa8dab264856",
            "698ee10f-835d-4d0d-bbc8-74680b46c88b",
            "6da4b48b-c1c9-4d61-be19-b66ce5b00685",
            "7f063341-84f7-4449-ba06-6dcb46c4833c",
            "85d810a9-cdf8-4ce4-8825-b6a1d3af7ea9",
            "8b7d2bcd-667c-45e9-9fce-4ebadbb2adf6",
            "9475c55f-2a57-4cd1-82da-53703150cba4",
            "9a869e97-5185-4a4e-84a2-69315a9517cf",
            "a0e2d6f4-ef7b-42fc-9ad7-26ae45670ff5",
            "a52151f7-16eb-425d-9794-bdd22527c0b1",
            "ac5980e3-8157-4a8f-8b16-a9133000aa11",
            "b05ac70c-9b16-4527-a813-d46b1884b494",
            "b7c833b1-2d8f-4772-b55d-c8e36553f54b",
            "b8931bd5-b8cb-437d-af31-a9623a296103",
            "bc1aaf09-eb6c-4974-8fb6-eea4d4a57cff",
            "cee38c31-1bdf-4fc8-9465-8b55745ac37a",
            "d3a0c455-2fc4-4a44-89da-b3cc998df384",
            "d6958f35-f134-44e7-8fe2-064fe317163c",
            "dafc8c7d-011b-401d-b5b5-ad3af819f84c",
            "fbc1a214-49ba-4ed9-bb77-a5666953c62a"
        };

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            //throw new NotImplementedException();
        }

        public void NonQueryExecuting(
            DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            var name = System.Web.HttpContext.Current?.User?.Identity?.Name;
            if (name == "seanmatthewwalsh" + "@" + "gmail.com") return;
            foreach (DbParameter param in command.Parameters)
            {
                if (param != null && param.Value != null && demoIds.ToList().Contains(param.Value.ToString().ToLower()))
                    throw new Exception("You may not modify the DEMO PROJECT" + name);
            }
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            //throw new NotImplementedException();
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            //throw new NotImplementedException();
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //throw new NotImplementedException();
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //throw new NotImplementedException();
        }
    }

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

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureModelBuilder(modelBuilder);

            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Entity>().HasOptional(c => c.PrimaryField);
            modelBuilder.Entity<Field>().HasMany(a => a.PrimaryFieldEntities).WithOptional(c => c.PrimaryField);
        }
    }
}