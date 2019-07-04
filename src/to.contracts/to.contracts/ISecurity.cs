namespace to.contracts
{
    using data.result;

    public interface ISecurity
    {
        Status ValidatePassword(string passwordToValidate, string hashedPassword);

        string HashPassword(string passwordToHash);
    }
}