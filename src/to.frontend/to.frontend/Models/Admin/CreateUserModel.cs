namespace to.frontend.Models.Admin
{
    using System.ComponentModel.DataAnnotations;

    using contracts.data.domain;

    public class CreateUserModel
    {
        [Required] public string Username { get; set; }

        [Required] public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords must match")]
        public string PasswordRepeat { get; set; }

        public UserRole UserRole { get; set; }
    }
}