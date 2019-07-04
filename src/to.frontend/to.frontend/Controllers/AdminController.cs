namespace to.frontend.Controllers
{
    using System;

    using contracts;
    using contracts.data.domain;
    using contracts.data.result;

    using Factories;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using Models.Admin;

    [Authorize(Roles = nameof(UserRole.Administrator))]
    public class AdminController : Controller
    {
        private const string ErrorMessageString = "errorMessage";
        private readonly IRequestHandler handler;

        public AdminController(IRequestHandlerFactory factory)
        {
            this.handler = factory.GetHandler();
        }

        [Route("Admin")]
        [Authorize(Policy = nameof(Permission.ListUser))]
        public IActionResult Index()
        {
            var (status, userList) = this.handler.HandleUserListRequest();

            switch (status)
            {
                case Failure f:
                    this.TempData[ErrorMessageString] = f.ErrorMessage;
                    break;
            }

            return View("Index", new Tuple<UserListResult, int>(userList, 1));
        }

        [HttpGet]
        [Route("Admin/EditUser/{userId}")]
        [Authorize(Policy = nameof(Permission.EditUser))]
        public IActionResult GetEditUser(int userId)
        {
            var (status, user) = this.handler.HandleUserEditRequest(new UserEditRequest {Id = userId});

            switch (status)
            {
                case Failure f:
                    this.TempData[ErrorMessageString] = f.ErrorMessage;
                    break;
            }

            return View("EditUser", user);
        }

        [HttpPost]
        [Route("Admin/EditUser/{userId}")]
        [Authorize(Policy = nameof(Permission.EditUser))]
        public IActionResult PostEditUser(int userId, EditUserModel model)
        {
            var (status, _) = this.handler.HandleUserUpdateRequest(new UserUpdateRequest
            {
                Id = model.Id,
                UserRole = model.UserRole
            });

            switch (status)
            {
                case Failure f:
                    this.TempData[ErrorMessageString] = f.ErrorMessage;
                    break;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("Admin/CreateUser")]
        [Authorize(Policy = nameof(Permission.CreateUser))]
        public IActionResult GetCreateUser()
        {
            return View("CreateUser", new CreateUserModel());
        }

        [HttpPost]
        [Route("Admin/CreateUser")]
        [Authorize(Policy = nameof(Permission.CreateUser))]
        public IActionResult PostCreateUser(CreateUserModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View("CreateUser", model);
            }

            var (status, userList) = this.handler.HandleUserCreateRequest(new UserCreateRequest
            {
                UserName = model.Username,
                Password = model.Password,
                UserRole = model.UserRole
            });

            switch (status)
            {
                case Failure f:
                    this.ModelState.AddModelError(string.Empty, f.ErrorMessage);
                    this.TempData[ErrorMessageString] = f.ErrorMessage;
                    break;
            }

            if (!this.ModelState.IsValid)
            {
                return View("CreateUser", model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("Admin/DeleteUser/")]
        [Authorize(Policy = nameof(Permission.DeleteUser))]
        public IActionResult DeleteUser(int id)
        {
            var (status, result) = this.handler.HandleUserDeleteRequest(new UserDeleteRequest {Id = id});
            if (status is Failure f)
            {
                this.TempData[ErrorMessageString] = f.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}