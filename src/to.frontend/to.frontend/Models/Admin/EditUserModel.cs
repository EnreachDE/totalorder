using to.contracts.data.domain;

namespace to.frontend.Models.Admin
{
    public class EditUserModel
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public UserRole UserRole { get; set; }
    }
}
