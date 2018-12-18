using System;
using System.Collections.Generic;
using to.contracts.data.domain;

namespace to.contracts
{
    public interface IUserRepo
    {
        void LoadUser(string username,
            Action<User> onSuccess,
            Action<string> onFailure);

        void LoadUser(int id,
            Action<User> onSuccess,
            Action<string> onFailure);


        void UpdateUser(int id, UserRole role, Action onSuccess, Action<string> onFailure);
        void GetExistingUsers(Action<IEnumerable<User>> onSuccess, Action<string> onFailure);
        void AddUser(User user, Action onSuccess, Action<string> onFailure);
        void DeleteUser(int id, Action<IEnumerable<User>> onSuccess, Action<string> onFailure);
    }
}
