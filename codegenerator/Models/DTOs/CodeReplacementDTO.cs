using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class CodeReplacementDTO
    {
        [Required]
        public Guid CodeReplacementId { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string Purpose { get; set; }

        [Required]
        public CodeType CodeType { get; set; }

        [Required]
        public bool Disabled { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FindCode { get; set; }

        public string ReplacementCode { get; set; }

        [Required]
        public int SortOrder { get; set; }

        public EntityDTO Entity { get; set; }

    }

    public partial class ModelFactory
    {
        public CodeReplacementDTO Create(CodeReplacement codeReplacement)
        {
            if (codeReplacement == null) return null;

            var codeReplacementDTO = new CodeReplacementDTO();

            codeReplacementDTO.CodeReplacementId = codeReplacement.CodeReplacementId;
            codeReplacementDTO.EntityId = codeReplacement.EntityId;
            codeReplacementDTO.Purpose = codeReplacement.Purpose;
            codeReplacementDTO.CodeType = codeReplacement.CodeType;
            codeReplacementDTO.Disabled = codeReplacement.Disabled;
            codeReplacementDTO.FindCode = codeReplacement.FindCode;
            codeReplacementDTO.ReplacementCode = codeReplacement.ReplacementCode;
            codeReplacementDTO.SortOrder = codeReplacement.SortOrder;
            codeReplacementDTO.Entity = Create(codeReplacement.Entity);

            return codeReplacementDTO;
        }

        public void Hydrate(CodeReplacement codeReplacement, CodeReplacementDTO codeReplacementDTO)
        {
            codeReplacement.EntityId = codeReplacementDTO.EntityId;
            codeReplacement.Purpose = codeReplacementDTO.Purpose;
            codeReplacement.CodeType = codeReplacementDTO.CodeType;
            codeReplacement.Disabled = codeReplacementDTO.Disabled;
            codeReplacement.FindCode = codeReplacementDTO.FindCode;
            codeReplacement.ReplacementCode = codeReplacementDTO.ReplacementCode;
            codeReplacement.SortOrder = codeReplacementDTO.SortOrder;
        }
    }
}
