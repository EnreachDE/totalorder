using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using to.contracts;
using to.contracts.data.domain;
using to.frontend.Factories;
using to.frontend.Models.Admin;
using System;

namespace to.frontend.Controllers
{
    [Authorize(Roles = nameof(UserRole.Administrator))]
    public class AdminController : Controller
    {
        private readonly IRequestHandler _handler;
        private const string ErrorMessageString = "errorMessage";

        public AdminController(IRequestHandlerFactory factory)
        {
            _handler = factory.GetHandler();
        }

        [Route("Admin")]
        [Authorize(Policy = nameof(Permission.ListUser))]
        public IActionResult Index()
        {
            var userList = new UserListResult();
            _handler.HandleUserListRequest(
                // Success
                userListResult => { userList = userListResult; },
                // Failure
                errorMessage => { TempData[ErrorMessageString] = errorMessage; }
            );
            
            return View("Index", new Tuple<UserListResult,int>(userList, 1));
        }

        [HttpGet]
        [Route("Admin/EditUser/{userId}")]
        [Authorize(Policy = nameof(Permission.EditUser))]
        public IActionResult GetEditUser(int userId)
        {
            UserQueryResult user = null;
            _handler.HandleUserEditRequest(new UserEditRequest {Id = userId},
                // Success
                userQueryResult => { user = userQueryResult; },
                // Failure
                errorMessage => { TempData[ErrorMessageString] = errorMessage; }
            );
            return View("EditUser", user);
        }

        [HttpPost]
        [Route("Admin/EditUser/{userId}")]
        [Authorize(Policy = nameof(Permission.EditUser))]
        public IActionResult PostEditUser(int userId, EditUserModel model)
        {
            _handler.HandleUserUpdateRequest(new UserUpdateRequest
                {
                    Id = model.Id,
                    UserRole = model.UserRole
                },
                // Success
                userListResult => {},
                // Failure
                errorMessage => { TempData[ErrorMessageString] = errorMessage; }
            );

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

            _handler.HandleUserCreateRequest(new UserCreateRequest
                {
                    UserName = model.Username,
                    Password = model.Password,
                    UserRole = model.UserRole
                },
                userListResult => { },
                errorMessage =>
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                    TempData[ErrorMessageString] = errorMessage;
                }
            );

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
            _handler.HandleUserDeleteRequest(new UserDeleteRequest
                {
                    Id = id
                },
                // Success
                userListResult => { },
                // Failure
                errorMessage => { TempData[ErrorMessageString] = errorMessage; }
            );

            return RedirectToAction(nameof(Index));
        }
    }
}
