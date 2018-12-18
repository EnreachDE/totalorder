using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using to.contracts;
using to.contracts.data.domain;

namespace to.userrepo.test
{
    [TestFixture]
    public class UserRepoTests
    {
        private UserRepo _userRepo;
        private const string TestRootDir = "SampleData";
        private const string UsersTestJson = "usersTest.json";

        [SetUp]
        public void Initialize()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            _userRepo = new UserRepo(TestRootDir, UsersTestJson);
            File.Copy(Path.Combine(TestRootDir, "users.json"), Path.Combine(TestRootDir, UsersTestJson), true);
        }

        [Test]
        [TestCase("peter", "x")]
        [TestCase("klaus", "y")]
        public void TestLoadUserFromName(string username, string password)
        {
            var repo = new UserRepo(TestRootDir, UsersTestJson);
            User result = null;
            repo.LoadUser(username.ToUpperInvariant(),
                (user) => result = user,
                _ => Assert.Fail());
            Assert.AreEqual(username, result.Username);
            Assert.AreEqual(password, result.PasswordHash);
        }

        [Test]
        [TestCase(1, "peter", "x")]
        [TestCase(2, "klaus", "y")]
        public void TestLoadUserFromId(int id, string username, string password)
        {
            User result = null;
            _userRepo.LoadUser(id,
                user => result = user,
                s => Assert.Fail());
            Assert.AreEqual(username, result.Username);
            Assert.AreEqual(password, result.PasswordHash);
        }

        [Test]
        public void TestLoadUserFromNameFailed()
        {
            string result = null;
            _userRepo.LoadUser("unknown",
                user => Assert.Fail("OnSuccess should not be called here!"),
                error => result = error);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("unknown does not exist"));
        }

        [Test]
        public void TestLoadUserFromIdFailed()
        {
            string result = null;
            _userRepo.LoadUser(999,
                user => Assert.Fail("OnSuccess should not be called here!"),
                error => result = error);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("999 does not exist"));
        }

        [Test]
        public void TestUpdateUser()
        {
            string result = null;
            List<User> userList = new List<User>();


            _userRepo.UpdateUser(1, UserRole.Guest, () => result = "success", error => result = error);
            Assert.AreEqual("success", result);

            result = null;
            _userRepo.GetExistingUsers(users => userList = users.ToList(), error => result = error);
            Assert.IsNull(result);

            var user = userList.Find(u => u.Id == 1);
            Assert.AreEqual(UserRole.Guest, user.UserRole);
        }

        [Test]
        public void TestUpdateUserFailed()
        {
            string result = null;


            _userRepo.UpdateUser(100, UserRole.Guest, () => result = "success", error => result = error);
            Assert.AreNotEqual(null, result);
            Assert.AreNotEqual("success", result);
        }

        [Test]
        public void TestAddUserSuccessful()
        {
            string result = null;
            var userList = new List<User>();
            var user = new User { Id = 0, PasswordHash = "abc", Username = "testuser", UserRole = UserRole.Guest };
            var expectedUser = new User { Id = 3, PasswordHash = "abc", Username = "testuser", UserRole = UserRole.Guest };
            var repo = new UserRepo(TestRootDir, UsersTestJson, g => 3);

            repo.AddUser(user, () => result = "success", s => { });

            Assert.AreEqual("success", result);
            result = null;
            repo.GetExistingUsers(users => userList = users.ToList(), error => result = error);
            Assert.IsNull(result);
            userList.FirstOrDefault(p => p.Id == 3).Should().BeEquivalentTo(expectedUser);
        }

        [Test]
        public void TestAddUserFailedAlreadyExists()
        {
            string result = null;
            var user = new User { Id = 1, PasswordHash = "x", Username = "peter", UserRole = UserRole.Guest };
            var repo = new UserRepo(TestRootDir, UsersTestJson, g => 3);

            repo.AddUser(user, () => {}, s => { result = s; });

            Assert.AreEqual("User already exists", result);
        }

        [Test]
        public void TestDeleteUser()
        {
            var repo = new UserRepo(TestRootDir, UsersTestJson, g => 3);

            var idToDelete = 1;
            IEnumerable<User> result = null;
            repo.DeleteUser(idToDelete, userList => result = userList.ToList(), s => { });

            result.Should().NotBeEmpty();
            result.Any(u => u.Id == idToDelete).Should().BeFalse();
        }

        [Test]
        public void TestDeleteUserFailedNotFound()
        {
            var repo = new UserRepo(TestRootDir, UsersTestJson, g => 3);

            var idToDelete = 666;
            string result = null;
            repo.DeleteUser(idToDelete, userList => { }, s => { result = s; });

            Assert.AreEqual("User not found", result);
        }
    }
}
