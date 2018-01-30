using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB.Controllers
{
    [Authorize, RoutePrefix("api/fields")]
    public class FieldsController : BaseApiController
    {
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Search([FromUri]PagingOptions pagingOptions, [FromUri]string q = null, [FromUri]Guid? entityId = null, [FromUri]Guid? lookupId = null)
        {
            IQueryable<Field> results = DbContext.Fields;
            if (pagingOptions.IncludeEntities)
            {
                results = results.Include(o => o.Lookup.Project);
                results = results.Include(o => o.Entity.Project);
            }

            if (!string.IsNullOrWhiteSpace(q))
                results = results.Where(o => o.Name.Contains(q));

            if (entityId.HasValue) results = results.Where(o => o.EntityId == entityId);
            if (lookupId.HasValue) results = results.Where(o => o.LookupId == lookupId);

            results = results.OrderBy(o => o.FieldOrder);

            return Ok((await GetPaginatedResponse(results, pagingOptions)).Select(o => ModelFactory.Create(o)));
        }

        [HttpGet, Route("{fieldId:Guid}")]
        public async Task<IHttpActionResult> Get(Guid fieldId)
        {
            var field = await DbContext.Fields
                .Include(o => o.Lookup.Project)
                .Include(o => o.Entity.Project)
                .SingleOrDefaultAsync(o => o.FieldId == fieldId);

            if (field == null)
                return NotFound();

            return Ok(ModelFactory.Create(field));
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Insert([FromBody]FieldDTO fieldDTO)
        {
            if (fieldDTO.FieldId != Guid.Empty) return BadRequest("Invalid FieldId");

            return await Save(fieldDTO);
        }

        [HttpPost, Route("{fieldId:Guid}")]
        public async Task<IHttpActionResult> Update(Guid fieldId, [FromBody]FieldDTO fieldDTO)
        {
            if (fieldDTO.FieldId != fieldId) return BadRequest("Id mismatch");

            return await Save(fieldDTO);
        }

        private async Task<IHttpActionResult> Save(FieldDTO fieldDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isNew = fieldDTO.FieldId == Guid.Empty;

            Field field;
            if (isNew)
            {
                field = new Field();

                fieldDTO.FieldOrder = (await DbContext.Fields.Where(o => o.EntityId == fieldDTO.EntityId).MaxAsync(o => (int?)o.FieldOrder) ?? 0) + 1;

                DbContext.Entry(field).State = EntityState.Added;
                field.FieldOrder = (DbContext.Fields.Where(f => f.EntityId == fieldDTO.EntityId).Max(f => (byte?)(f.FieldOrder + 1)) ?? 0);
            }
            else
            {
                field = await DbContext.Fields.SingleOrDefaultAsync(o => o.FieldId == fieldDTO.FieldId);

                if (field == null)
                    return NotFound();

                DbContext.Entry(field).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(field, fieldDTO);

            await DbContext.SaveChangesAsync();

            return await Get(field.FieldId);
        }

        [HttpDelete, Route("{fieldId:Guid}")]
        public async Task<IHttpActionResult> Delete(Guid fieldId)
        {
            var field = await DbContext.Fields.SingleOrDefaultAsync(o => o.FieldId == fieldId);

            if (field == null)
                return NotFound();

            if (DbContext.RelationshipFields.Any(o => o.ChildFieldId == field.FieldId))
                return BadRequest("Unable to delete the field as it has related relationship fields");

            if (DbContext.RelationshipFields.Any(o => o.ParentFieldId == field.FieldId))
                return BadRequest("Unable to delete the field as it has related relationship fields");

            if (DbContext.Relationships.Any(o => o.ParentFieldId == field.FieldId))
                return BadRequest("Unable to delete the field as it has related relationships");

            DbContext.Entry(field).State = EntityState.Deleted;

            await DbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost, Route("sort")]
        public async Task<IHttpActionResult> Sort([FromBody]SortedGuids sortedIds)
        {
            var fields = await DbContext.Fields.ToListAsync();
            if (fields.Count != sortedIds.ids.Length) return BadRequest("Some of the fields could not be found");

            var sortOrder = 0;
            foreach (var field in fields)
            {
                DbContext.Entry(field).State = EntityState.Modified;
                field.FieldOrder = Array.IndexOf(sortedIds.ids, field.FieldId);
                sortOrder++;
            }

            await DbContext.SaveChangesAsync();

            return Ok();
        }

    }
}
