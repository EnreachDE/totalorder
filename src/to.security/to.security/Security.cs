namespace to.security
{
    using System.Security.Cryptography;
    using System.Text;

    using contracts;
    using contracts.data.result;

    public class Security : ISecurity
    {
        public Status ValidatePassword(string passwordToValidate, string hashedPassword)
        {
            var hashToValidate = HashPassword(passwordToValidate);

            if (hashToValidate == hashedPassword)
            {
                return new Success();
            }

            return new Failure("Password failure");
        }

        public string HashPassword(string passwordToHash)
        {
            var mySHA256 = SHA256.Create();

            var buffer = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(passwordToHash));

            var result = new StringBuilder();

            foreach (var t in buffer) result.Append(t.ToString("x2"));

            return result.ToString();
        }
    }
}