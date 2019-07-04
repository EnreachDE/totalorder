namespace to.userrepo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using contracts;
    using contracts.data.domain;
    using contracts.data.result;

    using Domain;

    using Newtonsoft.Json;

    public class UserRepo : IUserRepo
    {
        private const string UserBacklogsFileName = "UserBacklogs.json";
        private const string UsersSubFolder = "Users";
        private readonly Func<int, int> idGenerator;
        private readonly string rootPath;
        private readonly string userListFileName;

        public UserRepo(string rootPath, string userListFileName)
        {
            var rnd = new Random();
            this.idGenerator = max => rnd.Next(0, max);
            this.rootPath = rootPath;
            this.userListFileName = userListFileName;
        }

        public UserRepo(Func<int, int> idGenerator) : this(Environment.CurrentDirectory, "users.json", idGenerator)
        {
        }

        public UserRepo(string rootPath, string filename, Func<int, int> idGenerator)
        {
            this.rootPath = rootPath;
            this.userListFileName = filename;
            this.idGenerator = idGenerator;
        }

        public (Status, User) LoadUser(string username)
        {
            var users = ReadUserList();

            var user = users.GetUserByName(username);

            if (user != null)
            {
                return (new Success(), user);
            }

            return (new Failure($"User {username} does not exist"), null);
        }

        public (Status, User) LoadUser(int id)
        {
            var users = ReadUserList();

            var user = users.GetById(id);

            if (user != null)
            {
                return (new Success(), user);
            }

            return (new Failure($"User with ID {id} does not exist"), null);
        }


        public Status UpdateUser(int id, UserRole role)
        {
            var users = ReadUserList();

            var user = users.GetById(id);
            if (user == null)
            {
                return new Failure($"User with id {id} does not exist.");
            }

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
                return new Failure("User already exists");
            }

            users.Add(user);
            SaveUserList(users);
            CreateUserDirectory(user.Id);

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
            DeleteUserDirectory(user.Id);

            return (new Success(), users.ToList());
        }

        public (Status, IEnumerable<string>) GetUserBacklogIds(int userId)
        {
            var path = Path.Combine(this.rootPath, UsersSubFolder, userId.ToString(), UserBacklogsFileName);
            if (!File.Exists(path))
            {
                return (new Failure("Cannot find requested user backlogs."), null);
            }

            var jsonString = File.ReadAllText(path);
            var backlogIds = JsonConvert.DeserializeObject<List<string>>(jsonString);

            return (new Success(), backlogIds);
        }

        public Status AddUserBacklogId(int userId, string backlogId)
        {
            var path = Path.Combine(this.rootPath, UsersSubFolder, userId.ToString(), UserBacklogsFileName);
            var backlogIds = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
            if (backlogIds.Contains(backlogId))
            {
                return new Failure("BacklogId exists already");
            }

            backlogIds.Add(backlogId);
            var jsonString = JsonConvert.SerializeObject(backlogIds.ToList());
            File.WriteAllText(path, jsonString);
            return new Success();

        }

        public Status DeleteUserBacklogId(int userId, string backlogId)
        {
            var path = Path.Combine(this.rootPath, UsersSubFolder, userId.ToString(), UserBacklogsFileName);
            var backlogIds = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
            if (!backlogIds.Contains(backlogId))
            {
                return new Failure("BacklogId does not exist");
            }

            backlogIds.Remove(backlogId);
            var jsonString = JsonConvert.SerializeObject(backlogIds.ToList());
            File.WriteAllText(path, jsonString);
            return new Success();

        }

        private void CreateUserDirectory(int userId)
        {
            var path = Path.Combine(this.rootPath, UsersSubFolder, userId.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var filePath = Path.Combine(path, UserBacklogsFileName);
            if (!File.Exists(filePath))
            {
                File.AppendAllText(filePath, "[]");
            }
        }

        private void DeleteUserDirectory(int userId)
        {
            var path = Path.Combine(this.rootPath, UsersSubFolder, userId.ToString());
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private void SaveUserList(UserList users)
        {
            var jsonString = JsonConvert.SerializeObject(users.ToList(), Formatting.Indented);
            File.WriteAllText(Path.Combine(this.rootPath, UsersSubFolder, this.userListFileName), jsonString);
        }

        private UserList ReadUserList()
        {
            var jsonString = File.ReadAllText(Path.Combine(this.rootPath, UsersSubFolder, this.userListFileName));
            var users = JsonConvert.DeserializeObject<List<User>>(jsonString);

            var userList = new UserList(this.idGenerator);
            userList.Add(users);
            return userList;
        }
    }
}