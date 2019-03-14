using System;
using System.Linq;
using to.contracts;
using to.contracts.data.domain;
using to.contracts.data.result;

namespace to.requesthandler
{
    using System.Collections.Generic;

    public class RequestHandler : IRequestHandler
    {
        private readonly IBacklogRepo _backlogrepo;
        private readonly ITotalOrder _totalorder;
        private readonly IUserRepo _userRepo;
        private readonly ISecurity _security;
        private readonly IPermissionRepo _permissionsRepo;

        public RequestHandler(IBacklogRepo backlogrepo, ITotalOrder totalOrder, IUserRepo userRepo, ISecurity security, IPermissionRepo permissionsRepo)
        {
            _backlogrepo = backlogrepo;
            _totalorder = totalOrder;
            _userRepo = userRepo;
            _security = security;
            _permissionsRepo = permissionsRepo;
        }

        public Status HandleBacklogDeleteRequest(string id)
        {
            _backlogrepo.DeleteBacklog(id);
            return new Success();
        }

        public Status HandleBacklogsShowRequest()
        {
            try
            {
                var backlogs = _backlogrepo.GetAll();
                var displayItems = backlogs.Select(Transform);
                return new Success<BacklogShowQueryResult>(new BacklogShowQueryResult(displayItems));
            }
            catch (Exception)
            {
                return new Failure("Error occured while retrieving backlogs.");
            }

            BacklogShowQueryResult.BacklogDisplayItem Transform(Backlog backlog)
            {
                var submissions = _backlogrepo.ReadSubmissions(backlog.Id);
                return BacklogShowQueryResult.BacklogDisplayItem.FromBacklog(backlog, submissions);
            }
        }

        public (Status, BacklogEvalQueryResult) HandleBacklogCreationRequest(BacklogCreationRequest request)
        {
            var backlog = new Backlog
            {
                Title = request.Title,
                UserStories = request.UserStories
            };

            string backlogid = _backlogrepo.CreateBacklog(backlog);
            return EvalSubmissions(backlogid);
        }

        private (Status, BacklogEvalQueryResult) EvalSubmissions(string backlogid)
        {
            var submissions = _backlogrepo.ReadSubmissions(backlogid);
            int[] currentOrder = _totalorder.Order(submissions);
            var backlog = _backlogrepo.ReadBacklog(backlogid);

            return (new Success(), new BacklogEvalQueryResult
            {
                Id = backlogid,
                Title = backlog.Title,
                UserStories = applyOrder(backlog.UserStories, currentOrder),
                NumberOfSubmissions = submissions.Length
            });
        }

        public static string[] applyOrder(string[] backlogUserStories, int[] currentOrder)
        {
            if (currentOrder.Length == 0)
                return backlogUserStories;

            string[] result = new string[backlogUserStories.Length];

            for (int i = 0; i < backlogUserStories.Length; i++)
            {
                result[i] = backlogUserStories[currentOrder[i]];
            }

            return result;
        }

        public (Status, BacklogEvalQueryResult) HandleBacklogEvalQuery(string id)
        {
            return EvalSubmissions(id);
        }

        public (Status, BacklogOrderQueryResult) HandleBacklogOrderQuery(string id)
        {
            var backlog = _backlogrepo.ReadBacklog(id);

            var result = new BacklogOrderQueryResult
            {
                Id = backlog.Id,
                Title = backlog.Title,
                UserStories = backlog.UserStories,
                UserStoryIndexes = new int[backlog.UserStories.Length]
            };

            for (int i = 0; i < backlog.UserStories.Length; i++)
            {
                result.UserStoryIndexes[i] = i;
            }

            return (new Success(), result);
        }

        public (Status, BacklogEvalQueryResult) HandleBacklogOrderSubmissionRequest(BacklogOrderRequest request)
        {
            var submission = new Submission() { Indexes = request.UserStoryIndexes };
            _backlogrepo.WriteSubmission(request.Id, submission);

            return EvalSubmissions(request.Id);
        }

        public (Status, UserLoginQueryResult) HandleLoginQuery(LoginRequest request)
        {
            var (status, user) = _userRepo.LoadUser(request.Username);
            if (status is Failure) return (status, null);

            status = _security.ValidatePassword(request.Password, user.PasswordHash);
            if (status is Failure) return (status, null);

            var (statusP, permissions) = _permissionsRepo.LoadPermissions(user.UserRole);
            if (statusP is Failure) return (statusP, null);

            return (new Success(), new UserLoginQueryResult(user, permissions.ToList()));
        }

        public (Status, UserListResult) HandleUserUpdateRequest(UserUpdateRequest request)
        {
            var status = _userRepo.UpdateUser(request.Id, request.UserRole);
            if (status is Failure) return (status, null);

            return LoadUsers();
        }

        private (Status, UserListResult) LoadUsers()
        {
            var (status, users) = _userRepo.GetExistingUsers();
            if (status is Failure) return (new Failure("Could not retrieve existing users."), null);

            var usersList = users.Select(p => new UserQueryResult(p)).ToArray();

            return (new Success(), new UserListResult {Users = usersList});
        }

        public (Status, UserQueryResult) HandleUserEditRequest(UserEditRequest request)
        {
            var (status, user) =_userRepo.LoadUser(request.Id);
            if (status is Failure) return (status, null);

            return (new Success(), new UserQueryResult(user));
        }

        public (Status, UserListResult) HandleUserListRequest()
        {
            var (status, userList) = _userRepo.GetExistingUsers();
            if (status is Failure) return (status, null);

            var userListResult = new UserListResult
            {
                Users = userList.Select(p => new UserQueryResult(p)).ToArray()
            };

            return (new Success(), userListResult);
        }

        public (Status, UserListResult) HandleUserCreateRequest(UserCreateRequest request)
        {
            var hashedPassword = _security.HashPassword(request.Password);
            var user = new User
            {
                PasswordHash = hashedPassword,
                UserRole = request.UserRole,
                Username = request.UserName
            };

            var status = _userRepo.AddUser(user);
            if (status is Failure) return (status, null);

            return LoadUsers();
        }

        public (Status, UserListResult) HandleUserDeleteRequest(UserDeleteRequest request)
        {
            var (status, result) = _userRepo.DeleteUser(request.Id);
            if (status is Failure) return (status, null);

            return (new Success(), CreateUserListResult(result));
        }

        private static UserListResult CreateUserListResult(IEnumerable<User> result)
        {
            var userListResult = new UserListResult
            {
                Users = result.Select(p => new UserQueryResult(p)).ToArray()
            };
            return userListResult;
        }
    }
}
