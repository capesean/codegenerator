using System.Linq;

namespace WEB.Models
{
    public partial class Relationship
    {
        public string AppSelector
        {
            get
            {
                return $"<{ParentEntity.Project.AngularDirectivePrefix}-select-{ParentEntity.FriendlyName.ToLower().Replace(" ", "-")} id=\"{RelationshipFields.Single().ChildField.Name.ToCamelCase()}\" name=\"{RelationshipFields.Single().ChildField.Name.ToCamelCase()}\" ng-model=\"vm.search.{RelationshipFields.Single().ChildField.Name.ToCamelCase()}\" placeholder=\"Select {ParentFriendlyName.ToLower()}\" singular=\"{ParentFriendlyName}\" plural=\"{ParentEntity.PluralFriendlyName}\" {ParentEntity.FriendlyName.ToLower().Replace(" ", "-")}=\"vm.searchObjects.{ParentEntity.Name.ToCamelCase()}\"></{ChildEntity.Project.AngularDirectivePrefix}-select-{ParentEntity.FriendlyName.ToLower().Replace(" ", "-")}>";
            }
        }
    }
}
