namespace to.contracts.data.domain
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole UserRole { get; set; }
    }
}