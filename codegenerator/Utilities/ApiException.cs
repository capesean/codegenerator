using System.Web;
using System.Web.Http.Filters;

namespace WEB.Utilities
{
    public class ApiException : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            ErrorLogger.Log(context.Exception, HttpContext.Current.Request, HttpContext.Current.Request.Url.ToString(), HttpContext.Current.User.Identity.Name);
        }
    }
}
