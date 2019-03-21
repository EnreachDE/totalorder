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
    using contracts.data.result;

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

        public (Status, User) LoadUser(string username)
        {
            var users = ReadUserList();

            var user = users.GetUserByName(username);

            if (user != null)
                return (new Success(), user);

            return (new Failure($"User {username} does not exist"), null);
        }

        public (Status, User) LoadUser(int id)
        {
            var users = ReadUserList();

            var user = users.GetById(id);

            if (user != null)
                return (new Success(), user);

            return (new Failure($"User with ID {id} does not exist"), null);
        }


        public Status UpdateUser(int id, UserRole role)
        {
            var users = ReadUserList();

            var user = users.GetById(id);
            if (user == null) return (new Failure($"User with id {id} does not exist."));

            user.UserRole = role;
            SaveUserList(users);

            return new Success();
        }

        public (Status, IEnumerable<User>) GetExistingUsers()
        {
            var userList = ReadUserList().ToList();

            if (!userList.Any())
            {
                return (new Failure("No users found."), null);
            }

            return (new Success(), userList);
        }

        public Status AddUser(User user)
        {
            var users = ReadUserList();

            if (users.Exists(user))
            {
                return (new Failure("User already exists"));
            }

            users.Add(user);
            SaveUserList(users);
            
            return new Success();
        }

        public (Status, IEnumerable<User>) DeleteUser(int id)
        {
            var users = ReadUserList();
            var user = users.GetById(id);

            if (user == null)
            {
                return (new Failure("User not found"), null);
            }

            users.Remove(user);
            SaveUserList(users);

            return (new Success(), users.ToList());
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
