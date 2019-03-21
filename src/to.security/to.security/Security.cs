

namespace to.security
{
    using contracts.data.result;
    using System.Security.Cryptography;
    using System.Text;
    using contracts;

    public class Security : ISecurity
    {
        public Status ValidatePassword(string passwordToValidate, string hashedPassword)
        {
            string hashToValidate = HashPassword(passwordToValidate);

            if (hashToValidate == hashedPassword)
                return new Success();
            return new Failure("Password failure");
        }

        public string HashPassword(string passwordToHash)
        {
            SHA256 mySHA256 = SHA256.Create();

            byte[] buffer = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(passwordToHash));

            StringBuilder result = new StringBuilder();

            foreach (var t in buffer)
            {
                result.Append(t.ToString("x2"));
            }

            return result.ToString();
        }
    }
}
