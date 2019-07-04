namespace to.contracts
{
    using System.Collections.Generic;

    using data.domain;
    using data.result;

    public interface IUserRepo
    {
        (Status, User) LoadUser(string username);
        (Status, User) LoadUser(int id);
        Status UpdateUser(int id, UserRole role);
        (Status, IEnumerable<User>) GetExistingUsers();
        Status AddUser(User user);
        (Status, IEnumerable<User>) DeleteUser(int id);
        (Status, IEnumerable<string>) GetUserBacklogIds(int userId);
        Status AddUserBacklogId(int userId, string backlogId);
        Status DeleteUserBacklogId(int userId, string backlogId);
    }
}