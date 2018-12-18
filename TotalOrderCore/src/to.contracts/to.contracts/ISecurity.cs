using System;

namespace to.contracts
{
    public interface ISecurity
    {
        void ValidatePassword(string passwordToValidate, string hashedPassword,
            Action onSuccess,
            Action<string> onFailure);

        string HashPassword(string passwordToHash);
    }
}
