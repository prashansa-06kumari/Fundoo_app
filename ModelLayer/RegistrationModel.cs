using System.ComponentModel.DataAnnotations;

namespace ModelLayer
{
    public class RegistrationModel
    {
        [Required]
        public string firstName { get; set; } = string.Empty;

        [Required]
        public string lastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string password { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{10}$")]
        public string contactNo { get; set; } = string.Empty;
    }
}