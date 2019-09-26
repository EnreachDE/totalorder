namespace to.frontend.Helper
{
    public interface IApplicationState
    {
        T Get<T>(string key);
        void Set<T>(string key, T value);
    }
}
