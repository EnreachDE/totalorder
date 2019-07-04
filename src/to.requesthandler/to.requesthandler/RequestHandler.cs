namespace to.requesthandler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using contracts;
    using contracts.data.domain;
    using contracts.data.result;

    public class RequestHandler : IRequestHandler
    {
        private readonly IBacklogRepo backlogRepo;
        private readonly IPermissionRepo permissionsRepo;
        private readonly ISecurity security;
        private readonly ITotalOrder totalOrder;
        private readonly IUserRepo userRepo;

        public RequestHandler(IBacklogRepo backlogRepo, ITotalOrder totalOrder, IUserRepo userRepo, ISecurity security,
            IPermissionRepo permissionsRepo)
        {
            this.backlogRepo = backlogRepo;
            this.totalOrder = totalOrder;
            this.userRepo = userRepo;
            this.security = security;
            this.permissionsRepo = permissionsRepo;
        }

        public Status HandleBacklogDeleteRequest(string backlogId, int userId)
        {
            var status = this.userRepo.DeleteUserBacklogId(userId, backlogId);
            if (status is Failure)
            {
                return status;
            }

            this.backlogRepo.DeleteBacklog(backlogId);

            return new Success();
        }

        public Status HandleBacklogsShowRequest()
        {
            try
            {
                var backlogs = this.backlogRepo.GetAll();
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
            var (status, backlogIds) = this.userRepo.GetUserBacklogIds(userId);
            if (status is Failure)
            {
                return (status, null);
            }

            var backlogs = this.backlogRepo.GetBacklogsByIds(backlogIds);
            var displayItems = backlogs.Select(Transform);
            return (new Success(), new BacklogShowQueryResult(displayItems));
        }

        public (Status, BacklogEvalQueryResult) HandleBacklogCreationRequest(BacklogCreationRequest request)
        {
            var backlog = new Backlog
            {
                Title = request.Title,
                UserStories = request.UserStories
            };

            var backlogId = this.backlogRepo.CreateBacklog(backlog);
            var status = this.userRepo.AddUserBacklogId(request.UserId, backlogId);
            if (status is Failure)
            {
                return (status, null);
            }

            return EvalSubmissions(backlogId);
        }

        public (Status, BacklogEvalQueryResult) HandleBacklogEvalQuery(string id)
        {
            return EvalSubmissions(id);
        }

        public (Status, BacklogOrderQueryResult) HandleBacklogOrderQuery(string id)
        {
            var backlog = this.backlogRepo.ReadBacklog(id);

            var result = new BacklogOrderQueryResult
            {
                Id = backlog.Id,
                Title = backlog.Title,
                UserStories = backlog.UserStories,
                UserStoryIndexes = new int[backlog.UserStories.Length]
            };

            for (var i = 0; i < backlog.UserStories.Length; i++) result.UserStoryIndexes[i] = i;

            return (new Success(), result);
        }

        public (Status, BacklogEvalQueryResult) HandleBacklogOrderSubmissionRequest(BacklogOrderRequest request)
        {
            var submission = new Submission {Indexes = request.UserStoryIndexes};
            this.backlogRepo.WriteSubmission(request.Id, submission);

            return EvalSubmissions(request.Id);
        }

        public (Status, UserLoginQueryResult) HandleLoginQuery(LoginRequest request)
        {
            var (status, user) = this.userRepo.LoadUser(request.Username);
            if (status is Failure)
            {
                return (status, null);
            }

            status = this.security.ValidatePassword(request.Password, user.PasswordHash);
            if (status is Failure)
            {
                return (status, null);
            }

            var (statusP, permissions) = this.permissionsRepo.LoadPermissions(user.UserRole);
            if (statusP is Failure)
            {
                return (statusP, null);
            }

            return (new Success(), new UserLoginQueryResult(user, permissions.ToList()));
        }

        public (Status, UserListResult) HandleUserUpdateRequest(UserUpdateRequest request)
        {
            var status = this.userRepo.UpdateUser(request.Id, request.UserRole);
            return status is Failure ? (status, null) : LoadUsers();
        }

        public (Status, UserQueryResult) HandleUserEditRequest(UserEditRequest request)
        {
            var (status, user) = this.userRepo.LoadUser(request.Id);
            if (status is Failure)
            {
                return (status, null);
            }

            return (new Success(), new UserQueryResult(user));
        }

        public (Status, UserListResult) HandleUserListRequest()
        {
            var (status, userList) = this.userRepo.GetExistingUsers();
            if (status is Failure)
            {
                return (status, null);
            }

            var userListResult = new UserListResult
            {
                Users = userList.Select(p => new UserQueryResult(p)).ToArray()
            };

            return (new Success(), userListResult);
        }

        public (Status, UserListResult) HandleUserCreateRequest(UserCreateRequest request)
        {
            var hashedPassword = this.security.HashPassword(request.Password);
            var user = new User
            {
                PasswordHash = hashedPassword,
                UserRole = request.UserRole,
                Username = request.UserName
            };

            var status = this.userRepo.AddUser(user);
            return status is Failure ? (status, null) : LoadUsers();
        }

        public (Status, UserListResult) HandleUserDeleteRequest(UserDeleteRequest request)
        {
            var (status, result) = this.userRepo.DeleteUser(request.Id);
            if (status is Failure)
            {
                return (status, null);
            }

            return (new Success(), CreateUserListResult(result));
        }

        private (Status, BacklogEvalQueryResult) EvalSubmissions(string backlogId)
        {
            var submissions = this.backlogRepo.ReadSubmissions(backlogId);
            var currentOrder = this.totalOrder.Order(submissions);
            var backlog = this.backlogRepo.ReadBacklog(backlogId);

            return (new Success(), new BacklogEvalQueryResult
            {
                Id = backlogId,
                Title = backlog.Title,
                UserStories = ApplyOrder(backlog.UserStories, currentOrder),
                NumberOfSubmissions = submissions.Length
            });
        }

        public static string[] ApplyOrder(string[] backlogUserStories, int[] currentOrder)
        {
            if (currentOrder.Length == 0)
            {
                return backlogUserStories;
            }

            var result = new string[backlogUserStories.Length];

            for (var i = 0; i < backlogUserStories.Length; i++) result[i] = backlogUserStories[currentOrder[i]];

            return result;
        }

        private (Status, UserListResult) LoadUsers()
        {
            var (status, users) = this.userRepo.GetExistingUsers();
            if (status is Failure)
            {
                return (new Failure("Could not retrieve existing users."), null);
            }

            var usersList = users.Select(p => new UserQueryResult(p)).ToArray();

            return (new Success(), new UserListResult {Users = usersList});
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
            var submissions = this.backlogRepo.ReadSubmissions(backlog.Id);
            return BacklogShowQueryResult.BacklogDisplayItem.FromBacklog(backlog, submissions);
        }
    }
}