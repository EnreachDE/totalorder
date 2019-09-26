using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace to.contracts
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Permission
    {
        CreateBacklog,
        DeleteBacklog,
        EvaluateBacklog,
        OrderBacklog,
        ListBacklog,
        CreateUser,
        DeleteUser,
        EditUser,
        ListUser
    }
}