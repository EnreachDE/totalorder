using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using to.contracts;
using to.contracts.data.domain;
using to.contracts.data.result;
using to.frontend.Factories;
using to.frontend.Models;
using to.frontend.Models.Home;

namespace to.frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApplicationState _applicationState;
        private readonly IRequestHandler _handler;

        public HomeController(IApplicationState applicationState, IRequestHandlerFactory factory)
        {
            _applicationState = applicationState;
            _handler = factory.GetHandler();
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                AdminUserExists = CheckAdminUserExists()
            };
            return View(model);
        }

        protected virtual bool CheckAdminUserExists()
        {
            var adminUserExists = _applicationState.Get<bool>("AdminUserExists");
            if (!adminUserExists)
            {
                var (status, result) = _handler.HandleUserListRequest();
                if (status is Success && result.Users.Length != 0)
                {
                    _applicationState.Set("AdminUserExists", true);
                    adminUserExists = true;
                }
            }

            return adminUserExists;
        }

        [HttpPost]
        [Route("Home/CreateAdmin")]
        public IActionResult PostCreateAdministrator(HomeViewModel model)
        {
            var adminUserExists = _applicationState.Get<bool>("AdminUserExists");
            if (!adminUserExists)
            {
                _applicationState.Set("AdminUserExists", true);
                _handler.HandleUserCreateRequest(new UserCreateRequest
                {
                    UserName = model.Username,
                    Password = model.Password,
                    UserRole = UserRole.Administrator
                });
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
