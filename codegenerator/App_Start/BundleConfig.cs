using System.Web.Optimization;

namespace WEB
{
    public partial class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //BundleTable.EnableOptimizations = false;

            bundles.Add(new ScriptBundle("~/scripts/login").Include(
                "~/scripts/jquery-2.2.3.js",
                "~/scripts/toastr.min.js"
                ));

            var scriptBundle = new ScriptBundle("~/scripts/main").Include(
                // ----- 3RD PARTY
                "~/scripts/moment.js",
                "~/scripts/jquery-2.2.3.js",
                "~/scripts/jquery-ui-1.11.4.min.js",
                "~/scripts/angular.js",
                "~/scripts/angular-ui-router.js",
                "~/scripts/angular-resource.js",
                "~/scripts/angular-sanitize.js",
                "~/scripts/angular-local-storage.js",
                //"~/scripts/fullcalendar.min.js",
                //"~/scripts/calendar.js",
                "~/scripts/bootstrap.js",
                "~/scripts/DataTables/jquery.dataTables.min.js",
                "~/scripts/DataTables/dataTables.bootstrap.js",
                "~/scripts/toastr.min.js",
                "~/scripts/angular-ui/ui-bootstrap.js",
                "~/scripts/angular-ui/ui-bootstrap-tpls.js",
                "~/scripts/angular-messages.min.js",
                "~/scripts/angular-breadcrumb.min.js",
                "~/scripts/nya-bs-select.min.js",
                "~/scripts/metisMenu.js",
                "~/scripts/angular-clipboard.js",
                "~/scripts/angular-ui/sortable.js",
                "~/scripts/Highlight/highlight.pack.js",
                // ----- COMMON
                "~/app/common/routes.js",
                "~/app/common/routes-entity.js", 
                "~/app/common/app.js",
                "~/app/common/api.js",
                "~/app/common/api-entity.js",
                "~/app/common/filters.js",
                "~/app/common/masterpagecontroller.js",
                "~/app/common/authservice.js",
                "~/app/common/errorservice.js",
                //"~/app/common/datepicker-popup.js",
                //"~/app/common/checklist-model.js",
                // ----- LOGIN
                "~/app/login/login.js",
                "~/app/login/resetpassword.js",
                "~/app/login/passwordreset.js",
                // ----- COMMON APP
                "~/app/home/home.js",
                "~/app/entities/entityCode.js",
                "~/app/settings/settings.js",
                "~/app/users/user.js",
                "~/app/users/users.js",
                // ----- OTHER
                "~/scripts/sb-admin-2.js"
                );

            AddGeneratedBundles(scriptBundle);

            bundles.Add(scriptBundle);

            bundles.Add(new StyleBundle("~/content/login").Include(
                "~/content/bootstrap.min.css",
                "~/content/bootstrap-theme.min.css",
                "~/content/site.css",
                "~/content/toastr.min.css",
                "~/content/font-awesome.min.css.css"
                ));

            bundles.Add(new StyleBundle("~/content/main").Include(
                "~/content/bootstrap.min.css",
                "~/content/bootstrap-theme.min.css",
                "~/content/sb-admin-2.css",
                "~/content/DataTables-1.10.4/css/dataTables.bootstrap.css",
                "~/content/site.css",
                //"~/content/fullcalendar.css",
                "~/content/toastr.min.css",
                "~/content/font-awesome.min.css",
                "~/content/nya-bs-select.min.css",
                "~/content/Highlight/vs.css",
                "~/content/metisMenu.min.css"
                ));


        }
    }
}
