using to.contracts.data.domain;

namespace to.contracts
{
    public interface ITotalOrder
    {
        int[] Order(Submission[] submissions);
    }
}
