using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB.Controllers
{
    [Authorize, RoutePrefix("api/codereplacements")]
    public class CodeReplacementsController : BaseApiController
    {
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Search([FromUri]PagingOptions pagingOptions, [FromUri]string q = null, [FromUri]Guid? entityId = null, [FromUri]string findCode = null, [FromUri]string replacementCode = null)
        {
            IQueryable<CodeReplacement> results = DbContext.CodeReplacements;
            results = results.Include(o => o.Entity.Project);

            if (!string.IsNullOrWhiteSpace(q))
                results = results.Where(o => o.Purpose.Contains(q));

            if (entityId.HasValue) results = results.Where(o => o.EntityId == entityId);
            if (findCode != null) results = results.Where(o => o.FindCode == findCode);
            if (replacementCode != null) results = results.Where(o => o.ReplacementCode == replacementCode);

            results = results.OrderBy(o => o.EntityId).ThenBy(o => o.SortOrder).ThenBy(o => o.CodeType);

            return Ok((await GetPaginatedResponse(results, pagingOptions)).Select(o => ModelFactory.Create(o)));
        }

        [HttpGet, Route("{codeReplacementId:Guid}")]
        public async Task<IHttpActionResult> Get(Guid codeReplacementId)
        {
            var codereplacement = await DbContext.CodeReplacements
                .Include(o => o.Entity.Project)
                .SingleOrDefaultAsync(o => o.CodeReplacementId == codeReplacementId);

            if (codereplacement == null)
                return NotFound();

            return Ok(ModelFactory.Create(codereplacement));
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Insert([FromBody]CodeReplacementDTO codeReplacementDTO)
        {
            if (codeReplacementDTO.CodeReplacementId != Guid.Empty) return BadRequest("Invalid CodeReplacementId");

            return await Save(codeReplacementDTO);
        }

        [HttpPost, Route("{codeReplacementId:Guid}")]
        public async Task<IHttpActionResult> Update(Guid codeReplacementId, [FromBody]CodeReplacementDTO codeReplacementDTO)
        {
            if (codeReplacementDTO.CodeReplacementId != codeReplacementId) return BadRequest("Id mismatch");

            return await Save(codeReplacementDTO);
        }

        private async Task<IHttpActionResult> Save(CodeReplacementDTO codeReplacementDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isNew = codeReplacementDTO.CodeReplacementId == Guid.Empty;

            CodeReplacement codeReplacement;
            if (isNew)
            {
                codeReplacement = new CodeReplacement();
                DbContext.Entry(codeReplacement).State = EntityState.Added;
                codeReplacementDTO.SortOrder = (await DbContext.CodeReplacements.Where(o => o.EntityId == codeReplacementDTO.EntityId).MaxAsync(o => (int?)o.SortOrder) ?? 0) + 1;
            }
            else
            {
                codeReplacement = await DbContext.CodeReplacements.SingleOrDefaultAsync(o => o.CodeReplacementId == codeReplacementDTO.CodeReplacementId);

                if (codeReplacement == null)
                    return NotFound();

                DbContext.Entry(codeReplacement).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(codeReplacement, codeReplacementDTO);

            await DbContext.SaveChangesAsync();

            return await Get(codeReplacement.CodeReplacementId);
        }

        [HttpDelete, Route("{codeReplacementId:Guid}")]
        public async Task<IHttpActionResult> Delete(Guid codeReplacementId)
        {
            var codeReplacement = await DbContext.CodeReplacements.SingleOrDefaultAsync(o => o.CodeReplacementId == codeReplacementId);

            if (codeReplacement == null)
                return NotFound();

            DbContext.Entry(codeReplacement).State = EntityState.Deleted;

            await DbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost, Route("sort")]
        public async Task<IHttpActionResult> Sort([FromBody]SortedGuids sortedIds)
        {
            var sortOrder = 0;
            foreach (var id in sortedIds.ids)
            {
                var item = await DbContext.CodeReplacements.SingleOrDefaultAsync(o => o.CodeReplacementId == id);

                if (item == null) return BadRequest("One of the code replacements could not be found");

                DbContext.Entry(item).State = EntityState.Modified;
                item.SortOrder = sortOrder;
                sortOrder++;
            }

            await DbContext.SaveChangesAsync();

            return Ok();
        }

    }
}
