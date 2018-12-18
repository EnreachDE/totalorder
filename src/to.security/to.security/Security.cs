using System;
using System.Security.Cryptography;
using System.Text;
using to.contracts;

namespace to.security
{
    public class Security : ISecurity
    {
        public void ValidatePassword(string passwordToValidate, string hashedPassword,
            Action onSuccess,
            Action<string> onFailure)
        {
            string hashToValidate = HashPassword(passwordToValidate);

            if (hashToValidate == hashedPassword)
                onSuccess();
            else
                onFailure("Password failure");
        }

        public string HashPassword(string passwordToHash)
        {
            SHA256 mySHA256 = SHA256Managed.Create();

            byte[] buffer = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(passwordToHash));

            StringBuilder result = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                result.Append(buffer[i].ToString("x2"));
            }
            return result.ToString();
        }
    }
}
