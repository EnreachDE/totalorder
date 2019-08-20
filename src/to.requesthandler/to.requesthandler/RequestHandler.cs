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

        public Status HandleBacklogDeleteRequest(string backlogId, int userId)
        {
            var status = _userRepo.DeleteUserBacklogId(userId, backlogId);
            if (status is Failure) return status;

            _backlogrepo.DeleteBacklog(backlogId);

            return new Success();
        }

        public Status HandleBacklogsShowRequest()
        {
            try
            {
                var (status, backlogs) = _backlogrepo.GetAll();
                if (status is Failure) return status;

                var displayItems = backlogs.Select(Transform);
                return new Success<BacklogShowQueryResult>(new BacklogShowQueryResult(displayItems));
            }
            catch (Exception)
            {
                return new Failure("Error occured while retrieving backlogs.");
            }
        }

        public (Status, BacklogShowQueryResult) HandleBacklogsShowRequest(int userId)
        {
            var (status, backlogIds) = _userRepo.GetUserBacklogIds(userId);
            if (status is Failure) return (status, null);

            var (status2, backlogs) = _backlogrepo.GetBacklogsByIds(backlogIds);
            if (status2 is Failure) return (status2, null);

            var displayItems = backlogs.Select(Transform);
            return (new Success(), new BacklogShowQueryResult(displayItems));
        }

        public (Status, BacklogEvalQueryResult) HandleBacklogCreationRequest(BacklogCreationRequest request)
        {
            var backlog = new Backlog
            {
                Title = request.Title,
                UserStories = request.UserStories,
                OneVotePerUser = request.OneVotePerUser
            };

            var (status, backlogId) = _backlogrepo.CreateBacklog(backlog);
            if (status is Failure) return (status, null);

            status = _userRepo.AddUserBacklogId(request.UserId, backlogId);
            if (status is Failure) return (status, null);

            return EvalSubmissions(backlogId);
        }

        private (Status, BacklogEvalQueryResult) EvalSubmissions(string backlogId)
        {
            var (status, submissions) = _backlogrepo.ReadSubmissions(backlogId);
            if (status is Failure) return (status, null);

            int[] currentOrder = _totalorder.Order(submissions);

            var (status2, backlog) = _backlogrepo.ReadBacklog(backlogId);
            if (status2 is Failure) return (status2, null);

            return (new Success(), new BacklogEvalQueryResult
            {
                Id = backlogId,
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
            var (status, backlog) = _backlogrepo.ReadBacklog(id);
            if (status is Failure) return (status, null);

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
            var submission = new Submission()
            {
                Indexes = request.UserStoryIndexes,
                UserId = request.UserId
            };
            
            var status =_backlogrepo.WriteSubmission(request.BacklogId, submission);
            if (status is Failure) return (status, null);

            return EvalSubmissions(request.BacklogId);
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

        private BacklogShowQueryResult.BacklogDisplayItem Transform(Backlog backlog)
        {
            var (status, submissions) = _backlogrepo.ReadSubmissions(backlog.Id);
            if (status is Failure) return null;

            return BacklogShowQueryResult.BacklogDisplayItem.FromBacklog(backlog, submissions);
        }
    }
}
