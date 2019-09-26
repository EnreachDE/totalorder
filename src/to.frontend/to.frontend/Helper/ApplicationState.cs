using System.Collections.Generic;

namespace to.frontend.Helper
{
    public class ApplicationState : IApplicationState
    {
        private readonly Dictionary<string, object> _state;

        public ApplicationState()
        {
            _state = new Dictionary<string, object>
            {
                {"AdminUserExists", false}
            };
        }

        public T Get<T>(string key)
        {
            _state.TryGetValue(key, out var result);
            return (T)result;
        }

        public void Set<T>(string key, T value)
        {
            if (!_state.TryAdd(key, value))
            {
                _state.Remove(key);
                _state.Add(key, value);
            }
        }
    }
}