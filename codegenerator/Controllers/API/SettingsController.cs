using System.Web.Http;
using WEB.Models;
using System.Data.Entity;

namespace WEB.Controllers
{
    [Authorize, RoutePrefix("api/settings")]
    public class SettingsController : BaseApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            return Ok(ModelFactory.Create(Settings, DbContext));
        }

        [HttpPost, Route(""), Authorize(Roles = "Administrator")]
        public IHttpActionResult Post([FromBody]SettingsDTO settingsDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            DbContext.Entry(Settings).State = EntityState.Modified;

            ModelFactory.Hydrate(Settings, settingsDTO);

            DbContext.SaveChanges();

            return Ok(ModelFactory.Create(Settings, DbContext));
        }
    }
}