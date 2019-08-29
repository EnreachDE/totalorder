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
        private IConfiguration _configuration;

        private const string ErrorMessageString = "errorMessage";

        private readonly IRequestHandler _requestHandler;

        public LoginController(IConfiguration configuration,
                               IRequestHandlerFactory requestHandlerFactory)
        {
            this._configuration = configuration;
            this._requestHandler = requestHandlerFactory.GetHandler();
        }

        [Route("Login")]
        [HttpGet]
        public ViewResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (TempData != null && !string.IsNullOrEmpty((string)TempData[ErrorMessageString]))
            {
                ModelState.AddModelError("", (string)TempData[ErrorMessageString]);
            }
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> PostLogin(UserLoginViewModel model, string returnUrl)
        {
            return await TryLogin(new LoginRequest
            {
                Password = model.Password,
                Username = model.UserName
            }, returnUrl);
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
        public IActionResult GetCreateUser(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("CreateUser", new CreateUserModel());
        }

        [HttpPost]
        [Route("Login/Create")]
        public async Task<IActionResult> PostCreateUser(CreateUserModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateUser", model);
            }

            var (status, userList) = _requestHandler.HandleUserCreateRequest(new UserCreateRequest
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
                    TempData[ErrorMessageString] = f.ErrorMessage;
                    break;
            }

            if (!ModelState.IsValid)
            {
                return View("CreateUser", model);
            }

            var loginRequest = new LoginRequest
            {
                Password = model.Password,
                Username = model.Username
            };
            var redirectUrl = returnUrl ?? "/Home";
            return await TryLogin(loginRequest, redirectUrl);
        }

        private async Task<ActionResult> TryLogin(LoginRequest loginRequest, string returnUrl)
        {
            try
            {
                var redirectUrl = nameof(Login);

                var (status, userResult) = this._requestHandler.HandleLoginQuery(loginRequest);

                if (status is Failure failure)
                {
                    TempData[ErrorMessageString] = failure.ErrorMessage;
                }
                else
                {
                    await SignIn(userResult);
                    redirectUrl = returnUrl ?? "/Home";
                }

                return Redirect(redirectUrl);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        private async Task SignIn(UserLoginQueryResult user)
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
    }
}
