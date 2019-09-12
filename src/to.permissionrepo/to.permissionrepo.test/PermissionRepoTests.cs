using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using to.contracts.data.domain;
using to.contracts.data.result;

namespace to.permissionrepo.test
{
    using System.Collections.Generic;

    using contracts;

    using Newtonsoft.Json;

    [TestFixture]
    public class PermissionRepoTests
    {
        private PermissionRepo _permissionRepo;
        private const string TestRootDir = "SampleData";
        private const string PermissionsTestJson = "permissionsTest.json";
        private const string PermissionsMissingTestJson = "permissionsMissingTest.json";

        [SetUp]
        public void Setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            _permissionRepo = new PermissionRepo(TestRootDir, PermissionsTestJson);
        }

        [TearDown]
        public void TearDown()
        {
            var path = Path.Combine(TestRootDir, PermissionsMissingTestJson);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        [Test]
        public void PermissionsCanBeLoaded_ForGuest()
        {
            File.Copy(Path.Combine(TestRootDir, "permissions.json"), Path.Combine(TestRootDir, PermissionsTestJson), true);
            var (status, permissions) = _permissionRepo.LoadPermissions(UserRole.Guest);
            Assert.That(status is Success);
            Assert.That(permissions.Any());
        }

        [Test]
        public void ThrowsExceptionIfInvalidValues()
        {
            File.Copy(Path.Combine(TestRootDir, "permissionsError.json"), Path.Combine(TestRootDir, PermissionsTestJson), true);
            Action load = () => { var (status, permissions) = _permissionRepo.LoadPermissions(UserRole.Administrator); };
            load.Should().Throw<Exception>();
        }

        [Test]
        public void InitialiseSuccessfully()
        {
            var path = Path.Combine(TestRootDir, PermissionsMissingTestJson);
            var permissionRepo = new PermissionRepo(TestRootDir, PermissionsMissingTestJson);
            var status = permissionRepo.Initialise();
            status.Should().BeOfType<Success>();
            File.Exists(path).Should().BeTrue();
            var fileContent = File.ReadAllText(path);
            var deserializeObject = JsonConvert.DeserializeObject<Dictionary<UserRole, List<Permission>>>(fileContent);
            deserializeObject.Should().ContainKeys(UserRole.Administrator, UserRole.Developer, UserRole.Guest, UserRole.ProductOwner);
        }
    }
}