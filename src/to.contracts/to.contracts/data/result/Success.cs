namespace to.contracts.data.result
{
    public class Success : Status
    {
    }

    public class Success<T> : Status
    {
        public Success(T data)
        {
            this.Data = data;
        }

        public T Data { get; }
    }
}