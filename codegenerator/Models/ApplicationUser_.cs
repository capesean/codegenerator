using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WEB.Models
{
    public partial class ApplicationUser : IdentityUser<Guid, AppUserLogin, AppUserRole, AppUserClaim>, IUser<Guid>
    {
        [NotMapped]
        public string FullName { get { return FirstName + " " + LastName; } }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, Guid> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            userIdentity.AddClaim(new Claim(ClaimTypes.Sid, Id.ToString()));

            return userIdentity;
        }
    }

    public class AppUserLogin : IdentityUserLogin<Guid> { }

    public class AppUserRole : IdentityUserRole<Guid> { }

    public class AppUserClaim : IdentityUserClaim<Guid> { }

    public class AppRole : IdentityRole<Guid, AppUserRole> { }

    public class AppClaimsPrincipal : ClaimsPrincipal
    {
        public AppClaimsPrincipal(ClaimsPrincipal principal) : base(principal)
        { }

        public Guid UserId
        {
            get { return Guid.Parse(this.FindFirst(ClaimTypes.Sid).Value); }
        }
    }

}