using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using to.contracts;
using to.contracts.data.domain;
using to.frontend.Factories;
using to.frontend.Models.Admin;
using System;
using to.frontend.Constants;

namespace to.frontend.Controllers
{
    using contracts.data.result;

    [Authorize(Roles = nameof(UserRole.Administrator))]
    public class AdminController : Controller
    {
        private readonly IRequestHandler _handler;

        public AdminController(IRequestHandlerFactory factory)
        {
            _handler = factory.GetHandler();
        }

        [Route("Admin")]
        [Authorize(Policy = nameof(Permission.ListUser))]
        public IActionResult Index()
        {
            var (status, userList) = _handler.HandleUserListRequest();

            switch (status)
            {
                case Failure f:
                    TempData[TempDataKeys.ErrorMessageString] = f.ErrorMessage;
                    break;
            }

            return View("Index", new Tuple<UserListResult,int>(userList,1));
        }

        [HttpGet]
        [Route("Admin/EditUser/{userId}")]
        [Authorize(Policy = nameof(Permission.EditUser))]
        public IActionResult GetEditUser(int userId)
        {
            var (status, user) = _handler.HandleUserEditRequest(new UserEditRequest {Id = userId});

            switch (status)
            {
                case Failure f:
                    TempData[TempDataKeys.ErrorMessageString] = f.ErrorMessage;
                    break;
            }

            return View("EditUser", user);
        }

        [HttpPost]
        [Route("Admin/EditUser/{userId}")]
        [Authorize(Policy = nameof(Permission.EditUser))]
        public IActionResult PostEditUser(int userId, EditUserModel model)
        {
            var (status, userList) = _handler.HandleUserUpdateRequest(new UserUpdateRequest
            {
                Id = model.Id,
                UserRole = model.UserRole
            });

            switch (status)
            {
                case Failure f:
                    TempData[TempDataKeys.ErrorMessageString] = f.ErrorMessage;
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
            if (!ModelState.IsValid)
            {
                return View("CreateUser", model);
            }

            var (status, userList) = _handler.HandleUserCreateRequest(new UserCreateRequest
            {
                UserName = model.Username,
                Password = model.Password,
                UserRole = model.UserRole
            });

            switch (status)
            {
                case Failure f:
                    ModelState.AddModelError(string.Empty, f.ErrorMessage);
                    TempData[TempDataKeys.ErrorMessageString] = f.ErrorMessage;
                    break;
            }

            if (!ModelState.IsValid)
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
            var (status, result) = _handler.HandleUserDeleteRequest(new UserDeleteRequest {Id = id});
            if (status is Failure f) TempData[TempDataKeys.ErrorMessageString] = f.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }
    }
}
