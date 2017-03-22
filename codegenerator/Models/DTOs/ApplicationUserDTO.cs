using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class ApplicationUserDTO
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string FullName { get; set; }

        public IList<Guid> RoleIds { get; set; }
    }

    public class PasswordResetDTO
    {
        [Display(Name = "Token")]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public partial class ModelFactory
    {
        public ApplicationUserDTO Create(ApplicationUser user)
        {
            var roleIds = new List<Guid>();
            foreach (var role in user.Roles)
                roleIds.Add(role.RoleId);

            return new ApplicationUserDTO
            {
                Id = user.Id,
                Enabled = user.Enabled,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleIds = roleIds,
                FullName = user.FullName
            };
        }

        public void Hydrate(ApplicationUser user, ApplicationUserDTO userDTO)
        {
            user.UserName = userDTO.Email;
            user.Email = userDTO.Email;
            user.Enabled = userDTO.Enabled;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
        }
    }
}