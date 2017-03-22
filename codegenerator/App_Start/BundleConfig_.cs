using System.Web.Optimization;

namespace WEB
{
    public partial class BundleConfig
    {
        public static void AddGeneratedBundles(Bundle bundle)
        {
            bundle.Include(
                "~/app/codereplacements/codereplacement.js",
                "~/app/codereplacements/codereplacements.js",
                "~/app/entities/entity.js",
                "~/app/entities/entities.js",
                "~/app/fields/field.js",
                "~/app/fields/fields.js",
                "~/app/lookups/lookup.js",
                "~/app/lookups/lookups.js",
                "~/app/lookupoptions/lookupoption.js",
                "~/app/lookupoptions/lookupoptions.js",
                "~/app/projects/project.js",
                "~/app/projects/projects.js",
                "~/app/relationships/relationship.js",
                "~/app/relationships/relationships.js",
                "~/app/relationshipfields/relationshipfield.js",
                "~/app/relationshipfields/relationshipfields.js"
                );
        }
    }
}
