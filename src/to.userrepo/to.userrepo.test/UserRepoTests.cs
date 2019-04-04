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
    using contracts.data.result;

    [TestFixture]
    public class UserRepoTests
    {
        private UserRepo _userRepo;
        private const string TestRootDir = "SampleData";
        private const string UserBacklogsFileName = "UserBacklogs.json";
        private const string UserInfoFileName = "UserInfo.json";
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

            var (status, result)  = repo.LoadUser(username.ToUpperInvariant());

            Assert.IsInstanceOf(typeof(Success), status);
            Assert.AreEqual(username, result.Username);
            Assert.AreEqual(password, result.PasswordHash);
        }

        [Test]
        [TestCase(1, "peter", "x")]
        [TestCase(2, "klaus", "y")]
        public void TestLoadUserFromId(int id, string username, string password)
        {
            var (status, result) = _userRepo.LoadUser(id);
            Assert.IsInstanceOf(typeof(Success), status);
            Assert.AreEqual(username, result.Username);
            Assert.AreEqual(password, result.PasswordHash);
        }

        [Test]
        public void TestLoadUserFromNameFailed()
        {
            var (status, result) = _userRepo.LoadUser("unknown");
            Assert.IsInstanceOf(typeof(Failure), status);
            Assert.IsNull(result);
            Assert.IsTrue(((Failure)status).ErrorMessage.Contains("unknown does not exist"));
        }

        [Test]
        public void TestLoadUserFromIdFailed()
        {
            var (status, result) =_userRepo.LoadUser(999);
            Assert.IsInstanceOf(typeof(Failure), status);
            Assert.IsNull(result);
            Assert.IsTrue(((Failure)status).ErrorMessage.Contains("999 does not exist"));
        }

        [Test]
        public void TestUpdateUser()
        {
            var status =_userRepo.UpdateUser(1, UserRole.Guest);
            status.Should().BeOfType<Success>();

            var (status1, userList) = _userRepo.GetExistingUsers();
            status1.Should().BeOfType<Success>();

            var user = userList.ToList().Find(u => u.Id == 1);
            user.UserRole.Should().BeEquivalentTo(UserRole.Guest);
        }

        [Test]
        public void TestUpdateUserFailed()
        {
            var status = _userRepo.UpdateUser(100, UserRole.Guest);
            status.Should().BeOfType<Failure>();
            ((Failure) status).ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void TestAddUserSuccessful()
        {
            var user = new User { Id = 0, PasswordHash = "abc", Username = "testuser", UserRole = UserRole.Guest };
            var expectedUser = new User { Id = 3, PasswordHash = "abc", Username = "testuser", UserRole = UserRole.Guest };
            var repo = new UserRepo(TestRootDir, UsersTestJson, g => 3);

            var status = repo.AddUser(user);
            status.Should().BeOfType<Success>();

            var (status1, userList) = repo.GetExistingUsers();
            status1.Should().BeOfType<Success>();
            userList.FirstOrDefault(p => p.Id == 3).Should().BeEquivalentTo(expectedUser);
        }

        [Test]
        public void TestAddUserFailedAlreadyExists()
        {
            var user = new User { Id = 1, PasswordHash = "x", Username = "peter", UserRole = UserRole.Guest };
            var repo = new UserRepo(TestRootDir, UsersTestJson, g => 3);

            var status = repo.AddUser(user);
            status.Should().BeOfType<Failure>();
            ((Failure) status).ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void TestDeleteUser()
        {
            var repo = new UserRepo(TestRootDir, UsersTestJson, g => 3);

            var idToDelete = 1;
            var (status, result) = repo.DeleteUser(idToDelete);

            status.Should().BeOfType<Success>();
            result.Should().NotBeEmpty();
            result.Any(u => u.Id == idToDelete).Should().BeFalse();
        }

        [Test]
        public void TestDeleteUserFailedNotFound()
        {
            var repo = new UserRepo(TestRootDir, UsersTestJson, g => 3);

            var idToDelete = 666;
            var (status, result) = repo.DeleteUser(idToDelete);

            status.Should().BeOfType<Failure>();
            ((Failure) status).ErrorMessage.Should().BeEquivalentTo("User not found");
            result.Should().BeNull();
        }

        [Test]
        public void TestGetUserBacklogIds()
        {
            Directory.CreateDirectory(Path.Combine(TestRootDir, "Users", "1"));
            File.Copy(Path.Combine(TestRootDir, "UserBacklogsTest.json"), Path.Combine(TestRootDir, "Users", "1", UserBacklogsFileName), true);
            var repo = new UserRepo(TestRootDir, UserBacklogsFileName, UserInfoFileName, g => 3);
            var (status, backlogIds) = repo.GetUserBacklogIds(1);

            status.Should().BeOfType<Success>();
            backlogIds.Should().HaveCount(3);
            backlogIds.Should().BeEquivalentTo(new[] {"1", "2", "3"});
        }
    }
}
