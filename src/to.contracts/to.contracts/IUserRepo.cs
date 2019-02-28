using System;
using System.Collections.Generic;
using to.contracts.data.domain;

namespace to.contracts
{
    using data.result;

    public interface IUserRepo
    {
        (Status, User) LoadUser(string username);
        (Status, User) LoadUser(int id);
        void UpdateUser(int id, UserRole role, Action onSuccess, Action<string> onFailure);
        void GetExistingUsers(Action<IEnumerable<User>> onSuccess, Action<string> onFailure);
        void AddUser(User user, Action onSuccess, Action<string> onFailure);
        (Status, IEnumerable<User>) DeleteUser(int id);
    }
}
