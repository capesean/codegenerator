using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WEB.Models;
using System.Net.Mail;
using System.Configuration;
using System.Linq;
using WEB.Utilities;

namespace WEB.Controllers
{
    [Authorize]
    public class AccountController : BaseMvcController
    {
        private ApplicationSignInManager _signInManager;

        public AccountController()
        {
        }

        public AccountController(AppUserManager userManager, ApplicationSignInManager signInManager)
        {
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        //
        // GET: /login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            // todo: should check the hostname here that it exists in the current valid clients list (central database)
            // todo: also check in other account options, such as password reset, etc. (so it doesn't create db on a request)
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, true, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login.");
                    return View(model);
            }
        }

        //
        // GET: /resetpassword
        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        // POST: /resetpassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //todo: html email
            var user = await UserManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email address.");
                return View(model);
            }

            // todo: locked out & not active:
            //if (!user.LockoutEndDateUtc)
            //{
            //    ModelState.AddModelError("", "Invalid email address.");
            //    return View(model);
            //}

            var provider = new MachineKeyProtectionProvider();
            UserManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, Guid>(provider.Create("PasswordReset"));
            var resetToken = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            var rootUrl = ConfigurationManager.AppSettings["RootUrl"];
            var message = new MailMessage();
            message.To.Add(new MailAddress(user.Email));
            message.Subject = "Password Reset";
            message.Body = user.FirstName + Environment.NewLine;
            message.Body += Environment.NewLine;
            message.Body += "A password reset has been requested. Please use the link below to reset your password." + Environment.NewLine;
            message.Body += Environment.NewLine;
            message.Body += rootUrl + "reset?e=" + user.Email + "&t=" + HttpUtility.UrlEncode(resetToken) + Environment.NewLine;

            Email.SendMail(message, Settings);

            return RedirectToAction("Login", new { msg = "tokensent" });

        }

        //
        // GET: /reset
        [AllowAnonymous]
        public ActionResult Reset()
        {
            var model = new ResetViewModel { Email = Request.QueryString["e"], Token = Request.QueryString["t"] };
            return View(model);
        }

        // POST: /reset
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reset(ResetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("lName", "Last Name  not found");
                return View(model);
            }

            var provider = new MachineKeyProtectionProvider();
            UserManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, Guid>(provider.Create("PasswordReset"));
            var user = await UserManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email address.");
                return View(model);
            }

            // todo: locked out & not active:
            //if (!user.LockoutEndDateUtc)
            //{
            //    ModelState.AddModelError("", "Invalid email address.");
            //    return View(model);
            //}

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Token, model.Password);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.Errors.First());
                return View(model);
            }

            var message = new MailMessage();
            message.To.Add(new MailAddress(user.Email));
            message.Subject = "Password Changed";
            message.Body = user.FirstName + Environment.NewLine;
            message.Body += Environment.NewLine;
            message.Body += "Your password has been changed." + Environment.NewLine;

            Utilities.Email.SendMail(message, Settings);

            if (!user.EmailConfirmed) user.EmailConfirmed = true;
            await UserManager.UpdateAsync(user);

            return RedirectToAction("Login", new { msg = "passwordchanged" });
        }

        //
        // GET: /logout
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToLocal("/login?msg=loggedout");
        }

        #region Helpers
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}