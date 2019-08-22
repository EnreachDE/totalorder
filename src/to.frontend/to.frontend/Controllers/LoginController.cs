namespace to.frontend.Controllers
{
    using contracts.data.result;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using contracts.data.domain;

    using to.contracts;
    using to.frontend.Constants;
    using to.frontend.Factories;
    using to.frontend.Models.Login;

    public class LoginController : Controller
    {
        private IConfiguration configuration;

        private const string errorMessageString = "errorMessage";

        private IRequestHandler requestHandler;

        public LoginController(IConfiguration configuration,
                               IRequestHandlerFactory requestHandlerFactory)
        {
            this.configuration = configuration;
            this.requestHandler = requestHandlerFactory.GetHandler();
        }

        [Route("Login")]
        [HttpGet]
        public ViewResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (TempData != null && !string.IsNullOrEmpty((string)TempData[errorMessageString]))
            {
                ModelState.AddModelError("", (string)TempData[errorMessageString]);
            }
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> PostLogin(UserLoginViewModel model, string returnUrl)
        {
            try
            {
                var redirectUrl = nameof(Login);
                var loginRequest = new LoginRequest
                {
                    Password = model.Password,
                    Username = model.UserName
                };

                var (status, userResult) = this.requestHandler.HandleLoginQuery(loginRequest);

                if (status is Failure failure) { TempData[errorMessageString] = failure.ErrorMessage; }
                else
                {
                    await CreateCookie(userResult);
                    redirectUrl = returnUrl ?? "/Home";
                }

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["Exception"] = ex;
                TempData["Model"] = model;
                return View("Error");
            }
        }

        private async Task CreateCookie(UserLoginQueryResult user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.UserRole.ToString()),
                new Claim(CustomClaims.UserId, user.Id.ToString())
            };

            foreach (var permission in user.Permissions)
            {
                claims.Add(new Claim(CustomClaims.Permission, permission.ToString()));
            }

            var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(userIdentity),
                new AuthenticationProperties { IsPersistent = true });
        }

        [HttpGet]
        [Route("Logout")]
        public async Task<ActionResult> GetLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Home");
        }

        [HttpGet]
        [Route("Register")]
        public ActionResult GetRegister()
        {
            return View("Login");
        }

        [HttpGet]
        [Route("Login/Create")]
        public IActionResult GetCreateUser()
        {
            return View("CreateUser", new CreateUserModel());
        }

        [HttpPost]
        [Route("Login/Create")]
        public IActionResult PostCreateUser(CreateUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateUser", model);
            }

            var (status, userList) = requestHandler.HandleUserCreateRequest(new UserCreateRequest
            {
                UserName = model.Username,
                Password = model.Password,
                // TODO: After reduction of the role system we should assign "User" here (PO will be removed)
                UserRole = UserRole.ProductOwner
            });

            switch (status)
            {
                case Failure f:
                    ModelState.AddModelError(string.Empty, f.ErrorMessage);
                    TempData[errorMessageString] = f.ErrorMessage;
                    break;
            }

            if (!ModelState.IsValid)
            {
                return View("CreateUser", model);
            }

            return RedirectToAction(nameof(Login));
        }
    }
}
