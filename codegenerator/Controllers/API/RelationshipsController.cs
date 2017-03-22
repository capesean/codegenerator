using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB.Controllers
{
    [Authorize, RoutePrefix("api/relationships")]
    public class RelationshipsController : BaseApiController
    {
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Search([FromUri]PagingOptions pagingOptions, [FromUri]Guid? parentEntityId = null, [FromUri]Guid? childEntityId = null, [FromUri]Guid? parentFieldId = null)
        {
            IQueryable<Relationship> results = DbContext.Relationships;
            if (pagingOptions.IncludeEntities)
            {
                results = results.Include(o => o.ChildEntity.Project);
                results = results.Include(o => o.ParentEntity.Project);
                results = results.Include(o => o.ParentField.Entity.Project);
                results = results.Include(o => o.ParentField.Lookup.Project);
            }

            if (parentEntityId.HasValue) results = results.Where(o => o.ParentEntityId == parentEntityId);
            if (childEntityId.HasValue) results = results.Where(o => o.ChildEntityId == childEntityId);
            if (parentFieldId.HasValue) results = results.Where(o => o.ParentFieldId == parentFieldId);

            results = results.OrderBy(o => o.SortOrder);

            return Ok((await GetPaginatedResponse(results, pagingOptions)).Select(o => ModelFactory.Create(o)));
        }

        [HttpGet, Route("{relationshipId:Guid}")]
        public async Task<IHttpActionResult> Get(Guid relationshipId)
        {
            var relationship = await DbContext.Relationships
                .Include(o => o.ChildEntity.Project)
                .Include(o => o.ParentEntity.Project)
                .Include(o => o.ParentField.Entity.Project)
                .Include(o => o.ParentField.Lookup.Project)
                .SingleOrDefaultAsync(o => o.RelationshipId == relationshipId);

            if (relationship == null)
                return NotFound();

            return Ok(ModelFactory.Create(relationship));
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Insert([FromBody]RelationshipDTO relationshipDTO)
        {
            if (relationshipDTO.RelationshipId != Guid.Empty) return BadRequest("Invalid RelationshipId");

            return await Save(relationshipDTO);
        }

        [HttpPost, Route("{relationshipId:Guid}")]
        public async Task<IHttpActionResult> Update(Guid relationshipId, [FromBody]RelationshipDTO relationshipDTO)
        {
            if (relationshipDTO.RelationshipId != relationshipId) return BadRequest("Id mismatch");

            return await Save(relationshipDTO);
        }

        private async Task<IHttpActionResult> Save(RelationshipDTO relationshipDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isNew = relationshipDTO.RelationshipId == Guid.Empty;

            Relationship relationship;
            if (isNew)
            {
                relationship = new Relationship();

                relationshipDTO.SortOrder = (await DbContext.Relationships.MaxAsync(o => (int?)o.SortOrder) ?? 0) + 1;

                DbContext.Entry(relationship).State = EntityState.Added;
            }
            else
            {
                relationship = await DbContext.Relationships.SingleOrDefaultAsync(o => o.RelationshipId == relationshipDTO.RelationshipId);

                if (relationship == null)
                    return NotFound();

                DbContext.Entry(relationship).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(relationship, relationshipDTO);

            await DbContext.SaveChangesAsync();

            return await Get(relationship.RelationshipId);
        }

        [HttpDelete, Route("{relationshipId:Guid}")]
        public async Task<IHttpActionResult> Delete(Guid relationshipId)
        {
            var relationship = await DbContext.Relationships.SingleOrDefaultAsync(o => o.RelationshipId == relationshipId);

            if (relationship == null)
                return NotFound();

            if (DbContext.RelationshipFields.Any(o => o.RelationshipId == relationship.RelationshipId))
                return BadRequest("Unable to delete the relationship as it has related relationship fields");

            DbContext.Entry(relationship).State = EntityState.Deleted;

            await DbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost, Route("sort")]
        public async Task<IHttpActionResult> Sort([FromBody]SortedGuids sortedIds)
        {
            var sortOrder = 0;
            foreach (var id in sortedIds.ids)
            {
                var item = await DbContext.Relationships.SingleOrDefaultAsync(o => o.RelationshipId == id);

                if (item == null) return BadRequest("One of the relationships could not be found");

                DbContext.Entry(item).State = EntityState.Modified;
                item.SortOrder = sortOrder;
                sortOrder++;
            }

            await DbContext.SaveChangesAsync();

            return Ok();
        }

    }
}
