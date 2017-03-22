using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WEB.Models
{
    public partial class SettingsDTO
    {
        [MaxLength(50)]
        public string RootUrl { get; set; }
        public List<RoleDTO> Roles { get; set; } = new List<RoleDTO>();
        public bool IsLocal { get; set; }
    }

    public class EnumDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
    }

    public class RoleDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public partial class ModelFactory
    {
        public static SettingsDTO Create(Settings settings, ApplicationDbContext dbContext)
        {
            var dto = new SettingsDTO
            {
                RootUrl = settings.RootUrl,
                IsLocal = System.Web.HttpContext.Current.Request.IsLocal
            };

            // add roles
            using (var roleStore = new RoleStore<AppRole, Guid, AppUserRole>(dbContext))
            using (var roleManager = new RoleManager<AppRole, Guid>(roleStore))
            {
                foreach (var role in roleManager.Roles.ToList())
                {
                    Roles roleEnum;
                    var parseResult = Enum.TryParse(role.Name, out roleEnum);
                    if (!parseResult) throw new InvalidCastException("Invalid role in SettingsDTO.Create: " + role.Name);

                    var roleDTO = new RoleDTO { Id = role.Id, Name = roleEnum.ToString() };
                    // override role names with friendly names here, e.g. roleDTO.Name = "Project Manager";

                    dto.Roles.Add(roleDTO);
                }
            }

            return dto;
        }

        public void Hydrate(Settings settings, SettingsDTO settingsDTO)
        {
        }
    }
}