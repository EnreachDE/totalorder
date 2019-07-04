namespace to.contracts
{
    using data.domain;

    public interface ITotalOrder
    {
        int[] Order(Submission[] submissions);
    }
}