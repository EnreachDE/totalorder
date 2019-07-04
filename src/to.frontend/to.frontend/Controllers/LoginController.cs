namespace to.frontend.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Constants;

    using contracts;
    using contracts.data.result;

    using Factories;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    using Models.Login;

    public class LoginController : Controller
    {
        private const string ErrorMessageString = "errorMessage";

        private readonly IRequestHandler requestHandler;
        private IConfiguration configuration;

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
            this.ViewBag.ReturnUrl = returnUrl;
            if (this.TempData != null && !string.IsNullOrEmpty((string) this.TempData[ErrorMessageString]))
            {
                this.ModelState.AddModelError("", (string) this.TempData[ErrorMessageString]);
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

                if (status is Failure failure)
                {
                    this.TempData[ErrorMessageString] = failure.ErrorMessage;
                }
                else
                {
                    await CreateCookie(userResult);
                    redirectUrl = returnUrl ?? "/Home";
                }

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                this.TempData["Exception"] = ex;
                this.TempData["Model"] = model;
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
                claims.Add(new Claim(CustomClaims.Permission, permission.ToString()));

            var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await this.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(userIdentity),
                new AuthenticationProperties {IsPersistent = true});
        }

        [HttpGet]
        [Route("Logout")]
        public async Task<ActionResult> GetLogout()
        {
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Home");
        }

        [HttpGet]
        [Route("Register")]
        public ActionResult GetRegister()
        {
            return View("Login");
        }
    }
}