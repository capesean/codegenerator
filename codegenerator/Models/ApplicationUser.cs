using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public partial class ApplicationUser
    {
        [Required(AllowEmptyStrings = true)]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public bool Enabled { get; set; }

        public ApplicationUser()
        {
            Id = Guid.NewGuid();
        }
    }
}
