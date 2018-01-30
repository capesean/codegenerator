using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class LookupOptionDTO
    {
        [Required]
        public Guid LookupOptionId { get; set; }

        [Required]
        public Guid LookupId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string Name { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string FriendlyName { get; set; }

        public int? Value { get; set; }

        [Required]
        public byte SortOrder { get; set; }

        public LookupDTO Lookup { get; set; }

    }

    public partial class ModelFactory
    {
        public LookupOptionDTO Create(LookupOption lookupOption)
        {
            if (lookupOption == null) return null;

            var lookupOptionDTO = new LookupOptionDTO();

            lookupOptionDTO.LookupOptionId = lookupOption.LookupOptionId;
            lookupOptionDTO.LookupId = lookupOption.LookupId;
            lookupOptionDTO.Name = lookupOption.Name;
            lookupOptionDTO.FriendlyName = lookupOption.FriendlyName;
            lookupOptionDTO.Value = lookupOption.Value;
            lookupOptionDTO.SortOrder = lookupOption.SortOrder;
            lookupOptionDTO.Lookup = Create(lookupOption.Lookup);

            return lookupOptionDTO;
        }

        public void Hydrate(LookupOption lookupOption, LookupOptionDTO lookupOptionDTO)
        {
            lookupOption.LookupId = lookupOptionDTO.LookupId;
            lookupOption.Name = lookupOptionDTO.Name;
            lookupOption.FriendlyName = lookupOptionDTO.FriendlyName;
            lookupOption.Value = lookupOptionDTO.Value;
            lookupOption.SortOrder = lookupOptionDTO.SortOrder;
        }
    }
}
