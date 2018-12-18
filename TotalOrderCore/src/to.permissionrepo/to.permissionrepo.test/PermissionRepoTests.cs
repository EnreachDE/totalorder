using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using to.contracts.data.domain;
using to.contracts.data.result;

namespace to.permissionrepo.test
{
    [TestFixture]
    public class PermissionRepoTests
    {
        private PermissionRepo _permissionRepo;
        private const string TestRootDir = "SampleData";
        private const string PermissionsTestJson = "permissionsTest.json";

        [SetUp]
        public void Setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
            _permissionRepo = new PermissionRepo(TestRootDir, PermissionsTestJson);
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
    }
}