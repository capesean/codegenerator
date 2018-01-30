using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.IO;

namespace WEB.Models
{
    public class Code
    {
        private Entity CurrentEntity { get; set; }
        private List<Entity> _allEntities { get; set; }
        private List<Entity> NormalEntities { get { return AllEntities.Where(e => e.EntityType == EntityType.Normal && !e.Exclude).ToList(); } }
        private List<Entity> AllEntities
        {
            get
            {
                if (_allEntities == null)
                {
                    _allEntities = DbContext.Entities
                        .Include(e => e.Project)
                        .Include(e => e.Fields)
                        .Include(e => e.CodeReplacements)
                        .Include(e => e.RelationshipsAsChild.Select(p => p.RelationshipFields))
                        .Include(e => e.RelationshipsAsChild.Select(p => p.ParentEntity))
                        .Include(e => e.RelationshipsAsParent.Select(p => p.RelationshipFields))
                        .Include(e => e.RelationshipsAsParent.Select(p => p.ChildEntity))
                        .Where(e => e.ProjectId == CurrentEntity.ProjectId).OrderBy(e => e.Name).ToList();
                }
                return _allEntities;
            }
        }
        private List<Lookup> _lookups { get; set; }
        private List<Lookup> Lookups
        {
            get
            {
                if (_lookups == null)
                {
                    _lookups = DbContext.Lookups.Include(l => l.LookupOptions).Where(l => l.ProjectId == CurrentEntity.ProjectId).OrderBy(l => l.Name).ToList();
                }
                return _lookups;
            }
        }
        private List<CodeReplacement> _codeReplacements { get; set; }
        private List<CodeReplacement> CodeReplacements
        {
            get
            {
                if (_codeReplacements == null)
                {
                    _codeReplacements = DbContext.CodeReplacements.Include(cr => cr.Entity).Where(cr => cr.Entity.ProjectId == CurrentEntity.ProjectId).OrderBy(cr => cr.SortOrder).ToList();
                }
                return _codeReplacements;
            }
        }
        private List<RelationshipField> _relationshipFields { get; set; }
        private List<RelationshipField> RelationshipFields
        {
            get
            {
                if (_relationshipFields == null)
                {
                    _relationshipFields = DbContext.RelationshipFields.Where(rf => rf.Relationship.ParentEntity.ProjectId == CurrentEntity.ProjectId).ToList();
                }
                return _relationshipFields;
            }
        }
        private ApplicationDbContext DbContext { get; set; }
        private Entity GetEntity(Guid entityId)
        {
            return AllEntities.Single(e => e.EntityId == entityId);
        }
        private Relationship ParentHierarchyRelationship
        {
            get { return CurrentEntity.RelationshipsAsChild.SingleOrDefault(r => r.Hierarchy); }
        }

        public Code(Entity currentEntity, ApplicationDbContext dbContext)
        {
            CurrentEntity = currentEntity;
            DbContext = dbContext;
        }

        public string GenerateModel()
        {
            //if (CurrentEntity.EntityType == EntityType.User) throw new NotImplementedException("Not Implemented: GenerateModel for User entity");

            var s = new StringBuilder();
            s.Add($"using System;");
            if (CurrentEntity.RelationshipsAsParent.Any())
                s.Add($"using System.Collections.Generic;");
            s.Add($"using System.ComponentModel.DataAnnotations;");
            if (CurrentEntity.RelationshipsAsChild.Any() || CurrentEntity.Fields.Any(f => f.IsUnique) || CurrentEntity.Fields.Any(f => f.FieldType == FieldType.Date) || CurrentEntity.KeyFields.Count > 1 || CurrentEntity.Fields.Any(f => f.EditPageType == EditPageType.CalculatedField))
                s.Add($"using System.ComponentModel.DataAnnotations.Schema;");
            s.Add($"");
            s.Add($"namespace {CurrentEntity.Project.Namespace}.Models");
            s.Add($"{{");
            s.Add($"    public {(CurrentEntity.PartialEntityClass ? "partial " : string.Empty)}class {CurrentEntity.Name}");
            s.Add($"    {{");

            // fields
            var keyCounter = 0;
            foreach (var field in CurrentEntity.Fields.OrderBy(f => f.FieldOrder))
            {
                var parentHierarchyRelationshipIndexFieldCount = 0;
                if (field.KeyField && CurrentEntity.EntityType == EntityType.User) continue;

                if (field.KeyField)
                {
                    s.Add($"        [Key]");
                    if (CurrentEntity.KeyFields.Count > 1)
                        s.Add($"        [Column(Order = {keyCounter})]");
                    keyCounter++;
                }

                if (field.EditPageType == EditPageType.CalculatedField)
                {
                    s.Add($"        [NotMapped]");
                    s.Add($"        public {field.NetType.ToString()} {field.Name} {{ get {{ return {field.CalculatedFieldDefinition}; }} }}");
                }
                else
                {
                    if (!field.IsNullable)
                    {
                        if (field.CustomType == CustomType.String)
                            s.Add($"        [Required(AllowEmptyStrings = true)]");
                        else
                            s.Add($"        [Required]");
                    }
                    if (field.NetType == "string")
                    {
                        if (field.Length == 0 && (field.FieldType == FieldType.Varchar || field.FieldType == FieldType.nVarchar))
                        {
                            //s.Add($"        [Column(TypeName = \"{field.FieldType.ToString().ToLower()}(MAX)\")]");
                        }
                        else if (field.Length > 0)
                        {
                            s.Add($"        [MaxLength({field.Length}){(field.MinLength > 0 ? $", MinLength({field.MinLength})" : "")}]");
                        }
                    }
                    if (field.IsUnique)
                        s.Add($"        [Index(\"IX_{CurrentEntity.Name}_{field.Name}\", IsUnique = true, Order = 0)]");
                    if (field.FieldType == FieldType.Date)
                        s.Add($"        [Column(TypeName = \"Date\")]");
                    else
                    {
                        // if it's a hierarchy, the unique key must be on the unique field (above) AND the relationship link fields to the hierarhcy parent 
                        if (ParentHierarchyRelationship != null && ParentHierarchyRelationship.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId))
                        {
                            // DISABLED THIS FOR IBER BECAUSE DISTRICT HAS TWO ISUNIQUE FIELDS, NOT SURE WHAT THIS IS MEANT TO DO

                            //// need to get the single unique field for the index name
                            //var uniqueField = CurrentEntity.Fields.SingleOrDefault(f => f.IsUnique);
                            //if (uniqueField != null)
                            //{
                            //    parentHierarchyRelationshipIndexFieldCount++;
                            //    s.Add($"        [Index(\"IX_{CurrentEntity.Name}_{uniqueField.Name}\", IsUnique = true, Order = {parentHierarchyRelationshipIndexFieldCount})]");
                            //}
                        }
                    }
                    s.Add($"        public {field.NetType.ToString()} {field.Name} {{ get; set; }}");
                }
                s.Add($"");
            }

            // child entities
            foreach (var relationship in CurrentEntity.RelationshipsAsParent.Where(r => !r.ChildEntity.Exclude).OrderBy(o => o.SortOrder))
            {
                s.Add($"        public virtual ICollection<{GetEntity(relationship.ChildEntityId).Name}> {relationship.CollectionName} {{ get; set; }} = new List<{GetEntity(relationship.ChildEntityId).Name}>();");
                s.Add($"");
            }

            // parent entities
            foreach (var relationship in CurrentEntity.RelationshipsAsChild.Where(r => !r.ParentEntity.Exclude).OrderBy(o => o.ParentEntity.Name))
            {
                if (relationship.RelationshipFields.Count() == 1)
                    s.Add($"        [ForeignKey(\"" + relationship.RelationshipFields.Single().ChildField.Name + "\")]");
                s.Add($"        public virtual {GetEntity(relationship.ParentEntityId).Name} {relationship.ParentName} {{ get; set; }}");
                s.Add($"");
            }

            // constructor
            if (CurrentEntity.KeyFields.Any(f => f.KeyField && f.FieldType == FieldType.Guid))
            {
                s.Add($"        public {CurrentEntity.Name}()");
                s.Add($"        {{");
                foreach (var field in CurrentEntity.KeyFields)
                    if (field.FieldType == FieldType.Guid)
                        s.Add($"            {field.Name} = Guid.NewGuid();");
                s.Add($"        }}");
            }

            s.Add($"    }}");
            s.Add($"}}");

            return RunCodeReplacements(s.ToString(), CodeType.Model);
        }

        public string GenerateEnums()
        {
            var s = new StringBuilder();

            s.Add($"namespace {CurrentEntity.Project.Namespace}.Models");
            s.Add($"{{");
            foreach (var lookup in Lookups)
            {
                s.Add($"    public enum " + lookup.Name);
                s.Add($"    {{");
                var options = lookup.LookupOptions.OrderBy(o => o.SortOrder);
                foreach (var option in options)
                    s.Add($"        {option.Name}{(option.Value.HasValue ? " = " + option.Value : string.Empty)}" + (option == options.Last() ? string.Empty : ","));
                s.Add($"    }}");
                s.Add($"");
            }
            s.Add($"    public static class Extensions");
            s.Add($"    {{");
            foreach (var lookup in Lookups)
            {
                s.Add($"        public static string Label(this {lookup.Name} {lookup.Name.ToCamelCase()})");
                s.Add($"        {{");
                s.Add($"            switch ({lookup.Name.ToCamelCase()})");
                s.Add($"            {{");
                var options = lookup.LookupOptions.OrderBy(o => o.SortOrder);
                foreach (var option in options)
                {
                    s.Add($"                case {lookup.Name}.{option.Name}:");
                    s.Add($"                    return \"{option.FriendlyName.Replace("\"", "\\\"")}\";");
                }
                s.Add($"                default:");
                s.Add($"                    return null;");
                s.Add($"            }}");
                s.Add($"        }}");
                s.Add($"");
            }
            s.Add($"    }}");
            s.Add($"}}");

            return RunCodeReplacements(s.ToString(), CodeType.Enums);
        }

        public string GenerateSettingsDTO()
        {
            var s = new StringBuilder();

            s.Add($"using System;");
            s.Add($"using System.Collections.Generic;");
            s.Add($"");
            s.Add($"namespace {CurrentEntity.Project.Namespace}.Models");
            s.Add($"{{");
            s.Add($"    public partial class SettingsDTO");
            s.Add($"    {{");
            foreach (var lookup in Lookups)
                s.Add($"        public List<EnumDTO> {lookup.Name} {{ get; set; }}");
            s.Add($"");
            s.Add($"        public SettingsDTO()");
            s.Add($"        {{");
            foreach (var lookup in Lookups)
            {
                s.Add($"            {lookup.Name} = new List<EnumDTO>();");
                s.Add($"            foreach ({lookup.Name} type in Enum.GetValues(typeof({lookup.Name})))");
                s.Add($"                {lookup.Name}.Add(new EnumDTO {{ Id = (int)type, Name = type.ToString(), Label = type.Label() }});");
                s.Add($"");
            }
            s.Add($"        }}");
            s.Add($"    }}");
            s.Add($"}}");

            return RunCodeReplacements(s.ToString(), CodeType.SettingsDTO);
        }

        public string GenerateDTO()
        {
            //if (CurrentEntity.EntityType == EntityType.User) return string.Empty;

            var s = new StringBuilder();

            s.Add($"using System;");
            s.Add($"using System.ComponentModel.DataAnnotations;");
            if (CurrentEntity.EntityType == EntityType.User)
                s.Add($"using System.Collections.Generic;");
            s.Add($"");
            s.Add($"namespace {CurrentEntity.Project.Namespace}.Models");
            s.Add($"{{");
            s.Add($"    public class {CurrentEntity.DTOName}");
            s.Add($"    {{");
            foreach (var field in CurrentEntity.Fields.OrderBy(f => f.FieldOrder))
            {
                if (field.EditPageType != EditPageType.CalculatedField)
                {
                    if (!field.IsNullable)
                    {
                        // to allow empty strings, can't be null and must use convertemptystringtonull...
                        if (field.CustomType == CustomType.String)
                            s.Add($"        [DisplayFormat(ConvertEmptyStringToNull = false)]");
                        else if (field.EditPageType != EditPageType.ReadOnly)
                            s.Add($"        [Required]");
                    }
                    if (field.NetType == "string" && field.Length > 0)
                        s.Add($"        [MaxLength({field.Length})]");
                }
                // force nullable for readonly fields
                s.Add($"        public {Field.GetNetType(field.FieldType, field.EditPageType == EditPageType.ReadOnly ? true : field.IsNullable, field.Lookup)} {field.Name} {{ get; set; }}");
                s.Add($"");
            }
            foreach (var relationship in CurrentEntity.RelationshipsAsChild.OrderBy(r => r.SortOrder))
            {
                // using exclude to avoid circular references. example: KTU-PACK: version => localisation => contentset => version (UpdateFromVersion)
                if (relationship.RelationshipAncestorLimit == RelationshipAncestorLimits.Exclude) continue;
                s.Add($"        public {relationship.ParentEntity.Name}DTO {relationship.ParentName} {{ get; set; }}");
                s.Add($"");
            }
            if (CurrentEntity.EntityType == EntityType.User)
            {
                s.Add($"        public string Email {{ get; set; }}");
                s.Add($"");
                s.Add($"        public IList<Guid> RoleIds {{ get; set; }}");
                s.Add($"");
            }
            s.Add($"    }}");
            s.Add($"");
            s.Add($"    public partial class ModelFactory");
            s.Add($"    {{");
            s.Add($"        public {CurrentEntity.DTOName} Create({CurrentEntity.Name} {CurrentEntity.CamelCaseName})");
            s.Add($"        {{");
            s.Add($"            if ({CurrentEntity.CamelCaseName} == null) return null;");
            s.Add($"");
            if (CurrentEntity.EntityType == EntityType.User)
            {
                s.Add($"            var roleIds = new List<Guid>();");
                s.Add($"            foreach (var role in user.Roles)");
                s.Add($"                roleIds.Add(role.RoleId);");
                s.Add($"");
            }
            s.Add($"            var {CurrentEntity.DTOName.ToCamelCase()} = new {CurrentEntity.DTOName}();");
            s.Add($"");
            foreach (var field in CurrentEntity.Fields.OrderBy(f => f.FieldOrder))
            {
                s.Add($"            {CurrentEntity.DTOName.ToCamelCase()}.{field.Name} = {CurrentEntity.CamelCaseName}.{field.Name};");
            }
            if (CurrentEntity.EntityType == EntityType.User)
            {
                s.Add($"            {CurrentEntity.DTOName.ToCamelCase()}.Email = {CurrentEntity.CamelCaseName}.Email;");
                s.Add($"            {CurrentEntity.DTOName.ToCamelCase()}.RoleIds = roleIds;");
            }
            foreach (var relationship in CurrentEntity.RelationshipsAsChild.OrderBy(o => o.ParentFriendlyName))
            {
                // using exclude to avoid circular references. example: KTU-PACK: version => localisation => contentset => version (UpdateFromVersion)
                if (relationship.RelationshipAncestorLimit == RelationshipAncestorLimits.Exclude) continue;
                s.Add($"            {CurrentEntity.DTOName.ToCamelCase()}.{relationship.ParentName} = Create({CurrentEntity.CamelCaseName}.{relationship.ParentName});");
            }
            s.Add($"");
            s.Add($"            return {CurrentEntity.DTOName.ToCamelCase()};");
            s.Add($"        }}");
            s.Add($"");
            s.Add($"        public void Hydrate({CurrentEntity.Name} {CurrentEntity.CamelCaseName}, {CurrentEntity.DTOName} {CurrentEntity.DTOName.ToCamelCase()})");
            s.Add($"        {{");
            if (CurrentEntity.EntityType == EntityType.User)
            {
                s.Add($"            {CurrentEntity.CamelCaseName}.UserName = {CurrentEntity.DTOName.ToCamelCase()}.Email;");
                s.Add($"            {CurrentEntity.CamelCaseName}.Email = {CurrentEntity.DTOName.ToCamelCase()}.Email;");
            }
            foreach (var field in CurrentEntity.Fields.OrderBy(f => f.FieldOrder))
            {
                if (field.KeyField || field.EditPageType == EditPageType.ReadOnly) continue;
                if (field.EditPageType == EditPageType.Exclude || field.EditPageType == EditPageType.CalculatedField) continue;
                s.Add($"            {CurrentEntity.CamelCaseName}.{field.Name} = {CurrentEntity.DTOName.ToCamelCase()}.{field.Name};");
            }
            s.Add($"        }}");
            s.Add($"    }}");
            s.Add($"}}");

            return RunCodeReplacements(s.ToString(), CodeType.DTO);
        }

        public string GenerateDbContext()
        {
            var s = new StringBuilder();

            s.Add($"using Microsoft.AspNet.Identity.EntityFramework;");
            s.Add($"using System.Data.Entity;");
            s.Add($"");
            s.Add($"namespace {CurrentEntity.Project.Namespace}.Models");
            s.Add($"{{");
            s.Add($"    public partial class ApplicationDbContext");
            s.Add($"    {{");
            foreach (var e in NormalEntities)
                s.Add($"        public DbSet<{e.Name}> {e.PluralName} {{ get; set; }}");
            s.Add($"");
            s.Add($"        public void ConfigureModelBuilder(DbModelBuilder modelBuilder)");
            s.Add($"        {{");
            foreach (var entity in AllEntities)
            {
                foreach (var relationship in entity.RelationshipsAsChild)
                {
                    if (!entity.RelationshipsAsChild.Any(r => r.ParentEntityId == relationship.ParentEntityId && r.RelationshipId != relationship.RelationshipId)) continue;
                    s.Add($"            modelBuilder.Entity<{entity.Name}>()");
                    s.Add($"                .{(relationship.RelationshipFields.First().ChildField.IsNullable ? "HasOptional" : "HasRequired")}(o => o.{relationship.ParentName})");
                    s.Add($"                .WithMany(o => o.{relationship.CollectionName})");
                    s.Add($"                .HasForeignKey(o => o.{relationship.RelationshipFields.First().ChildField.Name});");
                    s.Add($"");
                }
            }
            var decimalFields = DbContext.Fields.Where(f => f.FieldType == FieldType.Decimal && f.Entity.ProjectId == CurrentEntity.ProjectId).OrderBy(f => f.Entity.Name).ThenBy(f => f.FieldOrder).ToList();
            foreach (var field in decimalFields)
            {
                s.Add($"            modelBuilder.Entity<{field.Entity.Name}>().Property(o => o.{field.Name}).HasPrecision({field.Precision}, {field.Scale});");
            }
            var smallDateTimeFields = DbContext.Fields.Where(f => f.FieldType == FieldType.SmallDateTime && f.Entity.ProjectId == CurrentEntity.ProjectId).OrderBy(f => f.Entity.Name).ThenBy(f => f.FieldOrder).ToList();
            foreach (var field in smallDateTimeFields)
            {
                s.Add($"            modelBuilder.Entity<{field.Entity.Name}>().Property(o => o.{field.Name}).HasColumnType(\"smalldatetime\");");
            }
            s.Add($"        }}");
            s.Add($"    }}");
            s.Add($"}}");

            return RunCodeReplacements(s.ToString(), CodeType.DbContext);
        }

        public string GenerateController()
        {
            if (CurrentEntity.EntityType == EntityType.User) return string.Empty;

            var s = new StringBuilder();

            s.Add($"using System;");
            s.Add($"using System.Data.Entity;");
            s.Add($"using System.Linq;");
            s.Add($"using System.Web.Http;");
            s.Add($"using System.Threading.Tasks;");
            s.Add($"using {CurrentEntity.Project.Namespace}.Models;");
            s.Add($"");
            s.Add($"namespace {CurrentEntity.Project.Namespace}.Controllers");
            s.Add($"{{");
            s.Add($"    [Authorize{(CurrentEntity.AuthorizationType == AuthorizationType.ProtectAll ? (CurrentEntity.Project.UseStringAuthorizeAttributes ? "(Roles = \"Administrator\")" : "Roles(Roles.Administrator)") : string.Empty)}, RoutePrefix(\"api/{CurrentEntity.PluralName.ToLower()}\")]");
            s.Add($"    public {(CurrentEntity.PartialControllerClass ? "partial " : string.Empty)}class {CurrentEntity.PluralName}Controller : BaseApiController");
            s.Add($"    {{");

            #region search
            s.Add($"        [HttpGet, Route(\"\")]");

            var fieldsToSearch = new List<Field>();
            foreach (var relationship in CurrentEntity.RelationshipsAsChild.OrderBy(r => r.RelationshipFields.Min(f => f.ChildField.FieldOrder)))
                foreach (var relationshipField in relationship.RelationshipFields)
                    fieldsToSearch.Add(relationshipField.ChildField);
            foreach (var field in CurrentEntity.ExactSearchFields)
                if (!fieldsToSearch.Contains(field))
                    fieldsToSearch.Add(field);

            s.Add($"        public async Task<IHttpActionResult> Search([FromUri]PagingOptions pagingOptions{(CurrentEntity.TextSearchFields.Count > 0 ? ", [FromUri]string q = null" : "")}{(fieldsToSearch.Count > 0 ? $", {fieldsToSearch.Select(f => "[FromUri]" + (new Field { Name = f.Name, Lookup = f.Lookup, FieldType = f.FieldType, IsNullable = true }).NetType + " " + f.Name.ToCamelCase() + " = null").Aggregate((current, next) => current + ", " + next)}" : "")})");
            s.Add($"        {{");
            s.Add($"            IQueryable<{CurrentEntity.Name}> results = DbContext.{CurrentEntity.PluralName};");

            if (CurrentEntity.RelationshipsAsChild.Where(r => r.RelationshipAncestorLimit != RelationshipAncestorLimits.Exclude).Any())
            {
                s.Add($"            if (pagingOptions.IncludeEntities)");
                s.Add($"            {{");
                foreach (var relationship in CurrentEntity.RelationshipsAsChild.Where(r => r.RelationshipAncestorLimit != RelationshipAncestorLimits.Exclude).OrderBy(r => r.SortOrder))
                {
                    foreach (var result in GetTopAncestors(new List<string>(), "o", relationship, relationship.RelationshipAncestorLimit))
                        s.Add($"                results = results.Include(o => {result});");
                }
                s.Add($"            }}");
            }



            if (CurrentEntity.TextSearchFields.Count > 0)
            {
                s.Add($"");
                s.Add($"            if (!string.IsNullOrWhiteSpace(q))");
                s.Add($"                results = results.Where(o => {CurrentEntity.TextSearchFields.Select(o => $"o.{o.Name + (o.CustomType == CustomType.String ? string.Empty : ".ToString()")}.Contains(q)").Aggregate((current, next) => current + " || " + next) });");
            }

            if (fieldsToSearch.Count > 0)
            {
                s.Add($"");
                foreach (var field in fieldsToSearch)
                {
                    s.Add($"            if ({field.Name.ToCamelCase()}{(field.CustomType == CustomType.String ? " != null" : ".HasValue")}) results = results.Where(o => o.{field.Name} == {field.Name.ToCamelCase()});");
                }
            }

            s.Add($"");
            if (CurrentEntity.SortOrderFields.Count > 0)
                s.Add($"            results = results.Order{CurrentEntity.SortOrderFields.Select(f => "By" + (f.SortDescending ? "Descending" : string.Empty) + "(o => o." + f.Name + ")").Aggregate((current, next) => current + ".Then" + next)};");

            //var counter = 0;
            //foreach (var field in CurrentEntity.SortOrderFields)
            //{
            //    s.Add($"            results = results.{(counter == 0 ? "Order" : "Then")}By(o => o.{field.Name});");
            //    counter++;
            //}
            s.Add($"");
            s.Add($"            return Ok((await GetPaginatedResponse(results, pagingOptions)).Select(o => ModelFactory.Create(o)));");
            s.Add($"        }}");
            s.Add($"");
            #endregion

            #region get
            s.Add($"        [HttpGet, Route(\"{CurrentEntity.RoutePath}\")]");
            s.Add($"        public async Task<IHttpActionResult> Get({CurrentEntity.ControllerParameters})");
            s.Add($"        {{");
            s.Add($"            var {CurrentEntity.CamelCaseName} = await DbContext.{CurrentEntity.PluralName}");
            foreach (var relationship in CurrentEntity.RelationshipsAsChild.Where(r => r.RelationshipAncestorLimit != RelationshipAncestorLimits.Exclude).OrderBy(r => r.SortOrder))
            {
                foreach (var result in GetTopAncestors(new List<string>(), "o", relationship, relationship.RelationshipAncestorLimit))
                    s.Add($"                .Include(o => {result})");
            }
            s.Add($"                .SingleOrDefaultAsync(o => {GetKeyFieldLinq("o")});");
            s.Add($"");
            s.Add($"            if ({CurrentEntity.CamelCaseName} == null)");
            s.Add($"                return NotFound();");
            s.Add($"");
            s.Add($"            return Ok(ModelFactory.Create({CurrentEntity.CamelCaseName}));");
            s.Add($"        }}");
            s.Add($"");
            #endregion

            #region insert
            if (!CurrentEntity.HasCompositePrimaryKey)
            {
                // composite keys can't insert with blank keys - have to use the update method with a modified isnew check (if record exists)
                s.Add($"        [{(CurrentEntity.AuthorizationType == AuthorizationType.ProtectChanges ? (CurrentEntity.Project.UseStringAuthorizeAttributes ? "Authorize(Roles = \"Administrator\"), " : "AuthorizeRoles(Roles.Administrator), ") : string.Empty)}HttpPost, Route(\"\")]");
                s.Add($"        public async Task<IHttpActionResult> Insert([FromBody]{CurrentEntity.DTOName} {CurrentEntity.DTOName.ToCamelCase()})");
                s.Add($"        {{");
                foreach (var field in CurrentEntity.KeyFields)
                    s.Add($"            if ({CurrentEntity.DTOName.ToCamelCase()}.{field.Name} != {field.EmptyValue}) return BadRequest(\"Invalid {field.Name}\");");
                s.Add($"");
                // replaced with code in the save method, directly on the object, not the DTO
                //var defaults = CurrentEntity.Fields.Where(f => !string.IsNullOrWhiteSpace(f.ControllerInsertOverride)).ToList();
                //foreach (var field in defaults)
                //    s.Add($"            {CurrentEntity.DTOName.ToCamelCase()}.{field.Name} = {field.ControllerInsertOverride};");
                //if (defaults.Count > 0)
                //    s.Add($"");
                s.Add($"            return await Save({CurrentEntity.DTOName.ToCamelCase()});");
                s.Add($"        }}");
                s.Add($"");
            }
            #endregion

            #region update
            s.Add($"        [{(CurrentEntity.AuthorizationType == AuthorizationType.ProtectChanges ? (CurrentEntity.Project.UseStringAuthorizeAttributes ? "Authorize(Roles = \"Administrator\"), " : "AuthorizeRoles(Roles.Administrator), ") : string.Empty)}HttpPost, Route(\"{CurrentEntity.RoutePath}\")]");
            s.Add($"        public async Task<IHttpActionResult> Update({CurrentEntity.ControllerParameters}, [FromBody]{CurrentEntity.DTOName} {CurrentEntity.DTOName.ToCamelCase()})");
            s.Add($"        {{");
            s.Add($"            if ({GetKeyFieldLinq(CurrentEntity.DTOName.ToCamelCase(), null, "!=", "||")}) return BadRequest(\"Id mismatch\");");
            s.Add($"");
            s.Add($"            return await Save({CurrentEntity.DTOName.ToCamelCase()});");
            s.Add($"        }}");
            s.Add($"");
            #endregion

            #region save
            s.Add($"        private async Task<IHttpActionResult> Save({CurrentEntity.DTOName} {CurrentEntity.DTOName.ToCamelCase()})");
            s.Add($"        {{");
            s.Add($"            if (!ModelState.IsValid)");
            s.Add($"                return BadRequest(ModelState);");
            s.Add($"");
            foreach (var field in CurrentEntity.Fields.Where(f => f.IsUnique))
            {
                string hierarchyFields = string.Empty;
                if (ParentHierarchyRelationship != null)
                {
                    foreach (var relField in ParentHierarchyRelationship.RelationshipFields)
                        hierarchyFields += (hierarchyFields == string.Empty ? "" : " && ") + "o." + relField.ChildField.Name + " == " + CurrentEntity.DTOName.ToCamelCase() + "." + relField.ChildField.Name;
                    hierarchyFields += " && ";
                }
                s.Add($"            if (DbContext.{CurrentEntity.PluralName}.Any(o => {hierarchyFields}o.{field.Name} == {CurrentEntity.DTOName.ToCamelCase()}.{field.Name} && !({GetKeyFieldLinq("o", CurrentEntity.DTOName.ToCamelCase())})))");
                s.Add($"                return BadRequest(\"{field.Label} already exists{(ParentHierarchyRelationship == null ? string.Empty : " on this " + ParentHierarchyRelationship.ParentEntity.FriendlyName)}.\");");
                s.Add($"");
            }
            if (CurrentEntity.HasCompositePrimaryKey)
            {
                // composite keys don't use the insert method, they use the update for both inserts & updates
                s.Add($"            var {CurrentEntity.CamelCaseName} = await DbContext.{CurrentEntity.PluralName}.SingleOrDefaultAsync(o => {GetKeyFieldLinq("o", CurrentEntity.DTOName.ToCamelCase())});");
                s.Add($"            var isNew = {CurrentEntity.CamelCaseName} == null;");
                s.Add($"");
                s.Add($"            if (isNew)");
                s.Add($"            {{");
                s.Add($"                {CurrentEntity.CamelCaseName} = new {CurrentEntity.Name}();");
                s.Add($"");
                foreach (var field in CurrentEntity.Fields.Where(f => f.KeyField && f.Entity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(rf => rf.ChildFieldId == f.FieldId))))
                {
                    s.Add($"                {CurrentEntity.CamelCaseName}.{field.Name} = {CurrentEntity.DTOName.ToCamelCase() + "." + field.Name};");
                }
                foreach (var field in CurrentEntity.Fields.Where(f => !string.IsNullOrWhiteSpace(f.ControllerInsertOverride)))
                {
                    s.Add($"                {CurrentEntity.CamelCaseName}.{field.Name} = {field.ControllerInsertOverride};");
                }
                if (CurrentEntity.Fields.Any(f => !string.IsNullOrWhiteSpace(f.ControllerInsertOverride)) || CurrentEntity.Fields.Any(f => f.KeyField && f.Entity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(rf => rf.ChildFieldId == f.FieldId))))
                    s.Add($"");
                if (CurrentEntity.HasASortField)
                {
                    var field = CurrentEntity.SortField;
                    var sort = $"                {CurrentEntity.DTOName.ToCamelCase()}.{field.Name} = (await DbContext.{CurrentEntity.PluralName}";
                    if (CurrentEntity.RelationshipsAsChild.Any(r => r.Hierarchy) && CurrentEntity.RelationshipsAsChild.Count(r => r.Hierarchy) == 1)
                    {
                        sort += ".Where(o => " + (CurrentEntity.RelationshipsAsChild.Single(r => r.Hierarchy).RelationshipFields.Select(o => $"o.{o.ChildField.Name} == {CurrentEntity.DTOName.ToCamelCase()}.{o.ChildField.Name}").Aggregate((current, next) => current + " && " + next)) + ")";
                    }
                    sort += $".MaxAsync(o => (int?)o.{field.Name}) ?? -1) + 1;";
                    s.Add(sort);
                    s.Add($"");
                }
                s.Add($"                DbContext.Entry({CurrentEntity.CamelCaseName}).State = EntityState.Added;");
                s.Add($"            }}");
                s.Add($"            else");
                s.Add($"            {{");
                foreach (var field in CurrentEntity.Fields.Where(f => !string.IsNullOrWhiteSpace(f.ControllerUpdateOverride)).OrderBy(f => f.FieldOrder))
                {
                    s.Add($"                {CurrentEntity.CamelCaseName}.{field.Name} = {field.ControllerUpdateOverride};");
                }
                if (CurrentEntity.Fields.Any(f => !string.IsNullOrWhiteSpace(f.ControllerUpdateOverride)))
                    s.Add($"");
                s.Add($"                DbContext.Entry({CurrentEntity.CamelCaseName}).State = EntityState.Modified;");
                s.Add($"            }}");
            }
            else
            {
                s.Add($"            var isNew = {CurrentEntity.KeyFields.Select(f => CurrentEntity.DTOName.ToCamelCase() + "." + f.Name + " == " + f.EmptyValue).Aggregate((current, next) => current + " && " + next)};");
                s.Add($"");
                s.Add($"            {CurrentEntity.Name} {CurrentEntity.CamelCaseName};");
                s.Add($"            if (isNew)");
                s.Add($"            {{");
                s.Add($"                {CurrentEntity.CamelCaseName} = new {CurrentEntity.Name}();");
                s.Add($"");
                foreach (var field in CurrentEntity.Fields.Where(f => !string.IsNullOrWhiteSpace(f.ControllerInsertOverride)))
                {
                    s.Add($"                {CurrentEntity.CamelCaseName}.{field.Name} = {field.ControllerInsertOverride};");
                }
                if (CurrentEntity.Fields.Any(f => !string.IsNullOrWhiteSpace(f.ControllerInsertOverride)))
                    s.Add($"");
                if (CurrentEntity.HasASortField)
                {
                    var field = CurrentEntity.SortField;
                    var sort = $"                {CurrentEntity.DTOName.ToCamelCase()}.{field.Name} = (await DbContext.{CurrentEntity.PluralName}";
                    if (CurrentEntity.RelationshipsAsChild.Any(r => r.Hierarchy) && CurrentEntity.RelationshipsAsChild.Count(r => r.Hierarchy) == 1)
                    {
                        sort += ".Where(o => " + (CurrentEntity.RelationshipsAsChild.Single(r => r.Hierarchy).RelationshipFields.Select(o => $"o.{o.ChildField.Name} == {CurrentEntity.DTOName.ToCamelCase()}.{o.ChildField.Name}").Aggregate((current, next) => current + " && " + next)) + ")";
                    }
                    sort += $".MaxAsync(o => (int?)o.{field.Name}) ?? 0) + 1;";
                    s.Add(sort);
                    s.Add($"");
                }
                s.Add($"                DbContext.Entry({CurrentEntity.CamelCaseName}).State = EntityState.Added;");
                s.Add($"            }}");
                s.Add($"            else");
                s.Add($"            {{");
                s.Add($"                {CurrentEntity.CamelCaseName} = await DbContext.{CurrentEntity.PluralName}.SingleOrDefaultAsync(o => {GetKeyFieldLinq("o", CurrentEntity.DTOName.ToCamelCase())});");
                s.Add($"");
                s.Add($"                if ({CurrentEntity.CamelCaseName} == null)");
                s.Add($"                    return NotFound();");
                s.Add($"");
                foreach (var field in CurrentEntity.Fields.Where(f => !string.IsNullOrWhiteSpace(f.ControllerUpdateOverride)))
                {
                    s.Add($"                {CurrentEntity.CamelCaseName}.{field.Name} = {field.ControllerUpdateOverride};");
                }
                if (CurrentEntity.Fields.Any(f => !string.IsNullOrWhiteSpace(f.ControllerUpdateOverride)))
                    s.Add($"");
                s.Add($"                DbContext.Entry({CurrentEntity.CamelCaseName}).State = EntityState.Modified;");
                s.Add($"            }}");
            }
            s.Add($"");
            s.Add($"            ModelFactory.Hydrate({CurrentEntity.CamelCaseName}, {CurrentEntity.DTOName.ToCamelCase()});");
            s.Add($"");
            s.Add($"            await DbContext.SaveChangesAsync();");
            s.Add($"");
            s.Add($"            return await Get({CurrentEntity.KeyFields.Select(f => CurrentEntity.CamelCaseName + "." + f.Name).Aggregate((current, next) => current + ", " + next)});");
            s.Add($"        }}");
            s.Add($"");
            #endregion

            #region delete
            s.Add($"        [{(CurrentEntity.AuthorizationType == AuthorizationType.ProtectChanges ? (CurrentEntity.Project.UseStringAuthorizeAttributes ? "Authorize(Roles = \"Administrator\"), " : "AuthorizeRoles(Roles.Administrator), ") : string.Empty)}HttpDelete, Route(\"{CurrentEntity.RoutePath}\")]");
            s.Add($"        public async Task<IHttpActionResult> Delete({CurrentEntity.ControllerParameters})");
            s.Add($"        {{");
            s.Add($"            var {CurrentEntity.CamelCaseName} = await DbContext.{CurrentEntity.PluralName}.SingleOrDefaultAsync(o => {GetKeyFieldLinq("o")});");
            s.Add($"");
            s.Add($"            if ({CurrentEntity.CamelCaseName} == null)");
            s.Add($"                return NotFound();");
            s.Add($"");
            foreach (var relationship in CurrentEntity.RelationshipsAsParent.Where(rel => !rel.ChildEntity.Exclude).OrderBy(o => o.SortOrder))
            {
                if (relationship.CascadeDelete)
                {
                    s.Add($"            foreach (var {relationship.ChildEntity.CamelCaseName} in DbContext.{relationship.ChildEntity.PluralName}.Where(o => {relationship.RelationshipFields.Select(rf => "o." + rf.ChildField.Name + " == " + CurrentEntity.CamelCaseName + "." + rf.ParentField.Name).Aggregate((current, next) => current + " && " + next)}))");
                    s.Add($"                DbContext.Entry({relationship.ChildEntity.CamelCaseName}).State = EntityState.Deleted;");
                    s.Add($"");
                }
                else
                {
                    var joins = relationship.RelationshipFields.Select(o => $"o.{o.ChildField.Name} == {CurrentEntity.CamelCaseName}.{o.ParentField.Name}").Aggregate((current, next) => current + " && " + next);
                    s.Add($"            if (DbContext.{relationship.ChildEntity.PluralName}.Any(o => {joins}))");
                    s.Add($"                return BadRequest(\"Unable to delete the {CurrentEntity.FriendlyName.ToLower()} as it has related {relationship.ChildEntity.PluralFriendlyName.ToLower()}\");");
                    s.Add($"");
                }
            }
            // need to add fk checks here!
            s.Add($"            DbContext.Entry({CurrentEntity.CamelCaseName}).State = EntityState.Deleted;");
            s.Add($"");
            s.Add($"            await DbContext.SaveChangesAsync();");
            s.Add($"");
            s.Add($"            return Ok();");
            s.Add($"        }}");
            s.Add($"");
            #endregion

            #region sort
            if (CurrentEntity.HasASortField)
            {
                s.Add($"        [{(CurrentEntity.AuthorizationType == AuthorizationType.ProtectChanges ? (CurrentEntity.Project.UseStringAuthorizeAttributes ? "Authorize(Roles = \"Administrator\"), " : "AuthorizeRoles(Roles.Administrator), ") : "")}HttpPost, Route(\"sort\")]");
                s.Add($"        public async Task<IHttpActionResult> Sort([FromBody]SortedGuids sortedIds)");
                s.Add($"        {{");
                //s.Add($"            var {CurrentEntity.PluralName.ToCamelCase()} = await DbContext.{CurrentEntity.PluralName}.Where(o => sortedIds.ids.Contains(o.{CurrentEntity.KeyFields[0].Name})).ToListAsync();");
                s.Add($"            var {CurrentEntity.PluralName.ToCamelCase()} = await DbContext.{CurrentEntity.PluralName}.ToListAsync();");
                s.Add($"            if ({CurrentEntity.PluralName.ToCamelCase()}.Count != sortedIds.ids.Length) return BadRequest(\"Some of the {CurrentEntity.PluralFriendlyName.ToLower()} could not be found\");");
                s.Add($"");
                s.Add($"            var sortOrder = 0;");
                s.Add($"            foreach (var {CurrentEntity.Name.ToCamelCase()} in {CurrentEntity.PluralName.ToCamelCase()})");
                s.Add($"            {{");
                s.Add($"                DbContext.Entry({CurrentEntity.Name.ToCamelCase()}).State = EntityState.Modified;");
                s.Add($"                {CurrentEntity.Name.ToCamelCase()}.{CurrentEntity.SortField.Name} = Array.IndexOf(sortedIds.ids, {CurrentEntity.Name.ToCamelCase()}.{CurrentEntity.KeyFields[0].Name});");
                s.Add($"                sortOrder++;");
                s.Add($"            }}");
                s.Add($"");
                s.Add($"            await DbContext.SaveChangesAsync();");
                s.Add($"");
                s.Add($"            return Ok();");
                s.Add($"        }}");
                s.Add($"");
            }
            #endregion

            s.Add($"    }}");
            s.Add($"}}");

            return RunCodeReplacements(s.ToString(), CodeType.Controller);
        }

        public string GenerateBundleConfig()
        {
            var s = new StringBuilder();

            s.Add($"using System.Web.Optimization;");
            s.Add($"");
            s.Add($"namespace {CurrentEntity.Project.Namespace}");
            s.Add($"{{");
            s.Add($"    public partial class BundleConfig");
            s.Add($"    {{");
            s.Add($"        public static void AddGeneratedBundles(Bundle bundle)");
            s.Add($"        {{");
            s.Add($"            bundle.Include(");
            foreach (var e in NormalEntities)
            {
                s.Add($"                \"~/app/{e.PluralName.ToLower()}/{e.Name.ToLower()}.js\",");
                s.Add($"                \"~/app/{e.PluralName.ToLower()}/{e.PluralName.ToLower()}.js\"" + (e == NormalEntities.Last() ? string.Empty : ","));
            }
            s.Add($"                );");
            s.Add($"        }}");
            s.Add($"    }}");
            s.Add($"}}");

            return RunCodeReplacements(s.ToString(), CodeType.BundleConfig);

        }

        public string GenerateAppRouter()
        {
            var s = new StringBuilder();

            var noKeysEntity = NormalEntities.FirstOrDefault(e => e.KeyFields.Count == 0);
            if (noKeysEntity != null)
                throw new InvalidOperationException(noKeysEntity.FriendlyName + " has no keys defined");

            s.Add($"/// <reference path=\"../../scripts/typings/angularjs/angular.d.ts\" />");
            s.Add($"(function () {{");
            s.Add($"    \"use strict\";");
            s.Add($"");
            s.Add($"    angular.module('entityRoutes', []).config(entityRoutes);");
            s.Add($"");
            s.Add($"    entityRoutes.$inject = [\"$stateProvider\"];");
            s.Add($"    function entityRoutes($stateProvider) {{");
            s.Add($"");
            s.Add($"        var version = \"?v={string.Format("{0:yyyyMMddHHmmss}", DateTime.Now)}\";");
            s.Add($"");
            s.Add($"        $stateProvider");
            foreach (var e in NormalEntities)
            {
                s.Add($"            {(e == NormalEntities.First() ? string.Empty : "})")}.state(\"app.{e.Name.ToCamelCase()}\", {{");
                s.Add($"                url: \"{CurrentEntity.Project.UrlPrefix}{e.GetNavigationUrl()}\",");
                s.Add($"                templateUrl: \"/app/{e.PluralName.ToLower()}/{e.Name.ToLower()}.html\" + version,");
                s.Add($"                controller: \"{e.Name.ToCamelCase()}\",");
                s.Add($"                controllerAs: \"vm\",");
                //s.Add($"                data: {{");
                //s.Add($"                    roles: [\"Administrator\"]");
                //s.Add($"                }},");
                s.Add($"                ncyBreadcrumb: {{");
                s.Add($"                    parent: \"app.{(e.RelationshipsAsChild.Any(r => r.Hierarchy) ? e.RelationshipsAsChild.First(r => r.Hierarchy).ParentEntity.Name : e.PluralName).ToCamelCase()}\",");
                var defaultField = e.Fields.Where(f => f.ShowInSearchResults && !RelationshipFields.Any(rf => rf.ChildFieldId == f.FieldId)).OrderBy(f => f.FieldOrder).FirstOrDefault();
                if (defaultField == null && string.IsNullOrWhiteSpace(e.Breadcrumb))
                    s.Add($"                    label: \"{e.FriendlyName}\"");
                //throw new Exception(e.Name + " does not have a breadcrumb or default field (search result field that is not in a relationship as child)");
                else
                    s.Add($"                    label: \"{{{{{(!string.IsNullOrWhiteSpace(e.Breadcrumb) ? e.Breadcrumb : $"vm.{e.Name.ToCamelCase()}.{defaultField.Name.ToCamelCase()}")}}}}}\"");
                s.Add($"                }}");
                s.Add($"            }}).state(\"app.{e.PluralName.ToCamelCase()}\", {{");
                // todo: search fields
                s.Add($"                url: \"{CurrentEntity.Project.UrlPrefix}/{e.PluralName.ToLower()}\",");
                s.Add($"                templateUrl: \"/app/{e.PluralName.ToLower()}/{e.PluralName.ToLower()}.html\",");
                s.Add($"                controller: \"{e.PluralName.ToCamelCase()}\",");
                s.Add($"                controllerAs: \"vm\",");
                //s.Add($"                data: {{");
                //s.Add($"                    roles: [\"Administrator\"]");
                //s.Add($"                }},");
                s.Add($"                ncyBreadcrumb: {{");
                s.Add($"                    label: \"{e.PluralFriendlyName}\"");
                s.Add($"                }}");
                if (e == NormalEntities.Last()) s.Add($"            }});");
            }
            s.Add($"    }}");
            s.Add($"");
            s.Add($"}})();");


            return RunCodeReplacements(s.ToString(), CodeType.AppRouter);

        }

        public string GenerateApiResource()
        {
            var s = new StringBuilder();

            var noKeysEntity = NormalEntities.FirstOrDefault(e => e.KeyFields.Count == 0);
            if (noKeysEntity != null)
                throw new InvalidOperationException(noKeysEntity.FriendlyName + " has no keys defined");

            s.Add($"/// <reference path=\"../../scripts/typings/angularjs/angular.d.ts\" />");
            s.Add($"(function () {{");
            s.Add($"    \"use strict\";");
            s.Add($"");
            s.Add($"    angular");
            s.Add($"        .module(\"{CurrentEntity.Project.AngularModuleName}\")");
            foreach (var e in NormalEntities)
                s.Add($"        .factory(\"{e.ResourceName}\", {e.ResourceName})" + (e == NormalEntities.LastOrDefault() ? ";" : ""));
            s.Add($"");
            foreach (var e in NormalEntities)
            {
                s.Add($"    //#region {e.Name.ToLower()} resource");
                s.Add($"    {e.ResourceName}.$inject = [\"$resource\", \"appSettings\"];");
                s.Add($"    function {e.ResourceName}($resource, appSettings) {{");
                s.Add($"        return $resource(appSettings.apiServiceBaseUri + appSettings.apiPrefix + \"{e.PluralName.ToLower()}/{e.KeyFields.Select(o => ":" + o.Name.ToCamelCase()).Aggregate((current, next) => current + "/" + next) }\",");
                s.Add($"            {{");
                foreach (var field in e.KeyFields)
                    s.Add($"                {field.Name.ToCamelCase()}: \"@{field.Name.ToCamelCase()}\"{(field == e.KeyFields.Last() ? "" : ",")}");
                s.Add($"            }}" + (e.HasASortField || e.KeyFields.Count > 1 ? "," : string.Empty));
                if (e.HasCompositePrimaryKey)
                {
                    // composite primary keys can't use the .query method because: http://stackoverflow.com/questions/39405452/ngresource-query-with-composite-key-parameter/40087371#40087371
                    s.Add($"            {{");
                    s.Add($"                search: {{");
                    s.Add($"                    method: \"GET\",");
                    s.Add($"                    url: appSettings.apiServiceBaseUri + appSettings.apiPrefix + \"{e.PluralName.ToLower()}\",");
                    s.Add($"                    isArray: true");
                    s.Add($"                }}");
                    s.Add($"            }}" + (e.HasASortField ? "," : string.Empty));
                }
                if (e.HasASortField)
                {
                    s.Add($"            {{");
                    s.Add($"                sort: {{");
                    s.Add($"                    method: \"POST\",");
                    s.Add($"                    url: appSettings.apiServiceBaseUri + appSettings.apiPrefix + \"{e.PluralName.ToLower()}/sort\"");
                    s.Add($"                }}");
                    s.Add($"            }}");
                }
                s.Add($"        );");
                s.Add($"    }}");
                s.Add($"    //#endregion");
                s.Add($"");
            }
            s.Add($"}} ());");

            return RunCodeReplacements(s.ToString(), CodeType.ApiResource);

        }

        public string GenerateListHtml()
        {
            if (CurrentEntity.EntityType == EntityType.User) return string.Empty;

            var s = new StringBuilder();
            s.Add($"<div>");
            if (CurrentEntity.Fields.Any(f => f.SearchType != SearchType.None))
            {
                s.Add($"");
                s.Add($"    <form ng-submit=\"vm.runSearch(0)\" novalidate>");
                s.Add($"");
                s.Add($"        <div class=\"row\">");
                s.Add($"");
                if (CurrentEntity.Fields.Any(f => f.SearchType == SearchType.Text))
                {
                    s.Add($"            <div class=\"col-sm-6 col-md-4 col-lg-3\">");
                    s.Add($"                <div class=\"form-group\">");
                    s.Add($"                    <input type=\"search\" id=\"q\" ng-model=\"vm.search.q\" max=\"100\" class=\"form-control\" placeholder=\"Search {CurrentEntity.PluralFriendlyName.ToLower()}\" />");
                    s.Add($"                </div>");
                    s.Add($"            </div>");
                    s.Add($"");
                }
                foreach (var field in CurrentEntity.Fields.Where(f => f.SearchType == SearchType.Exact))
                {
                    if (field.CustomType == CustomType.Enum)
                    {
                        s.Add($"            <div class=\"col-sm-6 col-md-4 col-lg-3\">");
                        s.Add($"                <div class=\"form-group\">");
                        s.Add($"                    <ol id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" title=\"{field.Label}\" class=\"nya-bs-select form-control\" ng-model=\"vm.search.{field.Name.ToCamelCase()}\" data-size=\"10\" live-search=\"true\">");
                        s.Add($"                        <li nya-bs-option=\"item in vm.appSettings.{field.Lookup.Name.ToCamelCase()}\" class=\"nya-bs-option\" value=\"item.id\">");
                        s.Add($"                            <a>{{{{item.label}}}}<span class=\"fa fa-check check-mark\"></span></a>");
                        s.Add($"                        </li>");
                        s.Add($"                    </ol>");
                        s.Add($"                </div>");
                        s.Add($"            </div>");
                        s.Add($"");
                    }
                    else if (CurrentEntity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                    {
                        var relationship = CurrentEntity.GetParentSearchRelationship(field);
                        var parentEntity = relationship.ParentEntity;
                        var relField = relationship.RelationshipFields.Single();
                        s.Add($"            <div class=\"col-sm-6 col-md-4 col-lg-3\">");
                        s.Add($"                <div class=\"form-group\">");
                        s.Add($"                    <ol id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" title=\"{parentEntity.PluralFriendlyName}\" class=\"nya-bs-select form-control\" ng-model=\"vm.search.{field.Name.ToCamelCase()}\" data-size=\"10\" live-search=\"true\">");
                        s.Add($"                        <li nya-bs-option=\"{parentEntity.Name.ToCamelCase()} in vm.{parentEntity.PluralName.ToCamelCase()}\" class=\"nya-bs-option\" value=\"{parentEntity.Name.ToCamelCase()}.{relField.ParentField.Name.ToCamelCase()}\">");
                        s.Add($"                            <a>{{{{{parentEntity.Name.ToCamelCase()}.{relationship.ParentField.Name.ToCamelCase()}}}}}<span class=\"fa fa-check check-mark\"></span></a>");
                        s.Add($"                        </li>");
                        s.Add($"                    </ol>");
                        s.Add($"                </div>");
                        s.Add($"            </div>");
                        s.Add($"");

                    }
                }
                s.Add($"        </div>");
                s.Add($"");
                s.Add($"        <fieldset ng-disabled=\"vm.loading\">");
                s.Add($"");
                s.Add($"            <button type=\"submit\" class=\"btn btn-success\">Search<i class=\"fa fa-search{(CurrentEntity.Project.Bootstrap3 ? string.Empty : " ml-1")}\"></i></button>");
                if (CurrentEntity.RelationshipsAsChild.Count(r => r.Hierarchy) == 0)
                {
                    // todo: needs field list + field.newParameter
                    s.Add($"            <a href=\"{CurrentEntity.Project.UrlPrefix}/{CurrentEntity.PluralName.ToLower()}/{CurrentEntity.KeyFields.Select(f => $"{{{{vm.appSettings.{f.NewVariable}}}}}").Aggregate((current, next) => current + "/" + next)}\" class=\"btn btn-primary\">Add<i class=\"fa fa-plus-circle ml-1\"></i></a>");
                }
                s.Add($"");
                s.Add($"        </fieldset>");
                s.Add($"");
                s.Add($"    </form>");
            }
            s.Add($"");
            s.Add($"    <div ng-hide=\"vm.loading\">");
            s.Add($"");
            s.Add($"        <hr />");
            s.Add($"");
            // removed (not needed?): id=\"resultsList\" 
            s.Add($"        <table class=\"table table-striped table-hover table-bordered row-navigation table-sm\" ng-class=\"{{ 'disabled': vm.loading }}\">");
            s.Add($"            <thead>");
            s.Add($"                <tr>");
            foreach (var field in CurrentEntity.Fields.Where(f => f.ShowInSearchResults).OrderBy(f => f.FieldOrder))
                s.Add($"                    <th scope=\"col\">{field.Label}</th>");
            s.Add($"                </tr>");
            s.Add($"            </thead>");
            s.Add($"            <tbody{(CurrentEntity.HasASortField ? " ui-sortable=\"vm.sortOptions\" ng-model=\"vm." + CurrentEntity.PluralName.ToCamelCase() + "\"" : "")}>");
            s.Add($"                <tr ng-repeat=\"{CurrentEntity.CamelCaseName} in vm.{CurrentEntity.PluralName.ToCamelCase()}\" ng-click=\"vm.goTo{CurrentEntity.Name}({CurrentEntity.GetNavigationString()})\">");
            var firstCol = true;
            foreach (var field in CurrentEntity.Fields.Where(f => f.ShowInSearchResults).OrderBy(f => f.FieldOrder))
            {
                var handleStart = firstCol && CurrentEntity.HasASortField ? $"<i class=\"fa fa-sort sortable-handle mt-1\" ng-if=\"vm.{CurrentEntity.PluralName.ToCamelCase()}.length > 1\" ng-click=\"$event.stopPropagation();\"></i><div class=\"sortColumnText\">" : string.Empty;
                var handleEnd = firstCol && CurrentEntity.HasASortField ? $"</div>" : string.Empty;

                if (CurrentEntity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                {
                    var relationship = CurrentEntity.GetParentSearchRelationship(field);
                    s.Add($"                    <td>{handleStart}{{{{ { CurrentEntity.CamelCaseName}.{ relationship.ParentName.ToCamelCase()}.{relationship.ParentField.Name.ToCamelCase()} }}}}{handleEnd}</td>");
                }
                else
                {
                    if (field.CustomType == CustomType.Date)
                    {
                        if (field.IsNullable)
                            s.Add($"                    <td>{handleStart}{{{{ { CurrentEntity.CamelCaseName}.{ field.Name.ToCamelCase()} === null ? \"\" : vm.moment({ CurrentEntity.CamelCaseName}.{ field.Name.ToCamelCase()}).format('DD MMM YYYY{(field.FieldType == FieldType.Date ? string.Empty : " HH:mm" + (field.FieldType == FieldType.SmallDateTime ? "" : ":ss"))}') }}}}{handleEnd}</td>");
                        else
                            s.Add($"                    <td>{handleStart}{{{{ vm.moment({ CurrentEntity.CamelCaseName}.{ field.Name.ToCamelCase()}).format('DD MMM YYYY{(field.FieldType == FieldType.Date ? string.Empty : " HH:mm" + (field.FieldType == FieldType.SmallDateTime ? "" : ":ss"))}') }}}}{handleEnd}</td>");
                    }
                    else if (field.CustomType == CustomType.Enum)
                        s.Add($"                    <td>{handleStart}{{{{ vm.appSettings.find(vm.appSettings.{field.Lookup.Name.ToCamelCase()}, {CurrentEntity.CamelCaseName}.{field.Name.ToCamelCase()}).label }}}}{handleEnd}</td>");
                    else if (field.FieldType == FieldType.Date)
                        s.Add($"                    <td>{handleStart}{{{{ { field.Entity.Name.ToCamelCase()}.{ field.Name.ToCamelCase() } | toLocaleDateString }}}}{handleEnd}</td>");
                    else if (field.FieldType == FieldType.Bit)
                        s.Add($"                    <td>{handleStart}{{{{ { field.Entity.Name.ToCamelCase()}.{ field.Name.ToCamelCase() } ? \"Yes\" : \"No\" }}}}{handleEnd}</td>");
                    else
                        s.Add($"                    <td>{handleStart}{{{{ { CurrentEntity.CamelCaseName}.{ field.Name.ToCamelCase()} }}}}{handleEnd}</td>");
                }
                firstCol = false;
            }
            s.Add($"                </tr>");
            s.Add($"            </tbody>");
            s.Add($"        </table>");
            s.Add($"");
            // entities with sort fields need to show all (pageSize = 0) for sortability, so no paging needed
            if (!CurrentEntity.HasASortField)
            {
                s.Add($"        <div class=\"row\" ng-class=\"{{ 'disabled': vm.loading }}\">");
                s.Add($"            <div class=\"col-sm-7\">");
                s.Add($"                <{CurrentEntity.Project.AngularDirectivePrefix}-pager headers=\"vm.headers\" callback=\"vm.runSearch\"></{CurrentEntity.Project.AngularDirectivePrefix}-pager>");
                s.Add($"            </div>");
                s.Add($"            <div class=\"col-sm-5 text-right resultsInfo\">");
                s.Add($"                <{CurrentEntity.Project.AngularDirectivePrefix}-pager-info headers=\"vm.headers\"></{CurrentEntity.Project.AngularDirectivePrefix}-pager-info>");
                s.Add($"            </div>");
                s.Add($"        </div>");
                s.Add($"");
            }
            s.Add($"    </div>");
            s.Add($"");
            s.Add($"</div>");

            return RunCodeReplacements(s.ToString(), CodeType.ListHtml);
        }

        public string GenerateListTypeScript()
        {
            if (CurrentEntity.EntityType == EntityType.User) return string.Empty;

            var s = new StringBuilder();

            //s.Add($"/// <reference path=\"../../scripts/typings/jquery.datatables/jquery.datatables.d.ts\" />");
            s.Add($"/// <reference path=\"../../scripts/typings/angularjs/angular.d.ts\" />");
            s.Add($"(function () {{");
            s.Add($"    \"use strict\";");
            s.Add($"");
            s.Add($"    angular");
            s.Add($"        .module(\"{CurrentEntity.Project.AngularModuleName}\")");
            s.Add($"        .controller(\"{CurrentEntity.PluralName.ToCamelCase()}\", {CurrentEntity.PluralName.ToCamelCase()});");
            s.Add($"");
            var lookupEntities = new Dictionary<string, string>();
            foreach (var field in CurrentEntity.Fields.Where(f => f.SearchType == SearchType.Exact))
            {
                if (CurrentEntity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                {
                    var relationship = CurrentEntity.GetParentSearchRelationship(field);
                    var parentEntity = relationship.ParentEntity;
                    if (!lookupEntities.ContainsKey(parentEntity.ResourceName))
                        lookupEntities.Add(parentEntity.ResourceName, parentEntity.TypeScriptResource);
                }
            }
            s.Add($"    {CurrentEntity.PluralName.ToCamelCase()}.$inject = [\"$scope\", \"$state\", \"$q\", \"$timeout\", \"notifications\", \"appSettings\", \"{CurrentEntity.ResourceName}\"{(lookupEntities.Count() == 0 ? string.Empty : lookupEntities.Select(e => ", \"" + e.Key + "\"").Aggregate((current, next) => current + next))}];");
            s.Add($"    function {CurrentEntity.PluralName.ToCamelCase()}($scope, $state, $q, $timeout, notifications, appSettings, {CurrentEntity.ResourceName + (CurrentEntity.Project.ExcludeTypes ? string.Empty : ": " + CurrentEntity.TypeScriptResource)}{(lookupEntities.Count() == 0 ? string.Empty : lookupEntities.Select(e => ", " + e.Key + (CurrentEntity.Project.ExcludeTypes ? string.Empty : ": " + e.Value)).Aggregate((current, next) => current + next))}) {{");
            s.Add($"");
            s.Add($"        var vm = this;");
            s.Add($"        vm.loading = true;");
            s.Add($"        vm.appSettings = appSettings;");
            s.Add($"        vm.search = {{ }};");
            s.Add($"        vm.runSearch = runSearch;");
            s.Add($"        {CurrentEntity.GetGoToEntityCode()}");
            if (CurrentEntity.HasASortField)
            {
                s.Add($"        vm.sortOptions = {{ stop: sortItems, handle: \"i.sortable-handle\", axis: \"y\" }};");
            }
            s.Add($"        vm.moment = moment;");
            s.Add($"");
            s.Add($"        initPage();");
            s.Add($"");
            s.Add($"        function initPage() {{");
            s.Add($"");
            s.Add($"            var promises = [];");
            s.Add($"");
            var processedEntities = new List<Guid>();
            foreach (var field in CurrentEntity.Fields.Where(f => f.SearchType == SearchType.Exact))
            {
                if (CurrentEntity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                {
                    var relationship = CurrentEntity.GetParentSearchRelationship(field);
                    var parentEntity = relationship.ParentEntity;
                    var relField = relationship.RelationshipFields.Single();
                    if (processedEntities.Contains(parentEntity.EntityId)) continue;
                    processedEntities.Add(parentEntity.EntityId);

                    s.Add($"            promises.push(");
                    s.Add($"                {parentEntity.ResourceName}.query(");
                    s.Add($"                    {{");
                    s.Add($"                        pageSize: 0");
                    s.Add($"                    }},");
                    s.Add($"                    (data, headers) => {{");
                    s.Add($"");
                    s.Add($"                        vm.{parentEntity.PluralName.ToCamelCase()} = data;");
                    s.Add($"");
                    s.Add($"                    }},");
                    s.Add($"                    err => {{");
                    s.Add($"");
                    s.Add($"                        notifications.error(\"Failed to load the {parentEntity.PluralFriendlyName.ToLower()}.\", \"Error\", err);");
                    s.Add($"                        $state.go(\"app.home\");");
                    s.Add($"");
                    s.Add($"                    }}).$promise");
                    s.Add($"            );");
                    s.Add($"");

                }
            }
            s.Add($"            $q.all(promises).finally(() => runSearch(0));");
            s.Add($"");
            s.Add($"        }}");
            s.Add($"");
            s.Add($"        function runSearch(pageIndex) {{");
            s.Add($"");
            s.Add($"            vm.loading = true;");
            s.Add($"");
            s.Add($"            var promises = [];");
            s.Add($"");
            s.Add($"            promises.push(");
            // composite primary keys can't use the .query method because: http://stackoverflow.com/questions/39405452/ngresource-query-with-composite-key-parameter/40087371#40087371
            s.Add($"                {CurrentEntity.ResourceName}.{(!CurrentEntity.HasCompositePrimaryKey ? "query" : "search")}(");
            s.Add($"                    {{");
            foreach (var field in CurrentEntity.Fields.Where(f => f.SearchType == SearchType.Exact))
            {
                s.Add($"                        {field.Name.ToCamelCase()}: vm.search.{field.Name.ToCamelCase()},");
            }
            if (CurrentEntity.Fields.Any(f => f.SearchType == SearchType.Text))
                s.Add($"                        q: vm.search.q,"); // todo: what would happen if searching and the entity is sortable? should disable sorting? 
            // instruct api to load related entities as this entity has parent entities in results grid
            foreach (var field in CurrentEntity.Fields.Where(f => f.ShowInSearchResults).OrderBy(f => f.FieldOrder))
                if (CurrentEntity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                {
                    s.Add($"                        includeEntities: true,");
                    break;
                }
            if (CurrentEntity.HasASortField)
                s.Add($"                        pageSize: 0");
            else
                s.Add($"                        pageIndex: pageIndex");
            s.Add($"                    }},");
            s.Add($"                    (data, headers) => {{");
            s.Add($"");
            s.Add($"                        vm.{CurrentEntity.PluralName.ToCamelCase()} = data;");
            if (!CurrentEntity.HasASortField)
                s.Add($"                        vm.headers = JSON.parse(headers(\"X-Pagination\"))");
            s.Add($"");
            s.Add($"                    }},");
            s.Add($"                    err => {{");
            s.Add($"");
            s.Add($"                        notifications.error(\"Failed to load the {CurrentEntity.PluralFriendlyName.ToLower()}.\", \"Error\", err);");
            s.Add($"                        $state.go(\"app.home\");");
            s.Add($"");
            s.Add($"                    }}).$promise");
            s.Add($"            );");
            s.Add($"");
            s.Add($"            $q.all(promises).finally(() => vm.loading = false);");
            s.Add($"");
            s.Add($"        }};");
            s.Add($"");
            if (CurrentEntity.HasASortField)
            {
                s.Add($"        function sortItems() {{");
                s.Add($"");
                s.Add($"            vm.loading = true;");
                s.Add($"");
                s.Add($"            var ids = [];");
                s.Add($"            angular.forEach(vm.{CurrentEntity.PluralName.ToCamelCase()}, function (item, index) {{");
                s.Add($"                ids.push(item.{CurrentEntity.KeyFields[0].Name.ToCamelCase()});");
                s.Add($"            }});");
                s.Add($"");
                s.Add($"            {CurrentEntity.ResourceName}.sort(");
                s.Add($"                {{");
                s.Add($"                    ids: ids");
                s.Add($"                }},");
                s.Add($"                data => {{");
                s.Add($"");
                s.Add($"                    notifications.success(\"The sort order has been updated\", \"{CurrentEntity.FriendlyName} Ordering\");");
                s.Add($"");
                s.Add($"                }},");
                s.Add($"                err => {{");
                s.Add($"");
                s.Add($"                    notifications.error(\"Failed to sort the {CurrentEntity.PluralFriendlyName.ToLower()}. \" + (err.data && err.data.message ? err.data.message : \"\"), \"Error\", err);");
                s.Add($"");
                s.Add($"                }})");
                s.Add($"                .$promise.finally(() => vm.loading = false);");
                s.Add($"");
                s.Add($"        }}");
                s.Add($"");
            }
            s.Add($"    }};");
            s.Add($"}} ());");

            return RunCodeReplacements(s.ToString(), CodeType.ListTypeScript);
        }

        public string GenerateEditHtml()
        {
            //if (CurrentEntity.EntityType == EntityType.User) return string.Empty;

            var s = new StringBuilder();

            s.Add($"<div>");
            s.Add($"");
            s.Add($"    <form name=\"mainForm\" ng-submit=\"vm.save()\" novalidate>");
            s.Add($"");
            s.Add($"        <fieldset class=\"group\" ng-disabled=\"vm.loading\">");
            s.Add($"");
            s.Add($"            <legend>{CurrentEntity.FriendlyName}</legend>");
            s.Add($"");
            s.Add($"            <div class=\"row\">");
            s.Add($"");
            var t = string.Empty;
            // not really a bootstrap3 issue - old projects will be affected by this now being commented
            //if (CurrentEntity.Project.Bootstrap3)
            //{
            //    s.Add($"                <div class=\"col-sm-12\">");
            //    s.Add($"");
            //    t = "    ";
            //}
            #region form fields
            if (CurrentEntity.EntityType == EntityType.User)
            {
                s.Add(t + $"                <div class=\"col-sm-6 col-md-4\">");
                s.Add(t + $"                    <div class=\"form-group\" ng-class=\"{{ 'has-error':  mainForm.$submitted && mainForm.email.$invalid }}\">");
                s.Add(t + $"                        <label for=\"email\" class=\"control-label\">");
                s.Add(t + $"                            Email:");
                s.Add(t + $"                        </label>");
                s.Add(t + $"                        <input type=\"email\" id=\"email\" name=\"email\" ng-model=\"{CurrentEntity.ViewModelObject}.email\" maxlength=\"256\" class=\"form-control\" ng-required=\"true\" />");
                s.Add(t + $"                    </div>");
                s.Add(t + $"                </div>");
                s.Add($"");
            }

            foreach (var field in CurrentEntity.Fields.OrderBy(o => o.FieldOrder))
            {
                if (field.KeyField && field.CustomType != CustomType.String && !CurrentEntity.HasCompositePrimaryKey) continue;
                if (field.EditPageType == EditPageType.Exclude) continue;
                if (field.EditPageType == EditPageType.SortField) continue;
                if (field.EditPageType == EditPageType.CalculatedField) continue;

                if (field.EditPageType == EditPageType.ReadOnly)
                {
                    s.Add(t + $"                <div class=\"col-sm-6 col-md-4\">");
                    s.Add(t + $"                    <div class=\"form-group\">");
                    s.Add(t + $"                        <label for=\"{field.Name.ToCamelCase()}\" class=\"control-label\">");
                    s.Add(t + $"                            {field.Label}:");
                    s.Add(t + $"                        </label>");
                    if (field.CustomType == CustomType.Date)
                    {
                        s.Add(t + $"                        <input type=\"text\" id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" class=\"form-control\" ng-disabled=\"true\" value=\"{{{{{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()} ? vm.moment({CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}).format('DD MMMM YYYY{(field.FieldType == FieldType.Date ? string.Empty : " HH:mm" + (field.FieldType == FieldType.SmallDateTime ? "" : ":ss"))}') : ''}}}}\" />");
                    }
                    else if (field.CustomType == CustomType.Boolean)
                    {
                        s.Add(t + $"                        <input type=\"text\" id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" class=\"form-control\" ng-disabled=\"true\" value=\"{{{{{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()} ? 'Yes' : 'No'}}}}\" />");
                    }
                    else if (CurrentEntity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                    {
                        var relationship = CurrentEntity.GetParentSearchRelationship(field);
                        s.Add(t + $"                        <input type=\"text\" id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" class=\"form-control\" ng-disabled=\"true\" value=\"{{{{{CurrentEntity.ViewModelObject}.{relationship.ParentName.ToCamelCase()}.{relationship.ParentField.Name.ToCamelCase()}}}}}\" />");
                    }
                    else
                    {
                        s.Add(t + $"                        <input type=\"text\" id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" class=\"form-control\" ng-disabled=\"true\" value=\"{{{{{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}}}}}\" />");
                    }
                    s.Add(t + $"                    </div>");
                    s.Add(t + $"                </div>");
                    s.Add($"");
                }
                else if (CurrentEntity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                {
                    var relationship = CurrentEntity.GetParentSearchRelationship(field);
                    var relationshipField = relationship.RelationshipFields.Single(f => f.ChildFieldId == field.FieldId);
                    if (relationship.Hierarchy) continue;

                    s.Add(t + $"                <div class=\"col-sm-6 col-md-4\">");
                    s.Add(t + $"                    <div class=\"form-group\" ng-class=\"{{ 'has-error':  mainForm.$submitted && mainForm.{field.Name.ToCamelCase()}.$invalid}}\">");
                    s.Add(t + $"                        <label class=\"control-label\">");
                    s.Add(t + $"                            {field.Label}:");
                    s.Add(t + $"                        </label>");
                    s.Add(t + $"                        <ol id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" class=\"nya-bs-select form-control\" ng-model=\"{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}\"{(field.IsNullable ? string.Empty : " ng-required=\"true\"")} data-live-search=\"true\"{(field.KeyField ? " disabled=\"!vm.isNew\"" : string.Empty)}>");
                    s.Add(t + $"                            <li nya-bs-option=\"{relationship.ParentEntity.Name.ToCamelCase()} in vm.{relationship.ParentEntity.PluralName.ToCamelCase()}\" class=\"nya-bs-option\" value=\"{relationship.ParentEntity.Name.ToCamelCase()}.{relationshipField.ParentField.Name.ToCamelCase()}\">");
                    s.Add(t + $"                                <a>{{{{{relationship.ParentEntity.Name.ToCamelCase()}.{relationship.ParentField.Name.ToCamelCase()}}}}}<span class=\"fa fa-check check-mark\"></span></a>");
                    s.Add(t + $"                            </li>");
                    s.Add(t + $"                        </ol>");
                    s.Add(t + $"                    </div>");
                    s.Add(t + $"                </div>");
                    s.Add($"");
                }
                else if (field.CustomType == CustomType.Enum)
                {
                    s.Add(t + $"                <div class=\"col-sm-6 col-md-4\">");
                    s.Add(t + $"                    <div class=\"form-group\" ng-class=\"{{ 'has-error':  mainForm.$submitted && mainForm.{field.Name.ToCamelCase()}.$invalid }}\">");
                    s.Add(t + $"                        <label for=\"{field.Name.ToCamelCase()}\" class=\"control-label\">");
                    s.Add(t + $"                            {field.Label}:");
                    s.Add(t + $"                        </label>");
                    s.Add(t + $"                        <ol id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" class=\"nya-bs-select form-control\" ng-model=\"{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}\"{(field.IsNullable ? string.Empty : " ng-required=\"true\"")} data-live-search=\"true\">");
                    // todo: replace with lookups
                    s.Add(t + $"                            <li nya-bs-option=\"{field.Name.ToCamelCase()} in vm.appSettings.{field.Lookup.Name.ToCamelCase()}\" class=\"nya-bs-option\" value=\"{field.Name.ToCamelCase()}.id\">");
                    s.Add(t + $"                                <a>{{{{{field.Name.ToCamelCase()}.label}}}}<span class=\"fa fa-check check-mark\"></span></a>");
                    s.Add(t + $"                            </li>");
                    s.Add(t + $"                        </ol>");
                    s.Add(t + $"                    </div>");
                    s.Add(t + $"                </div>");
                    s.Add($"");
                }
                else if (field.CustomType == CustomType.String)
                {
                    s.Add(t + $"                <div class=\"col-sm-6 col-md-4\">");
                    s.Add(t + $"                    <div class=\"form-group\" ng-class=\"{{ 'has-error':  mainForm.$submitted && mainForm.{field.Name.ToCamelCase()}.$invalid }}\">");
                    s.Add(t + $"                        <label for=\"{field.Name.ToCamelCase()}\" class=\"control-label\">");
                    s.Add(t + $"                            {field.Label}:");
                    s.Add(t + $"                        </label>");
                    if (field.FieldType == FieldType.Text || field.FieldType == FieldType.nText || field.Length == 0)
                    {
                        s.Add(t + $"                        <textarea id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" ng-model=\"{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}\" class=\"form-control\"{(field.IsNullable ? string.Empty : " ng-required=\"true\"")} rows=\"6\"" + (field.Length == 0 ? string.Empty : $" maxlength=\"{field.Length}\"") + (field.MinLength > 0 ? " ng-minlength=\"" + field.MinLength + "\"" : "") + $"></textarea>");
                    }
                    else
                    {
                        s.Add(t + $"                        <input type=\"text\" id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" ng-model=\"{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}\"" + (field.Length == 0 ? string.Empty : $" maxlength=\"{field.Length}\"") + $" class=\"form-control\"{(field.IsNullable ? string.Empty : " ng-required=\"true\"")} />");
                    }
                    s.Add(t + $"                    </div>");
                    s.Add(t + $"                </div>");
                    s.Add($"");
                }
                else if (field.CustomType == CustomType.Number)
                {
                    s.Add(t + $"                <div class=\"col-sm-4 col-md-3 col-lg-2\">");
                    s.Add(t + $"                    <div class=\"form-group\" ng-class=\"{{ 'has-error':  mainForm.$submitted && mainForm.{field.Name.ToCamelCase()}.$invalid }}\">");
                    s.Add(t + $"                        <label for=\"{field.Name.ToCamelCase()}\" class=\"control-label\">");
                    s.Add(t + $"                            {field.Label}:");
                    s.Add(t + $"                        </label>");
                    s.Add(t + $"                        <input type=\"number\" id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" ng-model=\"{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}\" class=\"form-control\"{(field.IsNullable ? string.Empty : " ng-required=\"true\"")} />");
                    s.Add(t + $"                    </div>");
                    s.Add(t + $"                </div>");
                    s.Add($"");
                }
                else if (field.CustomType == CustomType.Boolean)
                {
                    s.Add(t + $"                <div class=\"col-sm-4 col-md-3 col-lg-2\">");
                    s.Add(t + $"                    <div class=\"form-group\" ng-class=\"{{ 'has-error':  mainForm.$submitted && mainForm.{field.Name.ToCamelCase()}.$invalid }}\">");
                    s.Add(t + $"                        <label for=\"{field.Name.ToCamelCase()}\" class=\"control-label\">");
                    s.Add(t + $"                            {field.Label}:");
                    s.Add(t + $"                        </label>");
                    s.Add(t + $"                        <div class=\"checkbox\">");
                    s.Add(t + $"                            <label>");
                    s.Add(t + $"                                <input type=\"checkbox\" id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" ng-model=\"{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}\" />");
                    s.Add(t + $"                                {field.Label}");
                    s.Add(t + $"                            </label>");
                    s.Add(t + $"                        </div>");
                    s.Add(t + $"                    </div>");
                    s.Add(t + $"                </div>");
                    s.Add($"");
                }
                else if (field.CustomType == CustomType.Date)
                {
                    s.Add(t + $"                <div class=\"col-sm-6 col-md-4\">");
                    s.Add(t + $"                    <div class=\"form-group\" ng-class=\"{{ 'has-error':  mainForm.$submitted && mainForm.{field.Name.ToCamelCase()}.$invalid }}\">");
                    s.Add(t + $"                        <label for=\"{field.Name.ToCamelCase()}\" class=\"control-label\">");
                    s.Add(t + $"                            {field.Label}:");
                    s.Add(t + $"                        </label>");
                    s.Add(t + $"                        <input type=\"{(field.FieldType == FieldType.Date ? "date" : "datetime-local")}\" id=\"{field.Name.ToCamelCase()}\" name=\"{field.Name.ToCamelCase()}\" ng-model=\"{CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}\" {(field.FieldType == FieldType.Date ? "ng-model-options=\"{timezone: 'utc'}\" " : "")} class=\"form-control\"{(field.IsNullable ? string.Empty : " ng-required=\"true\"")} />");
                    s.Add(t + $"                    </div>");
                    s.Add(t + $"                </div>");
                    s.Add($"");
                }
                else
                {
                    s.Add(t + $"NOT IMPLEMENTED: CustomType " + field.CustomType.ToString());
                    s.Add($"");
                    //throw new NotImplementedException("GenerateEditHtml: NetType: " + field.NetType.ToString());
                }
            }
            #endregion

            if (CurrentEntity.EntityType == EntityType.User)
            {
                s.Add(t + $"                <div class=\"col-sm-12 col-lg-6\">");
                s.Add(t + $"                    <div class=\"form-group\">");
                s.Add(t + $"                        <label class=\"control-label\">");
                s.Add(t + $"                            Roles:");
                s.Add(t + $"                        </label>");
                s.Add(t + $"                        <ol id=\"roles\" name=\"roles\" class=\"nya-bs-select form-control\" ng-model=\"vm.user.roleIds\" multiple>");
                s.Add(t + $"                            <li nya-bs-option=\"role in vm.appSettings.roles\" class=\"nya-bs-option\" value=\"role.id\">");
                s.Add(t + $"                                <a>{{{{role.label}}}}<span class=\"fa fa-check check-mark\"></span></a>");
                s.Add(t + $"                            </li>");
                s.Add(t + $"                        </ol>");
                s.Add(t + $"                    </div>");
                s.Add(t + $"                </div>");
                s.Add(t + $"");
            }

            // not really a bootstrap3 issue - old projects will be affected by this now being commented
            //if (CurrentEntity.Project.Bootstrap3)
            //    s.Add(t + $"            </div>");
            s.Add($"            </div>");
            s.Add($"");
            s.Add($"        </fieldset>");
            s.Add($"");
            s.Add($"        <div class=\"form-group error-messages has-error alert alert-danger\" ng-if=\"mainForm.$submitted && mainForm.$invalid\">");
            s.Add($"");
            s.Add($"            <span class=\"help-block has-error\"");
            s.Add($"                  ng-if=\"mainForm.$submitted\">");
            s.Add($"                <span>");
            s.Add($"                    Please correct the following errors:");
            s.Add($"                </span>");
            s.Add($"            </span>");
            s.Add($"");
            s.Add($"            <ul>");
            s.Add($"");
            #region form validation
            if (CurrentEntity.EntityType == EntityType.User)
            {
                s.Add($"                <li class=\"help-block has-error\"");
                s.Add($"                    ng-if=\"mainForm.$submitted\"");
                s.Add($"                    ng-messages=\"mainForm.email.$error\">");
                s.Add($"                    <span ng-message=\"required\">");
                s.Add($"                        Email address is required.");
                s.Add($"                    </span>");
                s.Add($"                    <span ng-message=\"minlength\">");
                s.Add($"                        Email address is too short.");
                s.Add($"                    </span>");
                s.Add($"                    <span ng-message=\"email\">");
                s.Add($"                        Email address is not valid.");
                s.Add($"                    </span>");
                s.Add($"                </li>");
                s.Add($"");
            }
            foreach (var field in CurrentEntity.Fields.OrderBy(o => o.FieldOrder))
            {
                if (field.EditPageType == EditPageType.ReadOnly || field.EditPageType == EditPageType.Exclude || field.EditPageType == EditPageType.SortField) continue;
                if (field.KeyField && field.CustomType != CustomType.String && !CurrentEntity.HasCompositePrimaryKey) continue;

                if (CurrentEntity.RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                {
                    if (field.IsNullable) continue;
                    var relationship = CurrentEntity.GetParentSearchRelationship(field);
                    if (relationship.Hierarchy) continue;

                    s.Add($"                <li class=\"help-block has-error\"");
                    s.Add($"                    ng-if=\"mainForm.$submitted\"");
                    s.Add($"                    ng-messages=\"mainForm.{field.Name.ToCamelCase()}.$error\">");
                    s.Add($"                    <span ng-message=\"required\">");
                    s.Add($"                        {field.Label} is required.");
                    s.Add($"                    </span>");
                    s.Add($"                </li>");
                    s.Add($"");
                }
                else if (field.CustomType == CustomType.Enum || field.CustomType == CustomType.String || field.CustomType == CustomType.Number)
                {
                    if (field.IsNullable && field.CustomType != CustomType.Number) continue;
                    s.Add($"                <li class=\"help-block has-error\"");
                    s.Add($"                    ng-if=\"mainForm.$submitted\"");
                    s.Add($"                    ng-messages=\"mainForm.{field.Name.ToCamelCase()}.$error\">");
                    if (!field.IsNullable)
                    {
                        s.Add($"                    <span ng-message=\"required\">");
                        s.Add($"                        {field.Label} is required.");
                        s.Add($"                    </span>");
                    }
                    if (field.CustomType == CustomType.Number)
                    {
                        s.Add($"                    <span ng-message=\"number\">");
                        s.Add($"                        {field.Label} is not a valid number.");
                        s.Add($"                    </span>");
                    }
                    if (field.MinLength > 0)
                    {
                        s.Add($"                    <span ng-message=\"minlength\">");
                        s.Add($"                        {field.Label} is too short.");
                        s.Add($"                    </span>");
                    }
                    s.Add($"                </li>");
                    s.Add($"");
                }
                else if (field.CustomType == CustomType.Date)
                {
                    s.Add($"                <li class=\"help-block has-error\"");
                    s.Add($"                    ng-if=\"mainForm.$submitted\"");
                    s.Add($"                    ng-messages=\"mainForm.{field.Name.ToCamelCase()}.$error\">");
                    s.Add($"                    <span ng-message=\"date\">");
                    s.Add($"                        {field.Label} is not a valid date.");
                    s.Add($"                    </span>");
                    if (!field.IsNullable)
                    {
                        s.Add($"                    <span ng-message=\"required\">");
                        s.Add($"                        {field.Label} is required.");
                        s.Add($"                    </span>");
                    }
                    s.Add($"                </li>");
                    s.Add($"");
                }
                if (field.IsNullable) continue;

            }
            #endregion
            s.Add($"            </ul>");
            s.Add($"");
            s.Add($"        </div>");
            s.Add($"");
            s.Add($"        <fieldset ng-disabled=\"vm.loading\">");
            s.Add($"            <button type=\"submit\" class=\"btn btn-success\">Save<i class=\"fa fa-check{(CurrentEntity.Project.Bootstrap3 ? string.Empty : " ml-1")}\"></i></button>");
            s.Add($"            <button type=\"button\" ng-if=\"!vm.isNew\" class=\"btn {(CurrentEntity.Project.Bootstrap3 ? "btn-danger btn-delete" : "btn-outline-danger")}\" ng-click=\"vm.delete()\">Delete<i class=\"fa fa-times{(CurrentEntity.Project.Bootstrap3 ? string.Empty : " ml-1")}\"></i></button>");
            s.Add($"        </fieldset>");
            s.Add($"");
            s.Add($"    </form>");
            s.Add($"");

            #region child lists
            if (CurrentEntity.RelationshipsAsParent.Any(r => r.DisplayListOnParent))
            {
                s.Add($"    <div ng-show=\"!vm.isNew\">");
                s.Add($"");
                foreach (var relationship in CurrentEntity.RelationshipsAsParent.Where(r => r.DisplayListOnParent).OrderBy(r => r.SortOrder))
                {
                    var childEntity = relationship.ChildEntity;
                    s.Add($"        <hr />");
                    s.Add($"");
                    s.Add($"        <h2>{relationship.CollectionFriendlyName}</h2>");
                    s.Add($"");
                    s.Add($"        <fieldset ng-disabled=\"vm.loading\">");
                    var href = "/";
                    foreach (var entity in childEntity.GetNavigationEntities())
                    {
                        href += (href == "/" ? string.Empty : "/") + entity.PluralName.ToLower();
                        foreach (var field in childEntity.GetNavigationFields().Where(f => f.EntityId == entity.EntityId))
                        {
                            if (entity == childEntity)
                                href += "/{{vm.appSettings." + field.NewVariable + "}}";
                            else
                                href += "/{{vm." + entity.Name.ToCamelCase() + "." + field.Name.ToCamelCase() + "}}";
                        }
                    }
                    s.Add($"            <a class=\"btn btn-primary\" href=\"{href}\">Add {childEntity.FriendlyName}<i class=\"fa fa-plus-circle ml-1\"></i></a><br />");
                    s.Add($"            <br />");
                    s.Add($"        </fieldset>");
                    s.Add($"");
                    s.Add($"        <table class=\"table table-striped table-hover table-bordered row-navigation table-sm\" ng-class=\"{{ 'disabled': vm.loading }}\">");
                    s.Add($"            <thead>");
                    s.Add($"                <tr>");
                    foreach (var column in childEntity.GetSearchResultsFields(CurrentEntity))
                    {
                        s.Add($"                    <th scope=\"col\">{column.Header}</th>");
                    }
                    s.Add($"                </tr>");
                    s.Add($"            </thead>");
                    s.Add($"            <tbody" + (childEntity.HasASortField ? $" ui-sortable=\"vm.{childEntity.PluralName.ToCamelCase()}SortOptions\" ng-model=\"vm.{childEntity.PluralName.ToCamelCase()}\"" : string.Empty) + ">");
                    s.Add($"                <tr ng-repeat=\"{childEntity.Name.ToCamelCase()} in vm.{relationship.CollectionName.ToCamelCase()}\" ng-click=\"vm.goTo{childEntity.Name}({childEntity.GetNavigationString()})\">");
                    var firstCol = true;
                    foreach (var column in childEntity.GetSearchResultsFields(CurrentEntity))
                    {
                        s.Add($"                    <td>{(firstCol && childEntity.HasASortField ? $"<i class=\"fa fa-sort sortable-handle mt-1\" ng-if=\"vm.{relationship.CollectionName.ToCamelCase()}.length > 1\" ng-click=\"$event.stopPropagation();\"></i>" : string.Empty)}{(firstCol && childEntity.HasASortField ? "<div class=\"sortColumnText\">" + column.Value + "</div>" : column.Value)}</td>");
                        firstCol = false;
                    }
                    s.Add($"                </tr>");
                    s.Add($"            </tbody>");
                    s.Add($"        </table>");
                    s.Add($"");
                    // entities with sort fields need to show all (pageSize = 0) for sortability, so no paging needed
                    if (!childEntity.HasASortField)
                    {
                        s.Add($"        <div class=\"row\" ng-class=\"{{ 'disabled': vm.loading }}\">");
                        s.Add($"            <div class=\"col-sm-7\">");
                        s.Add($"               <{CurrentEntity.Project.AngularDirectivePrefix}-pager headers=\"vm.{relationship.ChildEntity.PluralName.ToCamelCase()}Headers\" callback=\"vm.load{relationship.ChildEntity.PluralName}\"></{CurrentEntity.Project.AngularDirectivePrefix}-pager>");
                        s.Add($"            </div>");
                        s.Add($"            <div class=\"col-sm-5 text-right resultsInfo\">");
                        s.Add($"               <{CurrentEntity.Project.AngularDirectivePrefix}-pager-info headers=\"vm.{relationship.ChildEntity.PluralName.ToCamelCase()}Headers\"></{CurrentEntity.Project.AngularDirectivePrefix}-pager-info>");
                        s.Add($"            </div>");
                        s.Add($"        </div>");
                        s.Add($"");
                    }
                }
                s.Add($"    </div>");
                s.Add($"");
            }
            #endregion

            s.Add($"</div>");

            return RunCodeReplacements(s.ToString(), CodeType.EditHtml);
        }

        public string GenerateEditTypeScript()
        {
            //if (CurrentEntity.EntityType == EntityType.User) return string.Empty;

            var s = new StringBuilder();

            s.Add($"/// <reference path=\"../../scripts/typings/angularjs/angular.d.ts\" />");
            s.Add($"(function () {{");
            s.Add($"    \"use strict\";");
            s.Add($"");
            s.Add($"    angular");
            s.Add($"        .module(\"{CurrentEntity.Project.AngularModuleName}\")");
            s.Add($"        .controller(\"{CurrentEntity.CamelCaseName}\", {CurrentEntity.CamelCaseName});");
            s.Add($"");
            s.Add($"    {CurrentEntity.CamelCaseName}.$inject = [\"$scope\", \"$state\", \"$stateParams\", \"{CurrentEntity.ResourceName}\", \"notifications\", \"appSettings\", \"$q\", \"errorService\"{CurrentEntity.RequiredRelationshipEntities.Select(e => ", \"" + e.ResourceName + "\"").DefaultIfEmpty(string.Empty).Aggregate((current, next) => current + next)}];");
            s.Add($"    function {CurrentEntity.CamelCaseName}($scope, $state, $stateParams, {CurrentEntity.ResourceName + (CurrentEntity.Project.ExcludeTypes ? string.Empty : ": " + CurrentEntity.TypeScriptResource)}, notifications, appSettings, $q, errorService{CurrentEntity.RequiredRelationshipEntities.Select(e => ", " + e.ResourceName + (CurrentEntity.Project.ExcludeTypes ? string.Empty : ": " + e.TypeScriptResource)).DefaultIfEmpty(string.Empty).Aggregate((current, next) => current + next)}) {{");
            s.Add($"");
            s.Add($"        var vm = this;");
            if (!CurrentEntity.Project.ExcludeTypes)
            {
                s.Add($"        var {CurrentEntity.CamelCaseName}: {CurrentEntity.TypeScriptDTO} = new {CurrentEntity.ResourceName}();");
                s.Add($"        vm.{CurrentEntity.CamelCaseName} = {CurrentEntity.CamelCaseName};");
            }
            s.Add($"        vm.loading = true;");
            s.Add($"        vm.appSettings = appSettings;");
            if (CurrentEntity.Fields.Any(f => f.CustomType == CustomType.Date))
                s.Add($"        vm.moment = moment;");
            s.Add($"        vm.user = null;");
            s.Add($"        vm.save = save;");
            s.Add($"        vm.delete = del;");
            s.Add($"        vm.isNew = {CurrentEntity.KeyFields.Select(f => "$stateParams." + f.Name.ToCamelCase() + " === vm.appSettings." + f.NewVariable).Aggregate((current, next) => current + " && " + next)};");
            foreach (var rel in CurrentEntity.RelationshipsAsParent.Where(r => r.DisplayListOnParent))
            {
                s.Add($"        {rel.ChildEntity.GetGoToEntityCode()}");
                s.Add($"        vm.load{rel.ChildEntity.PluralName} = load{rel.ChildEntity.PluralName};");
                if (rel.ChildEntity.HasASortField)
                    s.Add($"        vm.{rel.ChildEntity.PluralName.ToCamelCase()}SortOptions = {{ stop: sort{rel.ChildEntity.PluralName}, handle: \"i.sortable-handle\", axis: \"y\" }};");
            }
            s.Add($"");
            s.Add($"        initPage();");

            #region init
            s.Add($"");
            s.Add($"        function initPage() {{");
            s.Add($"");
            s.Add($"            var promises = [];");
            s.Add($"");
            foreach (var entity in CurrentEntity.RelationshipsAsChild.Where(r => !r.Hierarchy).Select(r => r.ParentEntity).Distinct())
            {
                s.Add($"            promises.push(");
                s.Add($"                { entity.ResourceName}.query(");
                s.Add($"                    {{");
                s.Add($"                        pageSize: 0");
                s.Add($"                    }},");
                s.Add($"                    data => {{");
                s.Add($"                        vm.{entity.PluralName.ToCamelCase()} = data;");
                s.Add($"                    }},");
                s.Add($"                    err => {{");
                s.Add($"                        notifications.error(\"Failed to load the {entity.PluralFriendlyName.ToLower()}.\", \"Error\", err);");
                s.Add($"                        {CurrentEntity.DefaultGo}");
                s.Add($"                    }}).$promise");
                s.Add($"            );");
                s.Add($"");
            }
            s.Add($"            $q.all(promises)");
            s.Add($"                .then(() => {{");
            s.Add($"");
            s.Add($"                    if (vm.isNew) {{");
            s.Add($"");
            s.Add($"                        {CurrentEntity.ViewModelObject} = new {CurrentEntity.ResourceName}();");
            // without the HasCompositePrimaryKey check, it sets the values of the FK fields to newGuid, which then changes the coloring of nasBySelect (not 'empty') and sometimes selects an option (e.g. municipality in iber/datum)
            if (!CurrentEntity.HasCompositePrimaryKey)
                foreach (var field in CurrentEntity.KeyFields.OrderBy(f => f.FieldOrder))
                    s.Add($"                        {CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()} = appSettings.{field.NewVariable};");
            foreach (var field in CurrentEntity.Fields.Where(f => !string.IsNullOrWhiteSpace(f.EditPageDefault)))
                s.Add($"                        {CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()} = {field.EditPageDefault};");
            var relationship = CurrentEntity.RelationshipsAsChild.FirstOrDefault(r => r.Hierarchy);
            if (relationship != null)
            {
                foreach (var relationshipField in relationship.RelationshipFields)
                    s.Add($"                        {CurrentEntity.ViewModelObject}.{relationshipField.ChildField.Name.ToCamelCase()} = $stateParams.{relationshipField.ParentField.Name.ToCamelCase()};");
                s.Add($"");
                s.Add($"                        promises = [];");
                s.Add($"");
                s.Add($"                        promises.push(");
                s.Add($"                            {relationship.ParentEntity.ResourceName}.get(");
                s.Add($"                                {{");
                foreach (var field in relationship.ParentEntity.KeyFields)
                    s.Add($"                                    {field.Name.ToCamelCase()}: $stateParams.{field.Name.ToCamelCase()}" + (field == relationship.ParentEntity.KeyFields.Last() ? string.Empty : ","));
                s.Add($"                                }},");
                s.Add($"                                data => {{");
                s.Add($"                                    vm.{relationship.ParentEntity.Name.ToCamelCase()} = data;");
                var nextRelationship = relationship.ParentEntity.RelationshipsAsChild.Where(r => r.Hierarchy).FirstOrDefault();
                while (nextRelationship != null)
                {
                    s.Add($"                                    vm.{nextRelationship.ParentEntity.Name.ToCamelCase()} = vm.{nextRelationship.ChildEntity.Name.ToCamelCase()}.{nextRelationship.ParentEntity.Name.ToCamelCase()};");
                    nextRelationship = nextRelationship.ParentEntity.RelationshipsAsChild.Where(r => r.Hierarchy).FirstOrDefault();
                }
                s.Add($"                                }},");
                s.Add($"                                err => {{");
                s.Add($"");
                s.Add($"                                    errorService.handleApiError(err, \"{relationship.ParentEntity.FriendlyName.ToLower()}\", \"load\");");
                s.Add($"                                    {CurrentEntity.DefaultGo}");
                s.Add($"");
                s.Add($"                                }}).$promise");
                s.Add($"                        );");
                s.Add($"");
                s.Add($"                        $q.all(promises).finally(() => vm.loading = false);");
            }
            else
            {
                s.Add($"                        vm.loading = false;");
            }
            s.Add($"");
            s.Add($"                    }} else {{");
            s.Add($"");
            s.Add($"                        promises = [];");
            s.Add($"");
            s.Add($"                        promises.push(");
            s.Add($"                            {CurrentEntity.ResourceName}.get(");
            s.Add($"                                {{");
            foreach (var field in CurrentEntity.KeyFields.OrderBy(f => f.FieldOrder))
                s.Add($"                                    {field.Name.ToCamelCase()}: $stateParams.{field.Name.ToCamelCase()}" + (field == CurrentEntity.KeyFields.OrderBy(f => f.FieldOrder).Last() ? string.Empty : ","));
            s.Add($"                                }},");
            s.Add($"                                data => {{");
            if (CurrentEntity.Project.ExcludeTypes)
                s.Add($"                                    {CurrentEntity.ViewModelObject} = data;");
            else
                s.Add($"                                    angular.copy(data, {CurrentEntity.ViewModelObject});");
            var lastEntity = CurrentEntity;
            foreach (var entity in CurrentEntity.GetNavigationEntities(true))
            {
                if (entity == CurrentEntity) continue;
                s.Add($"                                    vm.{entity.Name.ToCamelCase()} = vm.{lastEntity.Name.ToCamelCase()}.{entity.Name.ToCamelCase()};");
                lastEntity = entity;
            }
            s.Add($"                                }},");
            s.Add($"                                err => {{");
            s.Add($"");
            s.Add($"                                    errorService.handleApiError(err, \"{CurrentEntity.FriendlyName.ToLower()}\", \"load\");");
            s.Add($"                                    {CurrentEntity.DefaultGo}");
            s.Add($"");
            s.Add($"                                }}).$promise");
            s.Add($"                        );");
            s.Add($"");
            foreach (var rel in CurrentEntity.RelationshipsAsParent.Where(r => r.DisplayListOnParent))
            {
                s.Add($"                        promises.push(load{rel.ChildEntity.PluralName}(0, true));");
            }
            if (CurrentEntity.RelationshipsAsParent.Where(r => r.DisplayListOnParent).Count() > 0)
                s.Add($"");
            s.Add($"                        $q.all(promises).finally(() => vm.loading = false);");
            s.Add($"                    }}");
            s.Add($"                }});");
            s.Add($"        }}");
            #endregion

            #region save
            s.Add($"");
            s.Add($"        function save() {{");
            s.Add($"");
            s.Add($"            if ($scope.mainForm.$invalid) {{");
            s.Add($"");
            s.Add($"                notifications.error(\"The form has not been completed correctly.\", \"Error\");");
            s.Add($"                return;");
            s.Add($"");
            s.Add($"            }}");
            s.Add($"");
            s.Add($"            vm.loading = true;");
            s.Add($"");
            s.Add($"            {CurrentEntity.ViewModelObject}.$save(");
            s.Add($"                data => {{");
            s.Add($"");
            // ngResource automatically updates the object
            //s.Add($"                    {CurrentEntity.ViewModelObject} = data;");
            s.Add($"                    notifications.success(\"The {CurrentEntity.FriendlyName.ToLower()} has been saved.\", \"Saved\");");
            s.Add($"                    if (vm.isNew)");
            s.Add($"                        $state.go(\"app.{CurrentEntity.CamelCaseName}\", {{");
            foreach (var field in CurrentEntity.KeyFields.OrderBy(f => f.FieldOrder))
                s.Add($"                            {field.Name.ToCamelCase()}: {CurrentEntity.ViewModelObject}.{field.Name.ToCamelCase()}" + (field == CurrentEntity.KeyFields.OrderBy(f => f.FieldOrder).Last() ? string.Empty : ","));
            s.Add($"                        }});");
            s.Add($"");
            s.Add($"                }},");
            s.Add($"                err=> {{");
            s.Add($"");
            s.Add($"                    errorService.handleApiError(err, \"{CurrentEntity.FriendlyName.ToLower()}\");");
            s.Add($"");
            s.Add($"                }}).finally(() => vm.loading = false);");
            s.Add($"");
            s.Add($"        }};");
            #endregion

            #region delete
            s.Add($"");
            s.Add($"        function del() {{");
            s.Add($"");
            s.Add($"            if (!confirm(\"Confirm delete?\")) return;");
            s.Add($"");
            s.Add($"            vm.loading = true;");
            s.Add($"");
            s.Add($"            {CurrentEntity.ResourceName}.delete(");
            s.Add($"                {{");
            foreach (var field in CurrentEntity.KeyFields.OrderBy(f => f.FieldOrder))
                s.Add($"                    {field.Name.ToCamelCase()}: $stateParams.{field.Name.ToCamelCase()}" + (field == CurrentEntity.KeyFields.OrderBy(f => f.FieldOrder).Last() ? string.Empty : ","));
            s.Add($"                }},");
            s.Add($"                () => {{");
            s.Add($"");
            s.Add($"                    notifications.success(\"The {CurrentEntity.FriendlyName.ToLower()} has been deleted.\", \"Deleted\");");
            s.Add($"                    {CurrentEntity.DefaultGo}");
            s.Add($"");
            s.Add($"                }}, err => {{");
            s.Add($"");
            s.Add($"                    errorService.handleApiError(err, \"{CurrentEntity.FriendlyName.ToLower()}\", \"delete\");");
            s.Add($"");
            s.Add($"                }})");
            s.Add($"                .$promise.finally(() => vm.loading = false);");
            s.Add($"");
            s.Add($"        }}");
            #endregion

            #region load child entities
            foreach (var rel in CurrentEntity.RelationshipsAsParent.Where(r => r.DisplayListOnParent))
            {
                s.Add($"");
                s.Add($"        function load{rel.ChildEntity.PluralName}(pageIndex, dontSetLoading) {{");
                s.Add($"");
                s.Add($"            if (!dontSetLoading) vm.loading = true;");
                s.Add($"");
                s.Add($"            var promise = {rel.ChildEntity.ResourceName}.{(rel.ChildEntity.HasCompositePrimaryKey ? "search" : "query")}(");
                s.Add($"                {{");
                foreach (var field in rel.RelationshipFields)
                    s.Add($"                    {field.ChildField.Name.ToCamelCase()}: $stateParams.{field.ParentField.Name.ToCamelCase()},");
                if (rel.ChildEntity.Fields.Any(f => f.EditPageType == EditPageType.SortField))
                    s.Add($"                    pageSize: 0,");
                if (rel.Hierarchy)
                {
                    s.Add($"                    pageIndex: pageIndex,");
                    s.Add($"                    includeEntities: true");
                }
                else
                {
                    s.Add($"                    pageIndex: pageIndex");
                }
                s.Add($"                }},");
                s.Add($"                (data, headers) => {{");
                if (!rel.ChildEntity.HasASortField)
                    s.Add($"                    vm.{rel.ChildEntity.PluralName.ToCamelCase()}Headers = JSON.parse(headers(\"X-Pagination\"))");
                s.Add($"                    vm.{rel.CollectionName.ToCamelCase()} = data;");
                s.Add($"                }},");
                s.Add($"                err => {{");
                s.Add($"");
                s.Add($"                    notifications.error(\"Failed to load the {rel.ChildEntity.PluralFriendlyName.ToCamelCase()}.\", \"Error\", err);");
                s.Add($"                    $state.go(\"app.{CurrentEntity.PluralName.ToCamelCase()}\");");
                s.Add($"");
                s.Add($"                }}).$promise;");
                s.Add($"");
                s.Add($"            promise.finally(() => {{ if (!dontSetLoading) vm.loading = false; }});");
                s.Add($"");
                s.Add($"            return promise;");
                s.Add($"        }}");
            }
            #endregion

            #region sort child entities
            foreach (var entity in CurrentEntity.RelationshipsAsParent.Where(r => r.ChildEntity.HasASortField && r.DisplayListOnParent).Select(r => r.ChildEntity))
            {
                s.Add($"");
                s.Add($"        function sort{entity.PluralName}(e, ui) {{");
                s.Add($"");
                s.Add($"            var ids = [];");
                s.Add($"            angular.forEach(vm.{entity.PluralName.ToCamelCase()}, function (item, index) {{");
                s.Add($"                ids.push(item.{entity.KeyFields[0].Name.ToCamelCase()});");
                s.Add($"            }});");
                s.Add($"");
                s.Add($"            vm.loading = true;");
                s.Add($"            {entity.ResourceName}.sort(");
                s.Add($"                {{");
                s.Add($"                    ids: ids");
                s.Add($"                }},");
                s.Add($"                data => {{");
                s.Add($"");
                s.Add($"                    notifications.success(\"The sort order has been updated\", \"{entity.FriendlyName} Ordering\");");
                s.Add($"");
                s.Add($"                }},");
                s.Add($"                err => {{");
                s.Add($"");
                s.Add($"                    notifications.error(\"Failed to sort the {entity.PluralFriendlyName.ToLower()}. \" + (err.data && err.data.message ? err.data.message : \"\"), \"Error\", err);");
                s.Add($"");
                s.Add($"                }})");
                s.Add($"                .$promise.finally(() => vm.loading = false);");
                s.Add($"");
                s.Add($"        }}");
            }
            #endregion

            s.Add($"    }};");
            s.Add($"");
            s.Add($"}} ());");

            return RunCodeReplacements(s.ToString(), CodeType.EditTypeScript);

        }

        // ---- HELPER METHODS -----------------------------------------------------------------------

        private string ClimbHierarchy(Relationship relationship, string result)
        {
            result += "." + relationship.ParentName;
            foreach (var relAbove in relationship.ParentEntity.RelationshipsAsChild.Where(r => r.Hierarchy))
                result = ClimbHierarchy(relAbove, result);
            return result;
        }

        private string RunCodeReplacements(string code, CodeType type)
        {
            // todo: needs a sort order

            var replacements = CurrentEntity.CodeReplacements.Where(cr => !cr.Disabled && cr.CodeType == type).ToList();
            replacements.InsertRange(0, DbContext.CodeReplacements.Where(o => o.Entity.ProjectId == CurrentEntity.ProjectId && !o.Disabled && o.CodeType == CodeType.Global).ToList());

            // common scripts need a common replacement
            if (type == CodeType.Enums || type == CodeType.ApiResource || type == CodeType.AppRouter || type == CodeType.BundleConfig || type == CodeType.DbContext)
                replacements = CodeReplacements.Where(cr => !cr.Disabled && cr.CodeType == type && cr.Entity.ProjectId == CurrentEntity.ProjectId).ToList();

            foreach (var replacement in replacements)
            {
                var findCode = replacement.FindCode.Replace("(", "\\(").Replace(")", "\\)").Replace("[", "\\[").Replace("]", "\\]").Replace("?", "\\?").Replace("*", "\\*").Replace("$", "\\$").Replace("+", "\\+").Replace("{", "\\{").Replace("}", "\\}").Replace("|", "\\|").Replace("\n", "\r\n").Replace("\r\r", "\r");
                var re = new Regex(findCode);
                if (replacement.CodeType != CodeType.Global && !re.IsMatch(code))
                    throw new Exception($"{CurrentEntity.Name} failed to replace {replacement.Purpose} in {replacement.Entity.Name}.{type.ToString()}");
                code = re.Replace(code, replacement.ReplacementCode ?? string.Empty).Replace("\n", "\r\n").Replace("\r\r", "\r");
            }
            return code;
        }

        private string GetKeyFieldLinq(string entityName, string otherEntityName = null, string comparison = "==", string joiner = "&&")
        {
            return CurrentEntity.KeyFields.Select(o => $"{entityName}.{o.Name} {comparison} " + (otherEntityName == null ? o.Name.ToCamelCase() : $"{otherEntityName}.{o.Name}")).Aggregate((current, next) => $"{current} {joiner} {next}");
        }

        public List<string> GetTopAncestors(List<string> list, string prefix, Relationship relationship, RelationshipAncestorLimits ancestorLimit, int level = 0)
        {
            //if (relationship.RelationshipAncestorLimit == RelationshipAncestorLimits.Exclude) return list;
            prefix += "." + relationship.ParentName;
            if (ancestorLimit == RelationshipAncestorLimits.IncludeRelatedEntity && level == 0)
            {
                list.Add(prefix);
            }
            else if (ancestorLimit == RelationshipAncestorLimits.IncludeRelatedParents && level == 1)
            {
                list.Add(prefix);
            }
            else if (relationship.ParentEntity.RelationshipsAsChild.Any() && relationship.ParentEntityId != relationship.ChildEntityId)
            {
                foreach (var parentRelationship in relationship.ParentEntity.RelationshipsAsChild.Where(r => r.RelationshipAncestorLimit != RelationshipAncestorLimits.Exclude))
                {
                    list = GetTopAncestors(list, prefix, parentRelationship, ancestorLimit, level + 1);
                }
            }
            else
            {
                list.Add(prefix);
            }
            return list;
        }

        public string Validate()
        {
            if (CurrentEntity.Fields.Count == 0) return "No fields are defined";
            if (CurrentEntity.KeyFields.Count == 0) return "No key fields are defined";
            if (!CurrentEntity.Fields.Any(f => f.ShowInSearchResults)) return "No fields are designated as search result fields";
            var rel = CurrentEntity.RelationshipsAsChild.FirstOrDefault(r => r.RelationshipFields.Count == 0);
            if (rel != null) return $"Relationship {rel.CollectionName} (to {rel.ParentEntity.FriendlyName}) has no link fields defined";
            rel = CurrentEntity.RelationshipsAsParent.FirstOrDefault(r => r.RelationshipFields.Count == 0);
            if (rel != null) return $"Relationship {rel.CollectionName} (to {rel.ChildEntity.FriendlyName}) has no link fields defined";
            //if (CurrentEntity.RelationshipsAsChild.Where(r => r.Hierarchy).Count() > 1) return $"{CurrentEntity.Name} is a hierarchical child on more than one relationship";
            return null;
        }

        public static string RunDeployment(ApplicationDbContext DbContext, Entity entity, DeploymentOptions deploymentOptions)
        {
            var codeGenerator = new Code(entity, DbContext);

            var error = codeGenerator.Validate();
            if (error != null)
                return (error);

            if (!Directory.Exists(entity.Project.RootPath))
                return ("Project path does not exist");

            if (deploymentOptions.Model && !string.IsNullOrWhiteSpace(entity.PreventModelDeployment))
                return ("Model deployment is not allowed: " + entity.PreventModelDeployment);
            if (deploymentOptions.DTO && !string.IsNullOrWhiteSpace(entity.PreventDTODeployment))
                return ("DTO deployment is not allowed: " + entity.PreventDTODeployment);
            if (deploymentOptions.DbContext && !string.IsNullOrWhiteSpace(entity.PreventDbContextDeployment))
                return ("DbContext deployment is not allowed: " + entity.PreventDbContextDeployment);
            if (deploymentOptions.Controller && !string.IsNullOrWhiteSpace(entity.PreventControllerDeployment))
                return ("Controller deployment is not allowed: " + entity.PreventControllerDeployment);
            if (deploymentOptions.BundleConfig && !string.IsNullOrWhiteSpace(entity.PreventBundleConfigDeployment))
                return ("BundleConfig deployment is not allowed: " + entity.PreventBundleConfigDeployment);
            if (deploymentOptions.AppRouter && !string.IsNullOrWhiteSpace(entity.PreventAppRouterDeployment))
                return ("AppRouter deployment is not allowed: " + entity.PreventAppRouterDeployment);
            if (deploymentOptions.ApiResource && !string.IsNullOrWhiteSpace(entity.PreventApiResourceDeployment))
                return ("ApiResource deployment is not allowed: " + entity.PreventApiResourceDeployment);
            if (deploymentOptions.ListHtml && !string.IsNullOrWhiteSpace(entity.PreventListHtmlDeployment))
                return ("ListHtml deployment is not allowed: " + entity.PreventListHtmlDeployment);
            if (deploymentOptions.ListTypeScript && !string.IsNullOrWhiteSpace(entity.PreventListTypeScriptDeployment))
                return ("ListTypeScript deployment is not allowed: " + entity.PreventListTypeScriptDeployment);
            if (deploymentOptions.EditHtml && !string.IsNullOrWhiteSpace(entity.PreventEditHtmlDeployment))
                return ("EditHtml deployment is not allowed: " + entity.PreventEditHtmlDeployment);
            if (deploymentOptions.EditTypeScript && !string.IsNullOrWhiteSpace(entity.PreventEditTypeScriptDeployment))
                return ("EditTypeScript deployment is not allowed: " + entity.PreventEditTypeScriptDeployment);

            if (deploymentOptions.DbContext)
            {
                var firstEntity = DbContext.Entities.SingleOrDefault(e => e.ProjectId == entity.ProjectId && e.PreventDbContextDeployment.Length > 0);
                if (firstEntity != null)
                    return ("DbContext deployment is not allowed on " + firstEntity.Name + ": " + entity.PreventDbContextDeployment);
            }
            if (deploymentOptions.BundleConfig)
            {
                var firstEntity = DbContext.Entities.SingleOrDefault(e => e.ProjectId == entity.ProjectId && e.PreventBundleConfigDeployment.Length > 0);
                if (firstEntity != null)
                    return ("BundleConfig deployment is not allowed on " + firstEntity.Name + ": " + entity.PreventBundleConfigDeployment);
            }
            if (deploymentOptions.AppRouter)
            {
                var firstEntity = DbContext.Entities.SingleOrDefault(e => e.ProjectId == entity.ProjectId && e.PreventAppRouterDeployment.Length > 0);
                if (firstEntity != null)
                    return ("AppRouter deployment is not allowed on " + firstEntity.Name + ": " + entity.PreventAppRouterDeployment);
            }
            if (deploymentOptions.ApiResource)
            {
                var firstEntity = DbContext.Entities.SingleOrDefault(e => e.ProjectId == entity.ProjectId && e.PreventApiResourceDeployment.Length > 0);
                if (firstEntity != null)
                    return ("ApiResource deployment is not allowed on " + firstEntity.Name + ": " + entity.PreventApiResourceDeployment);
            }

            #region model
            if (deploymentOptions.Model)
            {
                var path = Path.Combine(entity.Project.RootPath, "Models");
                if (!Directory.Exists(path))
                    return ("Models path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateModel();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, entity.Name + ".cs"), code);
            }
            #endregion

            #region enums
            if (deploymentOptions.Enums)
            {
                var path = Path.Combine(entity.Project.RootPath, "Models");
                if (!Directory.Exists(path))
                    return ("Models path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateEnums();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, "Enums.cs"), code);
            }
            #endregion

            #region dto
            if (deploymentOptions.DTO)
            {
                var path = Path.Combine(entity.Project.RootPath, "Models\\DTOs");
                if (!Directory.Exists(path))
                    return ("DTOs path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateDTO();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, entity.Name + "DTO.cs"), code);
            }
            #endregion

            #region settings dto
            if (deploymentOptions.SettingsDTO)
            {
                var path = Path.Combine(entity.Project.RootPath, "Models\\DTOs");
                if (!Directory.Exists(path))
                    return ("DTOs path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateSettingsDTO();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, "SettingsDTO_.cs"), code);
            }
            #endregion

            #region dbcontext
            if (deploymentOptions.DbContext)
            {
                var path = Path.Combine(entity.Project.RootPath, "Models");
                if (!Directory.Exists(path))
                    return ("Models path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateDbContext();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, "ApplicationDBContext_.cs"), code);
            }
            #endregion

            #region controller
            if (deploymentOptions.Controller)
            {
                var path = Path.Combine(entity.Project.RootPath, "Controllers\\API");
                if (!Directory.Exists(path))
                    return ("Controllers path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateController();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, entity.PluralName + "Controller.cs"), code);
            }
            #endregion

            #region settings
            if (deploymentOptions.SettingsDTO)
            {
                var path = Path.Combine(entity.Project.RootPath, "Models\\DTOs");
                if (!Directory.Exists(path))
                    return ("DTOs path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateSettingsDTO();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, "SettingsDTO_.cs"), code);
            }
            #endregion

            #region bundleconfig
            if (deploymentOptions.BundleConfig)
            {
                var path = Path.Combine(entity.Project.RootPath, "App_Start");
                if (!Directory.Exists(path))
                    return ("App_Start path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateBundleConfig();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, "BundleConfig_.cs"), code);
            }
            #endregion

            #region app router
            if (deploymentOptions.AppRouter)
            {
                var path = Path.Combine(entity.Project.RootPath, "app\\common");
                if (!Directory.Exists(path))
                    return ("App\\Common path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateAppRouter();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, "routes-entity.ts"), code);
            }
            #endregion

            #region api resource
            if (deploymentOptions.ApiResource)
            {
                var path = Path.Combine(entity.Project.RootPath, "app\\common");
                if (!Directory.Exists(path))
                    return ("App\\Common path does not exist");

                // todo: backup file

                var code = codeGenerator.GenerateApiResource();
                if (code != string.Empty) File.WriteAllText(Path.Combine(path, "api-entity.ts"), code);
            }
            #endregion

            #region list html
            if (deploymentOptions.ListHtml)
            {
                if (!CreateAppDirectory(entity, codeGenerator.GenerateListHtml(), entity.PluralName.ToLower() + ".html"))
                    return ("App path does not exist");
            }
            #endregion

            #region list typescript
            if (deploymentOptions.ListTypeScript)
            {
                if (!CreateAppDirectory(entity, codeGenerator.GenerateListTypeScript(), entity.PluralName.ToLower() + ".ts"))
                    return ("App path does not exist");
            }
            #endregion

            #region edit html
            if (deploymentOptions.EditHtml)
            {
                if (!CreateAppDirectory(entity, codeGenerator.GenerateEditHtml(), entity.Name.ToLower() + ".html"))
                    return ("App path does not exist");
            }
            #endregion

            #region edit typescript
            if (deploymentOptions.EditTypeScript)
            {
                if (!CreateAppDirectory(entity, codeGenerator.GenerateEditTypeScript(), entity.Name.ToLower() + ".ts"))
                    return ("App path does not exist");
            }
            #endregion

            return null;
        }

        private static bool CreateAppDirectory(Entity entity, string code, string fileName)
        {
            if (code == string.Empty) return true;

            var path = Path.Combine(entity.Project.RootPath, "app");
            if (!Directory.Exists(path))
                return false;
            path = Path.Combine(path, entity.PluralName.ToLower());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // todo: backup file

            File.WriteAllText(Path.Combine(path, fileName), code);
            return true;
        }

    }

    public class DeploymentOptions
    {
        public bool Model { get; set; }
        public bool Enums { get; set; }
        public bool DTO { get; set; }
        public bool SettingsDTO { get; set; }
        public bool DbContext { get; set; }
        public bool Controller { get; set; }
        public bool BundleConfig { get; set; }
        public bool AppRouter { get; set; }
        public bool ApiResource { get; set; }
        public bool ListHtml { get; set; }
        public bool ListTypeScript { get; set; }
        public bool EditHtml { get; set; }
        public bool EditTypeScript { get; set; }
    }

}
