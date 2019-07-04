namespace to.userrepo.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using contracts.data.domain;

    public class UserList
    {
        private readonly Func<int, int> idGenerator;

        public UserList(Func<int, int> idGenerator)
        {
            this.idGenerator = idGenerator;
        }

        private List<User> Users { get; set; }

        public void Add(List<User> users)
        {
            this.Users = users;
        }

        public void Add(User user)
        {
            int newUserId;
            do
            {
                newUserId = this.idGenerator(int.MaxValue);
            } while (this.Users.Any(p => p.Id == newUserId));

            user.Id = newUserId;
            this.Users.Add(user);
        }

        public bool Exists(User user)
        {
            return this.Users.Any(p =>
                string.Equals(p.Username, user.Username, StringComparison.InvariantCultureIgnoreCase));
        }

        public User GetUserByName(string username)
        {
            return this.Users.FirstOrDefault(u =>
                string.Compare(u.Username, username, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public void Remove(User user)
        {
            this.Users.Remove(user);
        }

        public User GetById(int id)
        {
            return this.Users.FirstOrDefault(u => u.Id == id);
        }

        public List<User> ToList()
        {
            return this.Users;
        }
    }
}