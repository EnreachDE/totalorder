namespace to.contracts.data.result
{
    public class Success : Status
    {
        
    }

    public class Success<T> : Status
    {
        public T Data { get; private set; }

        public Success(T data)
        {
            Data = data;
        }
    }
}