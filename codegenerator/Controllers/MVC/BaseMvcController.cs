using WEB.Models;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Mvc;

namespace WEB.Controllers
{
    [HandleError]
    public class BaseMvcController : Controller
    {
        private AppUserManager _userManager;
        private ApplicationDbContext _dbContext;
        private ApplicationUser _currentUser;
        private Settings _settings;
        internal AppUserManager UserManager
        {
            get
            {
                if (_userManager == null) _userManager = HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
                return _userManager;
            }
        }
        internal ApplicationDbContext DbContext
        {
            get
            {
                if (_dbContext == null) _dbContext = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
                return _dbContext;
            }
        }
        internal Settings Settings
        {
            get
            {
                if (_settings == null) _settings = new Settings(DbContext);
                return _settings;
            }
        }
        internal ApplicationUser CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = UserManager.FindByNameAsync(User.Identity.Name).Result;
                return _currentUser;
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Utilities.ErrorLogger.Log(filterContext.Exception, System.Web.HttpContext.Current.Request, filterContext.HttpContext.Request.Url.ToString(), filterContext.HttpContext.User.Identity.Name);
            base.OnException(filterContext);
        }
    }
}