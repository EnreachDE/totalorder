using System;
using System.Collections.Generic;
using System.Linq;
using to.contracts.data.domain;

namespace to.userrepo.Domain
{
    public class UserList
    {
        private List<User> Users { get; set; }
        private readonly Func<int, int> _idGenerator;

        public UserList(Func<int, int> idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public void Add(List<User> users)
        {
            Users = users;
        }

        public void Add(User user)
        {
            int newUserId;
            do
            {
                newUserId = _idGenerator(int.MaxValue);
            } while (Users.Any(p => p.Id == newUserId));

            user.Id = newUserId;
            Users.Add(user);
        }

        public bool Exists(User user)
        {
            return Users.Any(p =>
                string.Equals(p.Username, user.Username, StringComparison.InvariantCultureIgnoreCase));
        }

        public User GetUserByName(string username)
        {
            return Users.FirstOrDefault(u =>
                String.Compare(u.Username, username, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public void Remove(User user)
        {
            Users.Remove(user);
        }

        public User GetById(int id)
        {
            return Users.FirstOrDefault(u => u.Id == id);
        }

        public List<User> ToList()
        {
            return Users;
        }
    }
}
