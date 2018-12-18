using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using to.contracts;
using to.contracts.data.domain;
using to.userrepo.Domain;

namespace to.userrepo
{
    public class UserRepo : IUserRepo
    {
        private readonly string _rootpath;
        private readonly string _fileName;
        private readonly Func<int, int> _idGenerator;

        public UserRepo(string rootpath, string fileName)
        {
            var rnd = new Random();
            _idGenerator = max => rnd.Next(0, max);
            _rootpath = rootpath;
            _fileName = fileName;
        }

        public UserRepo(Func<int, int> idGenerator) : this(Environment.CurrentDirectory, "users.json", idGenerator)
        {
        }

        public UserRepo(string rootpath, string filename, Func<int, int> idGenerator) 
        {
            this._rootpath = rootpath;
            _fileName = filename;
            _idGenerator = idGenerator;
        }

        public void LoadUser(string username, Action<User> onSuccess, Action<string> onFailure)
        {
            Debug.Assert(onSuccess != null, "at least for the success case a delegate must be provided!");

            var users = ReadUserList();

            var user = users.GetUserByName(username);

            if (user != null)
                onSuccess(user);
            else
                onFailure(String.Format("User {0} does not exist", username));
        }

        public void LoadUser(int id, Action<User> onSuccess, Action<string> onFailure)
        {
            Debug.Assert(onSuccess != null, "at least for the success case a delegate must be provided!");

            var users = ReadUserList();

            var user = users.GetById(id);

            if (user != null)
                onSuccess(user);
            else
                onFailure(String.Format("User with ID {0} does not exist", id));
        }


        public void UpdateUser(int id, UserRole role, Action onSuccess, Action<string> onFailure)
        {
            Debug.Assert(onSuccess != null, "at least for the success case a delegate must be provided!");

            var users = ReadUserList();

            var user = users.GetById(id);

            if (user == null)
            {
                onFailure(String.Format("User with id {0} does not exist", id));
                return;
            }

            user.UserRole = role;

            SaveUserList(users);

            onSuccess();
        }

        public void GetExistingUsers(Action<IEnumerable<User>> onSuccess, Action<string> onFailure)
        {
            Debug.Assert(onSuccess != null, "at least for the success case a delegate must be provided!");

            var userList = ReadUserList().ToList();

            if (userList.Any())
            {
                onSuccess(userList);
            }
            else
            {
                onFailure("No users found");
            }
        }

        public void AddUser(User user, Action onSuccess, Action<string> onFailure)
        {
            var users = ReadUserList();

            if (users.Exists(user))
            {
                onFailure("User already exists");
                return;
            }

            users.Add(user);
            SaveUserList(users);
            onSuccess();
        }

        public void DeleteUser(int id, Action<IEnumerable<User>> onSuccess, Action<string> onFailure)
        {
            var users = ReadUserList();

            var user = users.GetById(id);
            if (user == null)
            {
                onFailure("User not found");
                return;
            }

            users.Remove(user);
            SaveUserList(users);

            onSuccess(users.ToList());
        }

        private void SaveUserList(UserList users)
        {
            var jsonString = JsonConvert.SerializeObject(users.ToList(), Formatting.Indented);
            File.WriteAllText(Path.Combine(this._rootpath, _fileName), jsonString);
        }

        private UserList ReadUserList()
        {
            var jsonString = File.ReadAllText(Path.Combine(this._rootpath, _fileName));
            var users = JsonConvert.DeserializeObject<List<User>>(jsonString);

            var userList = new UserList(_idGenerator);
            userList.Add(users);
            return userList;
        }
    }
}
