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
        private readonly string _userListFileNameObsolete;
        private readonly string _userBacklogsFileName;
        private readonly string _userInfoFileName;
        private const string _usersSubFolder = "Users";
        private readonly Func<int, int> _idGenerator;

        public UserRepo(string rootpath, string userListFileNameObsolete)
        {
            var rnd = new Random();
            _idGenerator = max => rnd.Next(0, max);
            _rootpath = rootpath;
            _userListFileNameObsolete = userListFileNameObsolete;
        }

        public UserRepo(Func<int, int> idGenerator) : this(Environment.CurrentDirectory, "users.json", idGenerator)
        {
        }

        public UserRepo(string rootpath, string filename, Func<int, int> idGenerator) 
        {
            this._rootpath = rootpath;
            _userListFileNameObsolete = filename;
            _idGenerator = idGenerator;
        }

        public UserRepo(string rootpath, string userBacklogsFileName, string userInfoFileName, Func<int, int> idGenerator) 
        {
            this._rootpath = rootpath;
            this._userBacklogsFileName = userBacklogsFileName;
            this._userInfoFileName = userInfoFileName;
            this._idGenerator = idGenerator;
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

        public (Status, IEnumerable<string>) GetUserBacklogIds(int userId)
        {
            var path = Path.Combine(_rootpath, _usersSubFolder, userId.ToString(), _userBacklogsFileName);
            if (!File.Exists(path))
                return (new Failure("Cannot find requested user backlogs."), null);
            
            var jsonString = File.ReadAllText(path);
            var backlogIds = JsonConvert.DeserializeObject<List<string>>(jsonString);

            return (new Success(), backlogIds);
        }

        private void SaveUserList(UserList users)
        {
            var jsonString = JsonConvert.SerializeObject(users.ToList(), Formatting.Indented);
            File.WriteAllText(Path.Combine(this._rootpath, _userListFileNameObsolete), jsonString);
        }

        private UserList ReadUserList()
        {
            var jsonString = File.ReadAllText(Path.Combine(this._rootpath, _userListFileNameObsolete));
            var users = JsonConvert.DeserializeObject<List<User>>(jsonString);

            var userList = new UserList(_idGenerator);
            userList.Add(users);
            return userList;
        }
    }
}
