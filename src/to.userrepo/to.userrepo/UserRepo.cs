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
        private readonly string _userListFileName;
        private const string _userBacklogsFileName = "UserBacklogs.json";
        private readonly string _userInfoFileName;
        private const string _usersSubFolder = "Users";
        private readonly Func<int, int> _idGenerator;

        public UserRepo(string rootpath, string userListFileName)
        {
            var rnd = new Random();
            _idGenerator = max => rnd.Next(0, max);
            _rootpath = rootpath;
            _userListFileName = userListFileName;
        }

        public UserRepo(Func<int, int> idGenerator) : this(Environment.CurrentDirectory, "users.json", idGenerator)
        {
        }

        public UserRepo(string rootpath, string filename, Func<int, int> idGenerator) 
        {
            _rootpath = rootpath;
            _userListFileName = filename;
            _idGenerator = idGenerator;
        }

        public UserRepo(string rootpath, string userBacklogsFileName, string userInfoFileName, Func<int, int> idGenerator) 
        {
            _rootpath = rootpath;
            _userInfoFileName = userInfoFileName;
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
            CreateUserDirectory(user.Id);

            return new Success();
        }

        private void CreateUserDirectory(int userId)
        {
            var path = Path.Combine(_rootpath, _usersSubFolder, userId.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filePath = Path.Combine(path, _userBacklogsFileName);
            if (!File.Exists(filePath))
            {
                File.AppendAllText(filePath, "[]");
            }
        }

        private void DeleteUserDirectory(int userId)
        {
            var path = Path.Combine(_rootpath, _usersSubFolder, userId.ToString());
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
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
            DeleteUserDirectory(user.Id);

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

        public Status AddUserBacklogId(int userId, string backlogId)
        {
            var path = Path.Combine(_rootpath, _usersSubFolder, userId.ToString(), _userBacklogsFileName);
            var backlogIds = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
            if (!backlogIds.Contains(backlogId))
            {
                backlogIds.Add(backlogId);
                var jsonString = JsonConvert.SerializeObject(backlogIds.ToList());
                File.WriteAllText(path, jsonString);
                return new Success();
            }

            return new Failure("BacklogId exists already");
        }

        public Status DeleteUserBacklogId(int userId, string backlogId)
        {
            var path = Path.Combine(_rootpath, _usersSubFolder, userId.ToString(), _userBacklogsFileName);
            var backlogIds = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
            if (backlogIds.Contains(backlogId))
            {
                backlogIds.Remove(backlogId);
                var jsonString = JsonConvert.SerializeObject(backlogIds.ToList());
                File.WriteAllText(path, jsonString);
                return new Success();
            }

            return new Failure("BacklogId does not exist");
        }

        private void SaveUserList(UserList users)
        {
            var jsonString = JsonConvert.SerializeObject(users.ToList(), Formatting.Indented);
            File.WriteAllText(Path.Combine(_rootpath,_usersSubFolder, _userListFileName), jsonString);
        }

        private UserList ReadUserList()
        {
            var jsonString = File.ReadAllText(Path.Combine(_rootpath, _usersSubFolder, _userListFileName));
            var users = JsonConvert.DeserializeObject<List<User>>(jsonString);

            var userList = new UserList(_idGenerator);
            userList.Add(users);
            return userList;
        }
    }
}
