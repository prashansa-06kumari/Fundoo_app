using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model
{
    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string newPassword { get; set; } = string.Empty;
    }
}