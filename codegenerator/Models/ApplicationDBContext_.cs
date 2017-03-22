using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace WEB.Models
{
    public partial class ApplicationDbContext
    {
        public DbSet<CodeReplacement> CodeReplacements { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Lookup> Lookups { get; set; }
        public DbSet<LookupOption> LookupOptions { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Relationship> Relationships { get; set; }
        public DbSet<RelationshipField> RelationshipFields { get; set; }

        public void ConfigureModelBuilder(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Relationship>()
                .HasRequired(o => o.ChildEntity)
                .WithMany(o => o.RelationshipsAsChild)
                .HasForeignKey(o => o.ChildEntityId);

            modelBuilder.Entity<Relationship>()
                .HasRequired(o => o.ParentEntity)
                .WithMany(o => o.RelationshipsAsParent)
                .HasForeignKey(o => o.ParentEntityId);

            modelBuilder.Entity<RelationshipField>()
                .HasRequired(o => o.ChildField)
                .WithMany(o => o.RelationshipFieldsAsChild)
                .HasForeignKey(o => o.ChildFieldId);

            modelBuilder.Entity<RelationshipField>()
                .HasRequired(o => o.ParentField)
                .WithMany(o => o.RelationshipFieldsAsParent)
                .HasForeignKey(o => o.ParentFieldId);

        }
    }
}
