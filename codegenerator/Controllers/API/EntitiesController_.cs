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
                .Include(o => o.RelationshipsAsChild)
                .Include(o => o.RelationshipsAsParent)
                .SingleOrDefault(o => o.EntityId == id);

            if (entity == null)
                return NotFound();

            var code = new Code(entity, DbContext);

            var error = code.Validate();
            if (error != null)
                return BadRequest(error);

            var result = new ApiCodeResult();

            result.Model = code.GenerateModel();
            result.Enums = code.GenerateEnums();
            result.DTO = code.GenerateDTO();
            result.SettingsDTO = code.GenerateSettingsDTO();
            result.DbContext = code.GenerateDbContext();
            result.Controller = code.GenerateController();
            result.BundleConfig = code.GenerateBundleConfig();
            result.AppRouter = code.GenerateAppRouter();
            result.ApiResource = code.GenerateApiResource();
            result.ListHtml = code.GenerateListHtml();
            result.ListTypeScript = code.GenerateListTypeScript();
            result.EditHtml = code.GenerateEditHtml();
            result.EditTypeScript = code.GenerateEditTypeScript();

            return Ok(result);
        }

        [HttpPost, Route("{id:Guid}/code")]
        public IHttpActionResult Deploy(Guid id, [FromBody]DeploymentOptions deploymentOptions)
        {
            var entity = DbContext.Entities
                .Include(o => o.Project)
                .Include(o => o.Fields)
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

