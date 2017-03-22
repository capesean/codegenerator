using System;
using System.Web.Mvc;

namespace WEB.Controllers.MVC
{
    public class ErrorController : Controller
    {
        public ActionResult Index(int? id)
        {
            if(id.HasValue) Response.StatusCode = Convert.ToInt32(id);
            return View();
        }
    }
}
