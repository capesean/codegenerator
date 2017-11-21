using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WEB.Models
{
    public partial class Entity
    {
        [NotMapped]
        public virtual List<Field> KeyFields
        {
            get
            {
                return Fields.Where(o => o.KeyField).OrderBy(o => o.FieldOrder).ToList();
            }
        }

        [NotMapped]
        public virtual List<Field> TextSearchFields
        {
            get
            {
                return Fields.Where(o => o.SearchType == SearchType.Text).OrderBy(o => o.FieldOrder).ToList();
            }
        }

        [NotMapped]
        public virtual List<Field> ExactSearchFields
        {
            get
            {
                return Fields.Where(o => o.SearchType == SearchType.Exact).OrderBy(o => o.FieldOrder).ToList();
            }
        }

        [NotMapped]
        public virtual List<Field> SortOrderFields
        {
            get
            {
                if (!Fields.Any(f => f.SortPriority.HasValue)) return KeyFields;
                return Fields.Where(o => o.SortPriority.HasValue).OrderBy(o => o.SortPriority).ToList();
            }
        }

        [NotMapped]
        public string RoutePath
        {
            get
            {
                return KeyFields.Select(o => $"{{{o.Name.ToCamelCase() + (o.ControllerConstraintType == "string" ? string.Empty : ":" + o.ControllerConstraintType)}}}").Aggregate((current, next) => current + "/" + next);
            }
        }

        [NotMapped]
        public string ControllerParameters
        {
            get
            {
                return KeyFields.Select(o => string.Format("{0} {1}", o.NetType, o.Name.ToCamelCase())).Aggregate((current, next) => current + ", " + next);
            }
        }

        [NotMapped]
        public List<Entity> RequiredRelationshipEntities
        {
            get
            {
                var entities = RelationshipsAsChild.Where(r => r.ParentEntityId != EntityId).Select(r => r.ParentEntity).ToList();
                entities.AddRange(RelationshipsAsParent.Where(r => r.DisplayListOnParent).Select(r => r.ChildEntity).ToList());
                return entities.Distinct().ToList();
            }
        }

        [NotMapped]
        public string DTOName
        {
            get
            {
                return Name + "DTO";
            }
        }

        [NotMapped]
        public string DefaultGo
        {
            get
            {
                var navEntities = GetNavigationEntities();
                if (navEntities.Count == 1 && navEntities[0].EntityId == EntityId)
                    return $"$state.go(\"app.{PluralName.ToCamelCase()}\");";
                else
                {
                    var navFields = GetNavigationFields();
                    var url = string.Empty;
                    foreach (var field in navFields)
                    {
                        if (field.EntityId == EntityId) continue;
                        url += (url == string.Empty ? string.Empty : ", ") + field.Name.ToCamelCase() + $": $stateParams.{field.Name.ToCamelCase()}";
                    }
                    url = $"$state.go(\"app.{navEntities[navEntities.Count - 2].Name.ToCamelCase()}\", {{ {url} }});";
                    return url;
                }
            }
        }

        public class SearchResultColumn
        {
            public string Header { get; set; }
            public string Value { get; set; }
        }

        internal Relationship GetParentSearchRelationship(Field field)
        {
            // field must be in a relationship of this entity, where the field is (part of) the key of the related entity
            var relationship = RelationshipsAsChild.Single(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId && f.ParentField.KeyField));
            if (relationship.RelationshipFields.Count() != 1) throw new Exception("Can't have a search field that is part of a multi-field key");
            return relationship;
        }

        internal List<SearchResultColumn> GetSearchResultsFields(Entity currentEntity)
        {
            var result = new List<SearchResultColumn>();
            foreach (var field in Fields.Where(f => f.ShowInSearchResults).OrderBy(f => f.FieldOrder))
            {
                if (RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                {
                    var rel = RelationshipsAsChild.Single(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId));
                    if (rel.ParentEntity == currentEntity) continue;

                    result.Add(new SearchResultColumn { Header = rel.ParentEntity.FriendlyName, Value = $"{{{{ { field.Entity.Name.ToCamelCase()}.{ rel.ParentName.ToCamelCase() }.{ rel.ParentField.Name.ToCamelCase() } }}}}" });
                }
                else
                {
                    var column = new SearchResultColumn { Header = field.Label };
                    if (field.FieldType == FieldType.Enum)
                        column.Value = $"{{{{ vm.appSettings.find(vm.appSettings.{ field.Lookup.Name.ToCamelCase() }, {field.Entity.Name.ToCamelCase()}.{ field.Name.ToCamelCase() }).label }}}}";
                    else if (field.FieldType == FieldType.Bit)
                        column.Value = $"{{{{ { field.Entity.Name.ToCamelCase()}.{ field.Name.ToCamelCase() } ? \"Yes\" : \"No\" }}}}";
                    else if (field.FieldType == FieldType.Date)
                        column.Value = $"{{{{ { field.Entity.Name.ToCamelCase()}.{ field.Name.ToCamelCase() } | toLocaleDateString }}}}";
                    else
                        column.Value = $"{{{{ { field.Entity.Name.ToCamelCase()}.{ field.Name.ToCamelCase() } }}}}";
                    result.Add(column);
                }
            }

            return result;
        }

        internal List<Field> GetNavigationFields()
        {
            var result = new List<Field>();
            foreach (var field in KeyFields.OrderBy(f => f.SortPriority))
                result.Add(field);

            // todo: if more than one relationship, will this FirstOrDefault randomly choose the wrong one?
            var relationship = RelationshipsAsChild.FirstOrDefault(r => r.Hierarchy);
            if (relationship != null)
            {
                var parentResult = relationship.ParentEntity.GetNavigationFields();
                result.InsertRange(0, parentResult);
            }

            return result;
        }

        internal List<Entity> GetNavigationEntities(bool reverse = false)
        {
            var entities = GetNavigationFields().Select(f => f.Entity).Distinct().ToList();
            if (reverse) entities.Reverse();
            return entities;
        }

        internal string GetNavigationUrl()
        {
            // builds the string for the app-router.ts, in the format: /parent1Name/:parent1Key1/:parent1Key2/entity1Name/:key1/:key2
            var result = "/";

            var navigationFields = GetNavigationFields();
            Entity lastEntity = null;
            foreach (var field in navigationFields)
            {
                if (lastEntity != field.Entity) result += field.Entity.PluralName.ToLower() + "/";
                result += ":" + field.Name.ToCamelCase() + (field == navigationFields.Last() ? string.Empty : "/");
                lastEntity = field.Entity;
            }

            return result;
        }

        internal string GetGoToEntityCode()
        {
            var navFields = GetNavigationFields();
            return $"vm.goTo{Name} = ({navFields.Select(o => o.Name.ToCamelCase()).Aggregate((current, next) => current + ", " + next) }) => $state.go(\"app.{Name.ToCamelCase()}\", {{ {navFields.Select(o => o.Name.ToCamelCase() + ": " + o.Name.ToCamelCase()).Aggregate((current, next) => current + ", " + next) } }});";
        }

        internal string GetNavigationString()
        {
            // builds the parameters for the vm.goToEntity() call in the html (NOT the method in typescript)
            // in the format: "entity.parent1.key1, entity.parent1.key2, entity.key1, entity.key2"
            var navigationString = string.Empty;
            foreach (var field in GetNavigationFields())
            {
                var reversedNavFields = GetNavigationFields();
                reversedNavFields.Reverse();
                var prefix = string.Empty;
                Entity lastEntity = null;
                foreach (var f in reversedNavFields)
                {
                    if (f.Entity == lastEntity) continue;

                    prefix += f.Entity.Name.ToCamelCase() + ".";

                    if (f.Entity == field.Entity) break;
                    lastEntity = f.Entity;
                }
                navigationString += (navigationString == string.Empty ? string.Empty : ", ") + prefix + field.Name.ToCamelCase();
            }
            return navigationString;
        }

        [NotMapped]
        internal string ResourceName
        {
            get
            {
                if (EntityType == EntityType.User) return "userResource";
                return Name.ToCamelCase() + "Resource";
            }
        }

        [NotMapped]
        internal string TypeScriptResource
        {
            get
            {
                return "Models." + Name + "Resource";
            }
        }

        [NotMapped]
        internal string TypeScriptDTO
        {
            get
            {
                return "Models." + Name + "DTO";
            }
        }

        [NotMapped]
        internal string CamelCaseName
        {
            get
            {
                return Name.ToCamelCase();
            }
        }

        [NotMapped]
        internal string ViewModelObject
        {
            get
            {
                if (Project.ExcludeTypes) return "vm." + Name.ToCamelCase();
                return Name.ToCamelCase();
            }
        }

        internal bool HasASortField
        {
            get
            {
                var sortCount = Fields.Count(f => f.EditPageType == EditPageType.SortField);

                if (sortCount > 1)
                    throw new InvalidOperationException("Only 1 sort field is allowed per entity");


                if (sortCount == 1)
                {
                    if (SortField.FieldType != FieldType.Int) throw new InvalidOperationException("Sortable fields must be of type Int");
                    //if (KeyFields.Count() > 1) throw new InvalidOperationException("Sortable is only allowed on single-keyed entities");
                    if (KeyFields[0].FieldType != FieldType.Guid) throw new InvalidOperationException("Sortable is only allowed on Guid-keyed entities");
                }

                return sortCount == 1;
            }
        }

        internal bool HasCompositePrimaryKey
        {
            get
            {
                // checks for composite primary key, i.e. a key made up of foreign keys to other entities,
                // like a fact key being the combination of the munic, year, & indicator entities.
                var returnVal = true;

                // force it for Tokens in IBER
                if (EntityId == new Guid("aba77376-8d43-4578-b157-9561f98cd6ff")) return true;

                foreach (var field in KeyFields)
                {
                    if (!RelationshipsAsChild.Any(r => r.RelationshipFields.Any(f => f.ChildFieldId == field.FieldId)))
                        returnVal = false;
                }


                return returnVal;
            }
        }

        internal Field SortField
        {
            get
            {
                return Fields.SingleOrDefault(f => f.EditPageType == EditPageType.SortField);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
