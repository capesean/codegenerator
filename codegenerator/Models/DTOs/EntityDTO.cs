using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class EntityDTO
    {
        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string Name { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string PluralName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string FriendlyName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string PluralFriendlyName { get; set; }

        [Required]
        public EntityType EntityType { get; set; }

        [Required]
        public bool PartialEntityClass { get; set; }

        [Required]
        public bool PartialControllerClass { get; set; }

        [MaxLength(250)]
        public string Breadcrumb { get; set; }

        [MaxLength(100)]
        public string PreventModelDeployment { get; set; }

        [MaxLength(100)]
        public string PreventDTODeployment { get; set; }

        [MaxLength(100)]
        public string PreventDbContextDeployment { get; set; }

        [MaxLength(100)]
        public string PreventControllerDeployment { get; set; }

        [MaxLength(100)]
        public string PreventBundleConfigDeployment { get; set; }

        [MaxLength(100)]
        public string PreventAppRouterDeployment { get; set; }

        [MaxLength(100)]
        public string PreventApiResourceDeployment { get; set; }

        [MaxLength(100)]
        public string PreventListHtmlDeployment { get; set; }

        [MaxLength(100)]
        public string PreventListTypeScriptDeployment { get; set; }

        [MaxLength(100)]
        public string PreventEditHtmlDeployment { get; set; }

        [MaxLength(100)]
        public string PreventEditTypeScriptDeployment { get; set; }

        [MaxLength(100)]
        public string PreventAppSelectHtmlDeployment { get; set; }

        [MaxLength(100)]
        public string PreventAppSelectTypeScriptDeployment { get; set; }

        [MaxLength(100)]
        public string PreventSelectModalHtmlDeployment { get; set; }

        [MaxLength(100)]
        public string PreventSelectModalTypeScriptDeployment { get; set; }

        [Required]
        public AuthorizationType AuthorizationType { get; set; }

        [Required]
        public bool Exclude { get; set; }

        public Guid? PrimaryFieldId { get; set; }

        [MaxLength(20)]
        public string IconClass { get; set; }

        public FieldDTO PrimaryField { get; set; }

        public ProjectDTO Project { get; set; }

    }

    public partial class ModelFactory
    {
        public EntityDTO Create(Entity entity)
        {
            if (entity == null) return null;

            var entityDTO = new EntityDTO();

            entityDTO.EntityId = entity.EntityId;
            entityDTO.ProjectId = entity.ProjectId;
            entityDTO.Name = entity.Name;
            entityDTO.PluralName = entity.PluralName;
            entityDTO.FriendlyName = entity.FriendlyName;
            entityDTO.PluralFriendlyName = entity.PluralFriendlyName;
            entityDTO.EntityType = entity.EntityType;
            entityDTO.PartialEntityClass = entity.PartialEntityClass;
            entityDTO.PartialControllerClass = entity.PartialControllerClass;
            entityDTO.Breadcrumb = entity.Breadcrumb;
            entityDTO.PreventModelDeployment = entity.PreventModelDeployment;
            entityDTO.PreventDTODeployment = entity.PreventDTODeployment;
            entityDTO.PreventDbContextDeployment = entity.PreventDbContextDeployment;
            entityDTO.PreventControllerDeployment = entity.PreventControllerDeployment;
            entityDTO.PreventBundleConfigDeployment = entity.PreventBundleConfigDeployment;
            entityDTO.PreventAppRouterDeployment = entity.PreventAppRouterDeployment;
            entityDTO.PreventApiResourceDeployment = entity.PreventApiResourceDeployment;
            entityDTO.PreventListHtmlDeployment = entity.PreventListHtmlDeployment;
            entityDTO.PreventListTypeScriptDeployment = entity.PreventListTypeScriptDeployment;
            entityDTO.PreventEditHtmlDeployment = entity.PreventEditHtmlDeployment;
            entityDTO.PreventEditTypeScriptDeployment = entity.PreventEditTypeScriptDeployment;
            entityDTO.PreventAppSelectHtmlDeployment = entity.PreventAppSelectHtmlDeployment;
            entityDTO.PreventAppSelectTypeScriptDeployment = entity.PreventAppSelectTypeScriptDeployment;
            entityDTO.PreventSelectModalHtmlDeployment = entity.PreventSelectModalHtmlDeployment;
            entityDTO.PreventSelectModalTypeScriptDeployment = entity.PreventSelectModalTypeScriptDeployment;
            entityDTO.AuthorizationType = entity.AuthorizationType;
            entityDTO.Exclude = entity.Exclude;
            entityDTO.PrimaryFieldId = entity.PrimaryFieldId;
            entityDTO.IconClass = entity.IconClass;
            entityDTO.PrimaryField = null;
            entityDTO.Project = Create(entity.Project);

            return entityDTO;
        }

        public void Hydrate(Entity entity, EntityDTO entityDTO)
        {
            entity.ProjectId = entityDTO.ProjectId;
            entity.Name = entityDTO.Name;
            entity.PluralName = entityDTO.PluralName;
            entity.FriendlyName = entityDTO.FriendlyName;
            entity.PluralFriendlyName = entityDTO.PluralFriendlyName;
            entity.EntityType = entityDTO.EntityType;
            entity.PartialEntityClass = entityDTO.PartialEntityClass;
            entity.PartialControllerClass = entityDTO.PartialControllerClass;
            entity.Breadcrumb = entityDTO.Breadcrumb;
            entity.PreventModelDeployment = entityDTO.PreventModelDeployment;
            entity.PreventDTODeployment = entityDTO.PreventDTODeployment;
            entity.PreventDbContextDeployment = entityDTO.PreventDbContextDeployment;
            entity.PreventControllerDeployment = entityDTO.PreventControllerDeployment;
            entity.PreventBundleConfigDeployment = entityDTO.PreventBundleConfigDeployment;
            entity.PreventAppRouterDeployment = entityDTO.PreventAppRouterDeployment;
            entity.PreventApiResourceDeployment = entityDTO.PreventApiResourceDeployment;
            entity.PreventListHtmlDeployment = entityDTO.PreventListHtmlDeployment;
            entity.PreventListTypeScriptDeployment = entityDTO.PreventListTypeScriptDeployment;
            entity.PreventEditHtmlDeployment = entityDTO.PreventEditHtmlDeployment;
            entity.PreventEditTypeScriptDeployment = entityDTO.PreventEditTypeScriptDeployment;
            entity.PreventAppSelectHtmlDeployment = entityDTO.PreventAppSelectHtmlDeployment;
            entity.PreventAppSelectTypeScriptDeployment = entityDTO.PreventAppSelectTypeScriptDeployment;
            entity.PreventSelectModalHtmlDeployment = entityDTO.PreventSelectModalHtmlDeployment;
            entity.PreventSelectModalTypeScriptDeployment = entityDTO.PreventSelectModalTypeScriptDeployment;
            entity.AuthorizationType = entityDTO.AuthorizationType;
            entity.Exclude = entityDTO.Exclude;
            entity.PrimaryFieldId = entityDTO.PrimaryFieldId;
            entity.IconClass = entityDTO.IconClass;
        }
    }
}
