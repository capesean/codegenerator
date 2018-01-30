using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class ProjectDTO
    {
        [Required]
        public Guid ProjectId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string Name { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(250)]
        public string RootPath { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(20)]
        public string Namespace { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(20)]
        public string AngularModuleName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(20)]
        public string AngularDirectivePrefix { get; set; }

        [Required]
        public bool Bootstrap3 { get; set; }

        [Required]
        public bool ExcludeTypes { get; set; }

        [MaxLength(50)]
        public string UrlPrefix { get; set; }

        [Required]
        public bool UseStringAuthorizeAttributes { get; set; }

    }

    public partial class ModelFactory
    {
        public ProjectDTO Create(Project project)
        {
            if (project == null) return null;

            var projectDTO = new ProjectDTO();

            projectDTO.ProjectId = project.ProjectId;
            projectDTO.Name = project.Name;
            projectDTO.RootPath = project.RootPath;
            projectDTO.Namespace = project.Namespace;
            projectDTO.AngularModuleName = project.AngularModuleName;
            projectDTO.AngularDirectivePrefix = project.AngularDirectivePrefix;
            projectDTO.Bootstrap3 = project.Bootstrap3;
            projectDTO.ExcludeTypes = project.ExcludeTypes;
            projectDTO.UrlPrefix = project.UrlPrefix;
            projectDTO.UseStringAuthorizeAttributes = project.UseStringAuthorizeAttributes;

            return projectDTO;
        }

        public void Hydrate(Project project, ProjectDTO projectDTO)
        {
            project.Name = projectDTO.Name;
            project.RootPath = projectDTO.RootPath;
            project.Namespace = projectDTO.Namespace;
            project.AngularModuleName = projectDTO.AngularModuleName;
            project.AngularDirectivePrefix = projectDTO.AngularDirectivePrefix;
            project.Bootstrap3 = projectDTO.Bootstrap3;
            project.ExcludeTypes = projectDTO.ExcludeTypes;
            project.UrlPrefix = projectDTO.UrlPrefix;
            project.UseStringAuthorizeAttributes = projectDTO.UseStringAuthorizeAttributes;
        }
    }
}
