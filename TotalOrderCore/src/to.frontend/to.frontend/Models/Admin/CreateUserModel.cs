using System.ComponentModel.DataAnnotations;
using to.contracts.data.domain;

namespace to.frontend.Models.Admin
{
    public class CreateUserModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords must match")]
        public string PasswordRepeat { get; set; }

        public UserRole UserRole { get; set; }
    }
}
