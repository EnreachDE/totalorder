namespace to.contracts.data.result
{
    public class Failure : Status
    {
        public Failure(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; }
    }
}