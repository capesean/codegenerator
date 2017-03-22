using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB.Controllers
{
    [Authorize, RoutePrefix("api/relationshipfields")]
    public class RelationshipFieldsController : BaseApiController
    {
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Search([FromUri]PagingOptions pagingOptions, [FromUri]Guid? relationshipId = null, [FromUri]Guid? parentFieldId = null, [FromUri]Guid? childFieldId = null)
        {
            IQueryable<RelationshipField> results = DbContext.RelationshipFields;
            results = results.Include(o => o.ChildField.Entity.Project);
            results = results.Include(o => o.ChildField.Lookup.Project);
            results = results.Include(o => o.ParentField.Entity.Project);
            results = results.Include(o => o.ParentField.Lookup.Project);
            results = results.Include(o => o.Relationship.ChildEntity.Project);
            results = results.Include(o => o.Relationship.ParentEntity.Project);
            results = results.Include(o => o.Relationship.ParentField.Entity.Project);
            results = results.Include(o => o.Relationship.ParentField.Lookup.Project);

            if (relationshipId.HasValue) results = results.Where(o => o.RelationshipId == relationshipId);
            if (parentFieldId.HasValue) results = results.Where(o => o.ParentFieldId == parentFieldId);
            if (childFieldId.HasValue) results = results.Where(o => o.ChildFieldId == childFieldId);

            results = results.OrderBy(o => o.RelationshipFieldId);

            return Ok((await GetPaginatedResponse(results, pagingOptions)).Select(o => ModelFactory.Create(o)));
        }

        [HttpGet, Route("{relationshipFieldId:Guid}")]
        public async Task<IHttpActionResult> Get(Guid relationshipFieldId)
        {
            var relationshipfield = await DbContext.RelationshipFields
                .Include(o => o.ChildField.Entity.Project)
                .Include(o => o.ChildField.Lookup.Project)
                .Include(o => o.ParentField.Entity.Project)
                .Include(o => o.ParentField.Lookup.Project)
                .Include(o => o.Relationship.ChildEntity.Project)
                .Include(o => o.Relationship.ParentEntity.Project)
                .Include(o => o.Relationship.ParentField.Entity.Project)
                .Include(o => o.Relationship.ParentField.Lookup.Project)
                .SingleOrDefaultAsync(o => o.RelationshipFieldId == relationshipFieldId);

            if (relationshipfield == null)
                return NotFound();

            return Ok(ModelFactory.Create(relationshipfield));
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Insert([FromBody]RelationshipFieldDTO relationshipFieldDTO)
        {
            if (relationshipFieldDTO.RelationshipFieldId != Guid.Empty) return BadRequest("Invalid RelationshipFieldId");

            return await Save(relationshipFieldDTO);
        }

        [HttpPost, Route("{relationshipFieldId:Guid}")]
        public async Task<IHttpActionResult> Update(Guid relationshipFieldId, [FromBody]RelationshipFieldDTO relationshipFieldDTO)
        {
            if (relationshipFieldDTO.RelationshipFieldId != relationshipFieldId) return BadRequest("Id mismatch");

            return await Save(relationshipFieldDTO);
        }

        private async Task<IHttpActionResult> Save(RelationshipFieldDTO relationshipFieldDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isNew = relationshipFieldDTO.RelationshipFieldId == Guid.Empty;

            RelationshipField relationshipField;
            if (isNew)
            {
                relationshipField = new RelationshipField();
                DbContext.Entry(relationshipField).State = EntityState.Added;
            }
            else
            {
                relationshipField = await DbContext.RelationshipFields.SingleOrDefaultAsync(o => o.RelationshipFieldId == relationshipFieldDTO.RelationshipFieldId);

                if (relationshipField == null)
                    return NotFound();

                DbContext.Entry(relationshipField).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(relationshipField, relationshipFieldDTO);

            await DbContext.SaveChangesAsync();

            return await Get(relationshipField.RelationshipFieldId);
        }

        [HttpDelete, Route("{relationshipFieldId:Guid}")]
        public async Task<IHttpActionResult> Delete(Guid relationshipFieldId)
        {
            var relationshipfield = await DbContext.RelationshipFields.SingleOrDefaultAsync(o => o.RelationshipFieldId == relationshipFieldId);

            if (relationshipfield == null)
                return NotFound();

            DbContext.Entry(relationshipfield).State = EntityState.Deleted;

            await DbContext.SaveChangesAsync();

            return Ok();
        }

    }
}
