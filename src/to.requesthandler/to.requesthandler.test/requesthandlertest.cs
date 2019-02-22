using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using to.contracts;
using to.contracts.data.domain;
using to.contracts.data.result;
using to.requesthandler;
using to.totalorder;

namespace to.requesthandlertest
{
    [TestFixture]
    public class RequestHandlerTest
    {
        private RequestHandler _sut;
        private Mock<IBacklogRepo> _backlogrepo;
        private Mock<ITotalOrder> _totalOrder;
        private Mock<IUserRepo> _userRepo;
        private Mock<ISecurity> _security;
        private Mock<IPermissionRepo> _permissions;

        [SetUp]
        public void Initialize()
        {
            _backlogrepo = new Mock<IBacklogRepo>();
            _totalOrder = new Mock<ITotalOrder>();
            _userRepo = new Mock<IUserRepo>();
            _security = new Mock<ISecurity>();
            _permissions = new Mock<IPermissionRepo>();

            _sut = new RequestHandler(_backlogrepo.Object, _totalOrder.Object, _userRepo.Object, _security.Object, _permissions.Object);
        }

        [Test]
        public void applyOrderTest()
        {
            string[] userStories = new[] { "A", "B", "C" };
            int[] currentOrder = { 1, 0, 2 };

            var newUserStoriesOrdered = RequestHandler.applyOrder(userStories, currentOrder);

            string[] expected = new[] { "B", "A", "C" };
            newUserStoriesOrdered.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void applyOrderWithoutSubmissionTest()
        {
            string[] userStories = new[] { "A", "B", "C" };
            int[] currentOrder = { };
            var newUserStoriesOrdered = RequestHandler.applyOrder(userStories, currentOrder);

            string[] expected = new[] { "A", "B", "C" };
            newUserStoriesOrdered.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void AcceptanceCreateBacklogTest()
        {
            IBacklogRepo repo = new BacklogRepoTest();
            ITotalOrder toalOrder = new TotalOrder();

            RequestHandler requestHandler = new RequestHandler(repo, toalOrder, null, null, null);

            BacklogCreationRequest backlogCreationRequest = new BacklogCreationRequest
            {
                Title = "The backlog",
                UserStories = new string[] { "A", "B", "C" }
            };
            var result = requestHandler.HandleBacklogCreationRequest(backlogCreationRequest);

            // expected result
            BacklogEvalQueryResult expectedResult = new BacklogEvalQueryResult
            {
                Id = "7",
                NumberOfSubmissions = 1,
                Title = "The backlog",
                UserStories = new string[] { "A", "B", "C" }
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void HandleUserListRequestTest()
        {
            // Arrange
            var userList = new List<User>
            {
                new User { Username = "Peter", UserRole = UserRole.ProductOwner },
                new User { Username = "Fuchs", UserRole = UserRole.Developer }
            };
            var actualResult = new UserListResult();
            var expectedResult = new UserListResult { Users = userList.Select(p => new UserQueryResult(p)).ToArray() };
            Action<string> dummyFailureAction = s => { };
            _userRepo.Setup(p => p.GetExistingUsers(It.IsAny<Action<IEnumerable<User>>>(), dummyFailureAction))
                .Callback((Action<IEnumerable<User>> onSuccess, Action<string> onFailure) => onSuccess(userList));

            // Act
            _sut.HandleUserListRequest(result => { actualResult = result; }, dummyFailureAction);

            // Assert
            _userRepo.Verify(p => p.GetExistingUsers(It.IsAny<Action<IEnumerable<User>>>(), dummyFailureAction));
            actualResult.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void HandleUserEditRequest()
        {
            // Arrange
            var request = new UserEditRequest { Id = 111 };
            var user = new User { Id = 111, Username = "Peter", UserRole = UserRole.ProductOwner, PasswordHash = "blablabla" };
            Action<string> dummyFailureAction = s => { };
            var actualResult = new UserQueryResult();
            var expectedResult = new UserQueryResult
            {
                Id = user.Id,
                Username = user.Username,
                UserRole = user.UserRole
            };
            _userRepo.Setup(p => p.LoadUser(request.Id)).Returns((new Success(), user));

            // Act
            _sut.HandleUserEditRequest(request, result => actualResult = result, dummyFailureAction);

            // Assert
            _userRepo.Verify(p => p.LoadUser(request.Id));
            actualResult.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void HandleGetBacklogsSuccessfulTest()
        {
            IBacklogRepo repo = new BacklogRepoTest();
            ITotalOrder totalOrder = new TotalOrder();

            RequestHandler requestHandler = new RequestHandler(repo, totalOrder, null, null, null);

            var result = (Success<BacklogShowQueryResult>)requestHandler.HandleBacklogsShowRequest();

            Assert.That(result.Data.Backlogs.Count == 2);
            foreach (var backlog in result.Data.Backlogs)
            {
                Assert.That(backlog.UserStoryCount == 3);
                Assert.That(backlog.VoteCount == 1);
            }
        }

        [Test]
        public void HandleGetBacklogsFailureTest()
        {
            var repo = new Mock<BacklogRepoTest>();
            ITotalOrder totalOrder = new TotalOrder();

            repo.Setup(s => s.GetAll()).Throws(new Exception());

            var requestHandler = new RequestHandler(repo.Object, totalOrder, null, null, null);

            var result = requestHandler.HandleBacklogsShowRequest();

            Assert.That(result is Failure);
        }
    }

    public class BacklogRepoTest : IBacklogRepo
    {
        public string CreateBacklog(Backlog backlog)
        {
            return "7";
        }

        public Backlog ReadBacklog(string id)
        {
            var backlog = new Backlog
            {
                Id = "7",
                Title = "The backlog",
                UserStories = new string[] { "A", "B", "C" }
            };
            return backlog;
        }

        public void WriteSubmission(string id, Submission submission)
        {
            throw new NotImplementedException();
        }

        public virtual List<Backlog> GetAll()
        {
            var backlog = new Backlog
            {
                Id = "7",
                Title = "The backlog",
                UserStories = new string[] { "A", "B", "C" }
            };
            var backlog2 = new Backlog
            {
                Id = "9",
                Title = "The second backlog",
                UserStories = new string[] { "X", "Y", "Z" }
            };

            return new List<Backlog> {backlog, backlog2};
        }

        public void DeleteBacklog(string id)
        {
            throw new NotImplementedException();
        }

        public Submission[] ReadSubmissions(string id)
        {
            Submission submission = new Submission();
            submission.Indexes = new int[3];
            submission.Indexes[0] = 0;
            submission.Indexes[1] = 1;
            submission.Indexes[2] = 2;

            Submission[] result = new Submission[1];
            result[0] = submission;
            return result;
        }
    }
}
