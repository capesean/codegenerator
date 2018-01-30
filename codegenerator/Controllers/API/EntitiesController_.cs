using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WEB.Models;

namespace WEB.Controllers
{
    public partial class EntitiesController
    {
        [HttpPost, Route("{id:Guid}/reorderfields")]
        public async Task<IHttpActionResult> ReorderFields(Guid id, [FromBody]OrderedIds newOrders)
        {
            var entity = await DbContext.Entities.Include(e => e.Fields).SingleOrDefaultAsync(e => e.EntityId == id);
            if (entity == null)
                return NotFound();

            var newOrder = (short)0;
            foreach (var itemId in newOrders.ids)
            {
                var field = entity.Fields.SingleOrDefault(o => o.FieldId == itemId);

                if (field == null)
                    return BadRequest("Field was not found");

                DbContext.Entry(field).State = EntityState.Modified;
                field.FieldOrder = newOrder;
                newOrder++;
            }
            DbContext.Database.Log = Console.WriteLine;
            DbContext.SaveChanges();

            return Ok();
        }

        // code generation
        [HttpGet, Route("{id:Guid}/code")]
        public IHttpActionResult Code(Guid id)
        {
            var entity = DbContext.Entities
                .Include(o => o.Project)
                .Include(o => o.Fields)
                .Include(o => o.CodeReplacements)
                .Include(o => o.RelationshipsAsChild.Select(p => p.RelationshipFields))
                .Include(o => o.RelationshipsAsChild.Select(p => p.ParentEntity))
                .Include(o => o.RelationshipsAsParent.Select(p => p.RelationshipFields))
                .Include(o => o.RelationshipsAsParent.Select(p => p.ChildEntity))
                .SingleOrDefault(o => o.EntityId == id);

            if (entity == null)
                return NotFound();

            var code = new Code(entity, DbContext);

            var error = code.Validate();
            if (error != null)
                return BadRequest(error);

            var result = new ApiCodeResult();

            if (string.IsNullOrWhiteSpace(entity.PreventModelDeployment)) result.Model = code.GenerateModel(); else result.Model = "--prevented--";
            result.Enums = code.GenerateEnums();
            if (string.IsNullOrWhiteSpace(entity.PreventDTODeployment)) result.DTO = code.GenerateDTO(); else result.DTO = "--prevented--";
            result.SettingsDTO = code.GenerateSettingsDTO();
            if (string.IsNullOrWhiteSpace(entity.PreventDbContextDeployment)) result.DbContext = code.GenerateDbContext(); else result.DbContext = "--prevented--";
            if (string.IsNullOrWhiteSpace(entity.PreventControllerDeployment)) result.Controller = code.GenerateController(); else result.Controller = "--prevented--";
            if (string.IsNullOrWhiteSpace(entity.PreventBundleConfigDeployment)) result.BundleConfig = code.GenerateBundleConfig(); else result.BundleConfig = "--prevented--";
            if (string.IsNullOrWhiteSpace(entity.PreventAppRouterDeployment)) result.AppRouter = code.GenerateAppRouter(); else result.AppRouter = "--prevented--";
            if (string.IsNullOrWhiteSpace(entity.PreventApiResourceDeployment)) result.ApiResource = code.GenerateApiResource(); else result.ApiResource = "--prevented--";
            if (string.IsNullOrWhiteSpace(entity.PreventListHtmlDeployment)) result.ListHtml = code.GenerateListHtml(); else result.ListHtml = "--prevented--";
            if (string.IsNullOrWhiteSpace(entity.PreventListTypeScriptDeployment)) result.ListTypeScript = code.GenerateListTypeScript(); else result.ListTypeScript = "--prevented--";
            if (string.IsNullOrWhiteSpace(entity.PreventEditHtmlDeployment)) result.EditHtml = code.GenerateEditHtml(); else result.EditHtml = "--prevented--";
            if (string.IsNullOrWhiteSpace(entity.PreventEditTypeScriptDeployment)) result.EditTypeScript = code.GenerateEditTypeScript(); else result.EditTypeScript = "--prevented--";

            return Ok(result);
        }

        [HttpPost, Route("{id:Guid}/code")]
        public IHttpActionResult Deploy(Guid id, [FromBody]DeploymentOptions deploymentOptions)
        {
            if (!HttpContext.Current.Request.IsLocal)
                return BadRequest("Deployment is only allowed when hosted on a local machine");

            var entity = DbContext.Entities
                .Include(o => o.Project)
                .Include(o => o.Fields)
                .Include(o => o.CodeReplacements)
                .Include(o => o.RelationshipsAsChild.Select(p => p.RelationshipFields))
                .Include(o => o.RelationshipsAsChild.Select(p => p.ParentEntity))
                .Include(o => o.RelationshipsAsParent.Select(p => p.RelationshipFields))
                .Include(o => o.RelationshipsAsParent.Select(p => p.ChildEntity))
                .SingleOrDefault(o => o.EntityId == id);

            if (entity == null)
                return NotFound();

            var result = Models.Code.RunDeployment(DbContext, entity, deploymentOptions);

            if (result != null) return BadRequest(result);

            return Code(id);
        }

        public class ApiCodeResult
        {
            public string Model { get; set; }
            public string Enums { get; set; }
            public string DTO { get; set; }
            public string SettingsDTO { get; set; }
            public string DbContext { get; set; }
            public string Controller { get; set; }
            public string BundleConfig { get; set; }
            public string AppRouter { get; set; }
            public string ApiResource { get; set; }
            public string ListHtml { get; set; }
            public string ListTypeScript { get; set; }
            public string EditHtml { get; set; }
            public string EditTypeScript { get; set; }
        }
    }

    public class OrderedIds
    {
        public Guid[] ids { get; set; }
    }
}

