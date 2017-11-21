using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using WEB.Models;

namespace WEB.Controllers
{
    [Authorize, RoutePrefix("api/projects")]
    public class ProjectsController : BaseApiController
    {
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Search([FromUri]PagingOptions pagingOptions, [FromUri]string q = null)
        {
            IQueryable<Project> results = DbContext.Projects;

            if (!string.IsNullOrWhiteSpace(q))
                results = results.Where(o => o.Name.Contains(q));

            results = results.OrderBy(o => o.Name);

            return Ok((await GetPaginatedResponse(results, pagingOptions)).Select(o => ModelFactory.Create(o)));
        }

        [HttpGet, Route("{projectId:Guid}")]
        public async Task<IHttpActionResult> Get(Guid projectId)
        {
            var project = await DbContext.Projects
                .SingleOrDefaultAsync(o => o.ProjectId == projectId);

            if (project == null)
                return NotFound();

            return Ok(ModelFactory.Create(project));
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Insert([FromBody]ProjectDTO projectDTO)
        {
            if (projectDTO.ProjectId != Guid.Empty) return BadRequest("Invalid ProjectId");

            return await Save(projectDTO);
        }

        [HttpPost, Route("{projectId:Guid}")]
        public async Task<IHttpActionResult> Update(Guid projectId, [FromBody]ProjectDTO projectDTO)
        {
            if (projectDTO.ProjectId != projectId) return BadRequest("Id mismatch");

            return await Save(projectDTO);
        }

        private async Task<IHttpActionResult> Save(ProjectDTO projectDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (DbContext.Projects.Any(o => o.Name == projectDTO.Name && !(o.ProjectId == projectDTO.ProjectId)))
                return BadRequest("Name already exists.");

            var isNew = projectDTO.ProjectId == Guid.Empty;

            Project project;
            if (isNew)
            {
                project = new Project();

                DbContext.Entry(project).State = EntityState.Added;
            }
            else
            {
                project = await DbContext.Projects.SingleOrDefaultAsync(o => o.ProjectId == projectDTO.ProjectId);

                if (project == null)
                    return NotFound();

                DbContext.Entry(project).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(project, projectDTO);

            await DbContext.SaveChangesAsync();

            return await Get(project.ProjectId);
        }

        [HttpDelete, Route("{projectId:Guid}")]
        public async Task<IHttpActionResult> Delete(Guid projectId)
        {
            var project = await DbContext.Projects.SingleOrDefaultAsync(o => o.ProjectId == projectId);

            if (project == null)
                return NotFound();

            if (DbContext.Entities.Any(o => o.ProjectId == project.ProjectId))
                return BadRequest("Unable to delete the project as it has related entities");

            if (DbContext.Lookups.Any(o => o.ProjectId == project.ProjectId))
                return BadRequest("Unable to delete the project as it has related lookups");

            DbContext.Entry(project).State = EntityState.Deleted;

            await DbContext.SaveChangesAsync();

            return Ok();
        }

    }
}
