using System.Linq;
using to.contracts.data.domain;

namespace to.backlogrepo.test
{
    using FluentAssertions;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using to.backlogrepo;
    using to.contracts;

    [TestFixture]
    public class BacklogRepoTests
    {
        private const string TestRootDir = "TestDB";
        private const string TestId = "XXX987";
        private readonly string _testDir = Path.Combine(TestRootDir, TestId);

        [SetUp]
        public void Initialize()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;

            // Precondition: Cleanup from last test run
            if (Directory.Exists(TestRootDir))
            {
                Directory.Delete(TestRootDir, true);
            }

        }

        [Test]
        public void GenerateBacklogIdTest()
        {
            var q = new Queue<int>(new[] { 1, 2, 3, 4, 5, 6 });
            var repo = new BacklogRepo(TestRootDir, g => q.Dequeue());

            var id = repo.GenerateBacklogId();

            id.Should().Be("BCD456");
            Directory.Exists(Path.Combine("TestDB",id)).Should().BeTrue();
        }

        [Test]
        public void SaveBacklogTest()
        {
            var repo = new BacklogRepo(TestRootDir, g => 1);
            Backlog testBacklog = new Backlog()
            {
                Id = String.Empty,
                Title = "TestBacklog",
                UserStories = new []{ "A", "B", "C", "D" }
            };

            var expectedBacklogContent = @"{""Id"":""XXX987"",""Title"":""TestBacklog"",""UserStories"":[""A"",""B"",""C"",""D""]}";

            repo.SaveBacklog(testBacklog, TestId);

            var backlogFolderPath = Path.Combine(Environment.CurrentDirectory, _testDir);
            var backlogFilePath = Path.Combine(backlogFolderPath, "Backlog.json");

            Directory.Exists(backlogFolderPath).Should().BeTrue();
            File.Exists(backlogFilePath).Should().BeTrue();

            var backlogFileContent = File.ReadAllText(backlogFilePath);
            backlogFileContent.Should().Be(expectedBacklogContent);
        }

        [Test]
        public void ReadBacklogTest()
        {
            var repo = new BacklogRepo(TestRootDir, g => 1);
            var expectedBacklog = new Backlog()
            {
                Id = TestId,
                Title = "TestBacklog",
                UserStories = new[] { "A", "B", "C", "D" }
            };

            string testDirectory = Path.Combine(Environment.CurrentDirectory, _testDir);
            Directory.CreateDirectory(testDirectory);
            File.Copy("TestBacklog.json", Path.Combine(testDirectory, "Backlog.json"));           

            var actualBacklog = repo.ReadBacklog(TestId);

            actualBacklog.Should().BeEquivalentTo(expectedBacklog);
        }

        [Test]
        public void ReadSubmissionsTest()
        {
            var repo = new BacklogRepo(TestRootDir, g => 1);

            var expectedSubmission1 = new Submission() { Indexes = new int[] { 1, 2, 3, 4 } };
            var expectedSubmission2 = new Submission() { Indexes = new int[] { 2, 4, 1, 3 } };

            Directory.CreateDirectory(_testDir);
            File.Copy("Submission1.json", Path.Combine(_testDir, "Submission1.json"));
            File.Copy("Submission2.json", Path.Combine(_testDir, "Submission2.json"));
            File.WriteAllText(Path.Combine(_testDir, "Backlog.json"), "This file will always be present in the current directory, but should not be considered as a submission!");

            var submissions = repo.ReadSubmissions(TestId);

            submissions.Length.Should().Be(2);
            submissions[0].Should().BeEquivalentTo(expectedSubmission1);
            submissions[1].Should().BeEquivalentTo(expectedSubmission2);
        }

        [Test]
        public void WriteSubmissionsTest()
        {
            var testGuid = Guid.NewGuid();
            var repo = new BacklogRepo(TestRootDir, () => testGuid);
            var expectedSubmission1 = new Submission() { Indexes = new int[] { 1, 2, 3, 4 } };

            Directory.CreateDirectory(_testDir);
            repo.WriteSubmission(TestId, expectedSubmission1);
            File.Exists(Path.Combine(_testDir, "Submission-" + testGuid + ".json")).Should().BeTrue();
        }

        [Test]
        public void GetAllTest()
        {
            var q = new Queue<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            var repo = new BacklogRepo(TestRootDir, g => q.Dequeue());

            var repo1 = repo.CreateBacklog(new Backlog
            {
                Title = "Backlog 123456",
                UserStories = new string[] {"UserStory 1", "UserStory 2"}
            });

            var repo2 = repo.CreateBacklog(new Backlog
            {
                Title = "Backlog ABCDEF",
                UserStories = new string[] { "UserStory Hurra", "UserStory Ein Test" }
            });
            
            var backlogs = repo.GetAll();

            Assert.That(backlogs.Count == 2);
            Assert.That(backlogs.FirstOrDefault(b => b.Id == repo1) != null);
            Assert.That(backlogs.FirstOrDefault(b => b.Id == repo2) != null);
        }

        [Test]
        public void DeleteBacklogTest()
        {
            var testGuid = Guid.NewGuid();
            var repo = new BacklogRepo(TestRootDir, () => testGuid);

            var testBacklog = new Backlog() {
                                Id = String.Empty,
                                Title = "TestBacklog",
                                UserStories = new []{ "A", "B", "C", "D" } };

            repo.SaveBacklog(testBacklog, TestId);
            repo.WriteSubmission(TestId, new Submission() { Indexes = new [] {3, 2, 1, 0} });

            var backlogFolderPath = Path.Combine(Environment.CurrentDirectory, _testDir);

            Directory.Exists(backlogFolderPath).Should().BeTrue();
            Directory.GetFiles(backlogFolderPath).Length.Should().Be(2, "exactly two files have been added to directory");

            repo.DeleteBacklog(TestId);
            Directory.Exists(backlogFolderPath).Should().BeFalse();

            Action act = () => repo.DeleteBacklog("Rubbish");
            act.Should().Throw<DirectoryNotFoundException>("directory \"Rubbish\" does not exist");
        }
    }
}
