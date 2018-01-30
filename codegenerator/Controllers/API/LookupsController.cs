using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB.Controllers
{
    [Authorize, RoutePrefix("api/lookups")]
    public partial class LookupsController : BaseApiController
    {
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Search([FromUri]PagingOptions pagingOptions, [FromUri]string q = null, [FromUri]Guid? projectId = null)
        {
            IQueryable<Lookup> results = DbContext.Lookups;
            if (pagingOptions.IncludeEntities)
            {
                results = results.Include(o => o.Project);
            }

            if (!string.IsNullOrWhiteSpace(q))
                results = results.Where(o => o.Name.Contains(q));

            if (projectId.HasValue) results = results.Where(o => o.ProjectId == projectId);

            results = results.OrderBy(o => o.Name);

            return Ok((await GetPaginatedResponse(results, pagingOptions)).Select(o => ModelFactory.Create(o)));
        }

        [HttpGet, Route("{lookupId:Guid}")]
        public async Task<IHttpActionResult> Get(Guid lookupId)
        {
            var lookup = await DbContext.Lookups
                .Include(o => o.Project)
                .SingleOrDefaultAsync(o => o.LookupId == lookupId);

            if (lookup == null)
                return NotFound();

            return Ok(ModelFactory.Create(lookup));
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Insert([FromBody]LookupDTO lookupDTO)
        {
            if (lookupDTO.LookupId != Guid.Empty) return BadRequest("Invalid LookupId");

            return await Save(lookupDTO);
        }

        [HttpPost, Route("{lookupId:Guid}")]
        public async Task<IHttpActionResult> Update(Guid lookupId, [FromBody]LookupDTO lookupDTO)
        {
            if (lookupDTO.LookupId != lookupId) return BadRequest("Id mismatch");

            return await Save(lookupDTO);
        }

        private async Task<IHttpActionResult> Save(LookupDTO lookupDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isNew = lookupDTO.LookupId == Guid.Empty;

            Lookup lookup;
            if (isNew)
            {
                lookup = new Lookup();

                DbContext.Entry(lookup).State = EntityState.Added;
            }
            else
            {
                lookup = await DbContext.Lookups.SingleOrDefaultAsync(o => o.LookupId == lookupDTO.LookupId);

                if (lookup == null)
                    return NotFound();

                DbContext.Entry(lookup).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(lookup, lookupDTO);

            await DbContext.SaveChangesAsync();

            return await Get(lookup.LookupId);
        }

        [HttpDelete, Route("{lookupId:Guid}")]
        public async Task<IHttpActionResult> Delete(Guid lookupId)
        {
            var lookup = await DbContext.Lookups.SingleOrDefaultAsync(o => o.LookupId == lookupId);

            if (lookup == null)
                return NotFound();

            if (DbContext.LookupOptions.Any(o => o.LookupId == lookup.LookupId))
                return BadRequest("Unable to delete the lookup as it has related lookup options");

            if (DbContext.Fields.Any(o => o.LookupId == lookup.LookupId))
                return BadRequest("Unable to delete the lookup as it has related fields");

            DbContext.Entry(lookup).State = EntityState.Deleted;

            await DbContext.SaveChangesAsync();

            return Ok();
        }

    }
}
