using System;
using System.Linq;
using to.contracts;
using to.contracts.data.domain;
using to.contracts.data.result;

namespace to.requesthandler
{
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
                return new Failure();
            }

            BacklogShowQueryResult.BacklogDisplayItem Transform(Backlog backlog)
            {
                var submissions = _backlogrepo.ReadSubmissions(backlog.Id);
                return BacklogShowQueryResult.BacklogDisplayItem.FromBacklog(backlog, submissions);
            }
        }

        public BacklogEvalQueryResult HandleBacklogCreationRequest(BacklogCreationRequest request)
        {
            var backlog = new Backlog
            {
                Title = request.Title,
                UserStories = request.UserStories
            };

            string backlogid = _backlogrepo.CreateBacklog(backlog);
            return EvalSubmissions(backlogid);
        }

        private BacklogEvalQueryResult EvalSubmissions(string backlogid)
        {
            var submissions = _backlogrepo.ReadSubmissions(backlogid);
            int[] currentOrder = _totalorder.Order(submissions);
            var backlog = _backlogrepo.ReadBacklog(backlogid);

            return new BacklogEvalQueryResult
            {
                Id = backlogid,
                Title = backlog.Title,
                UserStories = applyOrder(backlog.UserStories, currentOrder),
                NumberOfSubmissions = submissions.Length
            };
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

        public BacklogEvalQueryResult HandleBacklogEvalQuery(string id)
        {
            return EvalSubmissions(id);
        }

        public BacklogOrderQueryResult HandleBacklogOrderQuery(string id)
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

            return result;
        }

        public BacklogEvalQueryResult HandleBacklogOrderSubmissionRequest(BacklogOrderRequest request)
        {
            var submission = new Submission() { Indexes = request.UserStoryIndexes };
            _backlogrepo.WriteSubmission(request.Id, submission);

            return EvalSubmissions(request.Id);

        }

        public void HandleLoginQuery(LoginRequest request, Action<UserLoginQueryResult> OnSuccess, Action<string> OnFailure)
        {
            _userRepo.LoadUser(request.Username,
                user => _security.ValidatePassword(request.Password, user.PasswordHash,
                    () =>
                    {
                        var (status, permissions) = _permissionsRepo.LoadPermissions(user.UserRole);
                        if (status is Success)
                        {
                            OnSuccess(new UserLoginQueryResult(user, permissions.ToList()));
                        }
                        else
                        {
                            OnFailure("Could not load permissions");
                        }
                    },
                    OnFailure),
                OnFailure);
        }

        public void HandleUserUpdateRequest(UserUpdateRequest request, Action<UserListResult> OnSuccess,
            Action<string> OnFailure)
        {
            _userRepo.UpdateUser(request.Id, 
                request.UserRole, 
                () => LoadUsers(OnSuccess, OnFailure), 
                OnFailure);
        }

        private void LoadUsers(Action<UserListResult> OnSuccess, Action<string> OnFailure)
        {
            _userRepo.GetExistingUsers(users =>
                {
                    var usersList = users.Select(p => new UserQueryResult(p)).ToArray();
                    var result = new UserListResult { Users = usersList };
                    OnSuccess(result);
                }, OnFailure);
        }

        public void HandleUserEditRequest(UserEditRequest request, Action<UserQueryResult> OnSuccess,
            Action<string> OnFailure)
        {
            _userRepo.LoadUser(
                request.Id,
                user => OnSuccess(new UserQueryResult(user)),
                OnFailure);
        }

        public void HandleUserListRequest(Action<UserListResult> OnSuccess, Action<string> OnFailure)
        {
            _userRepo.GetExistingUsers(userList =>
            {
                var userListResult = new UserListResult
                {
                    Users = userList.Select(p => new UserQueryResult(p)).ToArray()
                };
                OnSuccess(userListResult);
            }, OnFailure);
        }

        public void HandleUserCreateRequest(UserCreateRequest request, Action<UserListResult> OnSuccess, Action<string> OnFailure)
        {
            var hashedPassword = _security.HashPassword(request.Password);
            var user = new User
            {
                PasswordHash = hashedPassword,
                UserRole = request.UserRole,
                Username = request.UserName
            };

            _userRepo.AddUser(user,
                () => LoadUsers(OnSuccess, OnFailure), 
                OnFailure);
        }

        public void HandleUserDeleteRequest(UserDeleteRequest request, Action<UserListResult> OnSuccess, Action<string> OnFailure)
        {
            _userRepo.DeleteUser(request.Id, userList =>
            {
                var userListResult = new UserListResult
                {
                    Users = userList.Select(p => new UserQueryResult(p)).ToArray()
                };
                OnSuccess(userListResult);
            }, OnFailure);
        }
    }
}
