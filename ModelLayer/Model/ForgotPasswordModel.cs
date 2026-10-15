using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;
    }
}