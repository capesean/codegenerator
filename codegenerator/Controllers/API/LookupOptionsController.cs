using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB.Controllers
{
    [Authorize, RoutePrefix("api/lookupoptions")]
    public class LookupOptionsController : BaseApiController
    {
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Search([FromUri]PagingOptions pagingOptions, [FromUri]Guid? lookupId = null, [FromUri]string name = null, [FromUri]string friendlyName = null)
        {
            IQueryable<LookupOption> results = DbContext.LookupOptions;
            results = results.Include(o => o.Lookup.Project);

            if (lookupId.HasValue) results = results.Where(o => o.LookupId == lookupId);
            if (name != null) results = results.Where(o => o.Name == name);
            if (friendlyName != null) results = results.Where(o => o.FriendlyName == friendlyName);

            results = results.OrderBy(o => o.SortOrder);

            return Ok((await GetPaginatedResponse(results, pagingOptions)).Select(o => ModelFactory.Create(o)));
        }

        [HttpGet, Route("{lookupOptionId:Guid}")]
        public async Task<IHttpActionResult> Get(Guid lookupOptionId)
        {
            var lookupoption = await DbContext.LookupOptions
                .Include(o => o.Lookup.Project)
                .SingleOrDefaultAsync(o => o.LookupOptionId == lookupOptionId);

            if (lookupoption == null)
                return NotFound();

            return Ok(ModelFactory.Create(lookupoption));
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Insert([FromBody]LookupOptionDTO lookupOptionDTO)
        {
            if (lookupOptionDTO.LookupOptionId != Guid.Empty) return BadRequest("Invalid LookupOptionId");

            return await Save(lookupOptionDTO);
        }

        [HttpPost, Route("{lookupOptionId:Guid}")]
        public async Task<IHttpActionResult> Update(Guid lookupOptionId, [FromBody]LookupOptionDTO lookupOptionDTO)
        {
            if (lookupOptionDTO.LookupOptionId != lookupOptionId) return BadRequest("Id mismatch");

            return await Save(lookupOptionDTO);
        }

        private async Task<IHttpActionResult> Save(LookupOptionDTO lookupOptionDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isNew = lookupOptionDTO.LookupOptionId == Guid.Empty;

            LookupOption lookupOption;
            if (isNew)
            {
                lookupOption = new LookupOption();
                DbContext.Entry(lookupOption).State = EntityState.Added;
                lookupOptionDTO.SortOrder = (DbContext.LookupOptions.Where(o => o.LookupId == lookupOptionDTO.LookupId).Max(o => (byte?)(o.SortOrder + 1)) ?? 0);
            }
            else
            {
                lookupOption = await DbContext.LookupOptions.SingleOrDefaultAsync(o => o.LookupOptionId == lookupOptionDTO.LookupOptionId);

                if (lookupOption == null)
                    return NotFound();

                DbContext.Entry(lookupOption).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(lookupOption, lookupOptionDTO);

            await DbContext.SaveChangesAsync();

            return await Get(lookupOption.LookupOptionId);
        }

        [HttpDelete, Route("{lookupOptionId:Guid}")]
        public async Task<IHttpActionResult> Delete(Guid lookupOptionId)
        {
            var lookupoption = await DbContext.LookupOptions.SingleOrDefaultAsync(o => o.LookupOptionId == lookupOptionId);

            if (lookupoption == null)
                return NotFound();

            DbContext.Entry(lookupoption).State = EntityState.Deleted;

            await DbContext.SaveChangesAsync();

            return Ok();
        }

    }
}
