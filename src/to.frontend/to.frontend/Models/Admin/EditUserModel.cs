namespace to.frontend.Models.Admin
{
    using contracts.data.domain;

    public class EditUserModel
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public UserRole UserRole { get; set; }
    }
}