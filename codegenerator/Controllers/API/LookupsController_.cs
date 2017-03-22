using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace WEB.Controllers
{
    public partial class LookupsController
    {
        [HttpPost, Route("{id:Guid}/updateorders")]
        public async Task<IHttpActionResult> UpdateOrders(Guid id, [FromBody]OrderedIds newOrders)
        {
            var lookup = await DbContext.Lookups.Include(o => o.LookupOptions).SingleOrDefaultAsync(o => o.LookupId == id);
            if (lookup == null)
                return NotFound();

            var newOrder = (byte)0;
            foreach (var itemId in newOrders.ids)
            {
                var option = lookup.LookupOptions.SingleOrDefault(o => o.LookupOptionId == itemId);

                if (option == null)
                    return BadRequest("Key was not found: " + itemId);

                DbContext.Entry(option).State = EntityState.Modified;
                option.SortOrder = newOrder;
                newOrder++;
            }

            DbContext.SaveChanges();

            return Ok();
        }

        public class OrderedIds
        {
            public Guid[] ids { get; set; }
        }
    }
}