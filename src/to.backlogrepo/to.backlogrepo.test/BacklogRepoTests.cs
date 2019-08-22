using System.Linq;
using to.contracts.data.domain;

namespace to.backlogrepo.test
{
    using FluentAssertions;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;

    using contracts.data.result;

    using to.backlogrepo;
    using to.contracts;

    [TestFixture]
    public class BacklogRepoTests
    {
        private const string TestRootDir = "TestDB";
        private const string TestId = "XXX987";
        private const string BacklogsSubFolder = "Backlogs";
        private readonly string _testDir = Path.Combine(TestRootDir, BacklogsSubFolder, TestId);

        [SetUp]
        public void Initialize()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;

            // Precondition: Cleanup from last test run
            var path = Path.Combine(TestRootDir, BacklogsSubFolder);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

        }

        [Test]
        public void SaveBacklogTest()
        {
            var repo = new BacklogRepo(TestRootDir, Guid.NewGuid);
            Backlog testBacklog1 = new Backlog()
            {
                Id = String.Empty,
                Title = "TestBacklog",
                UserStories = new[] { "A", "B", "C", "D" }
            };

            Backlog testBacklog2 = new Backlog()
            {
                Id = String.Empty,
                Title = "TestBacklog",
                UserStories = new[] { "A", "B", "C", "D" },
                OneVotePerUser = true
            };

            var expectedBacklogContent1 = @"{""Id"":""XXX987"",""Title"":""TestBacklog"",""UserStories"":[""A"",""B"",""C"",""D""],""OneVotePerUser"":false}";
            var expectedBacklogContent2 = @"{""Id"":""XXX987"",""Title"":""TestBacklog"",""UserStories"":[""A"",""B"",""C"",""D""],""OneVotePerUser"":true}";

            repo.SaveBacklog(testBacklog1, TestId);

            var backlogFolderPath = Path.Combine(Environment.CurrentDirectory, _testDir);
            var backlogFilePath = Path.Combine(backlogFolderPath, "Backlog.json");

            Directory.Exists(backlogFolderPath).Should().BeTrue();
            File.Exists(backlogFilePath).Should().BeTrue();

            var backlogFileContent = File.ReadAllText(backlogFilePath);
            backlogFileContent.Should().Be(expectedBacklogContent1);

            repo.SaveBacklog(testBacklog2, TestId);
            backlogFileContent = File.ReadAllText(backlogFilePath);
            backlogFileContent.Should().Be(expectedBacklogContent2);
        }

        [Test]
        public void ReadBacklogTest()
        {
            var repo = new BacklogRepo(TestRootDir, Guid.NewGuid);
            var expectedBacklog = new Backlog()
            {
                Id = TestId,
                Title = "TestBacklog",
                UserStories = new[] { "A", "B", "C", "D" }
            };

            string testDirectory = Path.Combine(Environment.CurrentDirectory, _testDir);
            Directory.CreateDirectory(testDirectory);
            File.Copy("TestBacklog.json", Path.Combine(testDirectory, "Backlog.json"));

            var (status, actualBacklog) = repo.ReadBacklog(TestId);

            status.Should().BeOfType<Success>();
            actualBacklog.Should().BeEquivalentTo(expectedBacklog);
        }

        [Test]
        public void ReadSubmissionsTest()
        {
            var repo = new BacklogRepo(TestRootDir, Guid.NewGuid);

            var expectedSubmission1 = new Submission() { Indexes = new int[] { 1, 2, 3, 4 } };
            var expectedSubmission2 = new Submission() { Indexes = new int[] { 2, 4, 1, 3 }, UserId = 123456 };

            Directory.CreateDirectory(_testDir);
            File.Copy("Submission1.json", Path.Combine(_testDir, "Submission1.json"));
            File.Copy("Submission2.json", Path.Combine(_testDir, "Submission2.json"));
            File.WriteAllText(Path.Combine(_testDir, "Backlog.json"), "This file will always be present in the current directory, but should not be considered as a submission!");

            var (status, submissions) = repo.ReadSubmissions(TestId);

            status.Should().BeOfType<Success>();
            submissions.Length.Should().Be(2);
            submissions[0].Should().BeEquivalentTo(expectedSubmission1);
            submissions[1].Should().BeEquivalentTo(expectedSubmission2);
        }

        [Test]
        public void WriteSubmissionsToAnonymousBacklog()
        {
            var testGuid1 = Guid.NewGuid();
            var testGuid2 = Guid.NewGuid();
            var q = new Queue<Guid>(new[] { testGuid1, testGuid2 });
            var repo = new BacklogRepo(TestRootDir, () => q.Dequeue());
            var expectedSubmission1 = new Submission() { Indexes = new[] { 1, 2, 3, 4 } };
            var expectedSubmission2 = new Submission() { Indexes = new[] { 1, 2, 3, 4 }, UserId = 123456 };

            var anonymousBacklog = new Backlog()
            {
                Id = TestId,
                OneVotePerUser = false,
                Title = "TestBacklogAnonymous",
                UserStories = new[] {"A", "B", "C", "D"}
            };

            repo.SaveBacklog(anonymousBacklog, TestId);

            repo.WriteSubmission(TestId, expectedSubmission1);
            File.Exists(Path.Combine(_testDir, "Submission-" + testGuid1 + ".json")).Should().BeTrue();

            repo.WriteSubmission(TestId, expectedSubmission2);
            File.Exists(Path.Combine(_testDir, "Submission-123456.json")).Should().BeFalse();
            File.Exists(Path.Combine(_testDir, "Submission-" + testGuid2 + ".json")).Should().BeTrue();
        }
        
        [Test]
        public void WriteSubmissionsToAuthenticatedBacklog()
        {
            var testGuid1 = Guid.NewGuid();
            var testGuid2 = Guid.NewGuid();
            var q = new Queue<Guid>(new[] { testGuid1, testGuid2 });
            var repo = new BacklogRepo(TestRootDir, () => q.Dequeue());
            var anonymousSubmission = new Submission() { Indexes = new[] { 1, 2, 3, 4 } };
            var authenticatedSubmission = new Submission() { Indexes = new[] { 1, 2, 3, 4 }, UserId = 123456 };

            var authenticatedBacklog = new Backlog()
            {
                Id = TestId,
                OneVotePerUser = true,
                Title = "TestBacklogAnonymous",
                UserStories = new[] {"A", "B", "C", "D"}
            };

            repo.SaveBacklog(authenticatedBacklog, TestId);

            var status = repo.WriteSubmission(TestId, anonymousSubmission);
            status.Should().BeOfType<Failure>().Which.ErrorMessage.Should().Contain("require a UserId!");
            File.Exists(Path.Combine(_testDir, "Submission-" + testGuid1 + ".json")).Should().BeFalse();

            status = repo.WriteSubmission(TestId, authenticatedSubmission);
            status.Should().BeOfType<Success>();
            File.Exists(Path.Combine(_testDir, "Submission-123456.json")).Should().BeTrue();
            File.Exists(Path.Combine(_testDir, "Submission-" + testGuid1 + ".json")).Should().BeFalse();
            File.Exists(Path.Combine(_testDir, "Submission-" + testGuid2 + ".json")).Should().BeFalse();
        }

        [Test]
        public void GetAllTest()
        {

            var q = new Queue<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() });
            var repo = new BacklogRepo(TestRootDir, () => q.Dequeue());

            var (status1, backlogId1) = repo.CreateBacklog(new Backlog
            {
                Title = "Backlog 123456",
                UserStories = new string[] { "UserStory 1", "UserStory 2" }
            });

            var (status2, backlogId2) = repo.CreateBacklog(new Backlog
            {
                Title = "Backlog ABCDEF",
                UserStories = new string[] { "UserStory Hurra", "UserStory Ein Test" }
            });

            var (status, backlogs) = repo.GetAll();

            status.Should().BeOfType<Success>();
            backlogs.Should().HaveCount(2);
            backlogs.Should().ContainSingle(x => x.Id == backlogId1);
            backlogs.Should().ContainSingle(x => x.Id == backlogId2);
        }

        [Test]
        public void DeleteBacklogTest()
        {
            var testGuid = Guid.NewGuid();
            var repo = new BacklogRepo(TestRootDir, () => testGuid);

            var testBacklog = new Backlog()
            {
                Id = String.Empty,
                Title = "TestBacklog",
                UserStories = new[] { "A", "B", "C", "D" }
            };

            repo.SaveBacklog(testBacklog, TestId);
            repo.WriteSubmission(TestId, new Submission() { Indexes = new[] { 3, 2, 1, 0 } });

            var backlogFolderPath = Path.Combine(Environment.CurrentDirectory, _testDir);

            Directory.Exists(backlogFolderPath).Should().BeTrue();
            Directory.GetFiles(backlogFolderPath).Length.Should().Be(2, "exactly two files have been added to directory");

            var status = repo.DeleteBacklog(TestId);

            status.Should().BeOfType<Success>();
            Directory.Exists(backlogFolderPath).Should().BeFalse();

            status = repo.DeleteBacklog("Rubbish");
            status.Should().BeOfType<Failure>().Which.ErrorMessage.Should().Contain("Could not find a part of the path");
        }

        [Test]
        public void GetAllUserBacklogsTest()
        {
            var firstBacklogId = Guid.NewGuid();
            var secondBacklogId = Guid.NewGuid();
            var thirdBacklogId = Guid.NewGuid();

            var q = new Queue<Guid>(new[] { firstBacklogId, secondBacklogId, thirdBacklogId });
            var repo = new BacklogRepo(TestRootDir, () => q.Dequeue());

            var (status1, repo1) = repo.CreateBacklog(new Backlog
            {
                Title = "Backlog 123456",
                UserStories = new string[] { "UserStory 1", "UserStory 2" }
            });

            var (status2, repo2) = repo.CreateBacklog(new Backlog
            {
                Title = "Backlog ABCDEF",
                UserStories = new string[] { "UserStory Hurra", "UserStory Ein Test" }
            });

            var (status3, repo3) = repo.CreateBacklog(new Backlog
            {
                Title = "Backlog BlaBla",
                UserStories = new string[] { "UserStory Hurra", "UserStory Ein Test" }
            });

            var (status, backlogs) = repo.GetBacklogsByIds(new[] { firstBacklogId.ToString(), secondBacklogId.ToString(), thirdBacklogId.ToString() });

            backlogs.Should().HaveCount(3);
            backlogs.Should().ContainSingle(x => x.Id == repo1);
            backlogs.Should().ContainSingle(x => x.Id == repo2);
            backlogs.Should().ContainSingle(x => x.Id == repo3);
        }
    }
}
