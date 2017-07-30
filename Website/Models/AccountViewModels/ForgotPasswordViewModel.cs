using System.ComponentModel.DataAnnotations;

namespace HomeEconomics.Website.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
