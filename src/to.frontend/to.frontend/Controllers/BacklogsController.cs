using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using to.contracts;
using to.contracts.data.domain;
using to.contracts.data.result;
using to.frontend.Factories;
using to.frontend.Models.Backlog;

namespace to.frontend.Controllers
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Internal;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;

    [Authorize(Roles = nameof(UserRole.Developer) +", "+ nameof(UserRole.ProductOwner) +", "+ nameof(UserRole.Administrator))]
    public class BacklogsController : Controller
    {
        private readonly IRequestHandler _handler;
        private readonly IHostingEnvironment _env;
        private const string ErrorMessageString = "errorMessage";

        public BacklogsController(IRequestHandlerFactory factory, IHostingEnvironment env)
        {
            _handler = factory.GetHandler();
            _env = env;
        }

        [HttpGet]
        [Route("Backlogs/Create")]
        [Authorize(Policy = nameof(Permission.CreateBacklog))]
        public ActionResult GetCreate()
        {
            var viewModel = new CreateBacklogViewModel();

            return View("Create", viewModel);
        }

        [HttpPost]
        [Route("Backlogs/Create")]
        [Authorize(Policy = nameof(Permission.CreateBacklog))]
        public ActionResult PostCreate(CreateBacklogViewModel model)
        {
            var request = Mapper.Map<BacklogCreationRequest>(model);
            request.UserId = User.GetId();

            var result = _handler.HandleBacklogCreationRequest(request);

            var evalModel = Mapper.Map<BacklogEvalQueryResult>(result.Item2);
            return RedirectToAction(nameof(Eval), new { evalModel.Id });
        }

        [HttpGet]
        [Route("Backlogs/Order/{id}")]
        [AllowAnonymous]
        public ActionResult GetOrder(string id)
        {
            var (status, backlogOrderQueryResult) = _handler.HandleBacklogOrderQuery(id);
            if (status is Failure f)
            {
                TempData[ErrorMessageString] = f.ErrorMessage;
                return View("Error");
            }

            if (backlogOrderQueryResult.OneVotePerUser && !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Login", new { returnUrl = HttpContext.Request.GetEncodedUrl() });
            }

            var viewModel = Mapper.Map<BacklogEvalViewModel>(backlogOrderQueryResult);

            return View("Order", viewModel);
        }

        [HttpPost]
        [Route("Backlogs/Order")]
        [AllowAnonymous]
        public ActionResult PostOrder(BacklogOrderRequestViewModel model)
        {
            var orderRequest = Mapper.Map<BacklogOrderRequest>(model);
            
            if (User.Identity.IsAuthenticated)
                orderRequest.UserId = User.GetId();
            
            var (status, result) = _handler.HandleBacklogOrderSubmissionRequest(orderRequest);
            if (status is Failure failure) { 
                TempData[ErrorMessageString] = failure.ErrorMessage;
                TempData["Environment"] = this._env.EnvironmentName;
                return View("Error");
            }

            var viewModel = Mapper.Map<BacklogEvalViewModel>(result);
            return RedirectToAction(nameof(Eval), new { viewModel.Id });
        }

        [HttpGet]
        [Route("Backlogs/Eval/{id}")]
        [AllowAnonymous]
        public ActionResult Eval(string id)
        {
            var (status, result) = _handler.HandleBacklogEvalQuery(id);
            if (status is Failure failure) { 
                TempData[ErrorMessageString] = failure.ErrorMessage;
                return Redirect("/Home");
            }
            else
            {
                var viewModel = Mapper.Map<BacklogEvalViewModel>(result);
                return View(viewModel);
            }
        }

        [HttpGet]
        [Route("Backlogs")]
        [Authorize(Policy = nameof(Permission.ListBacklog))]
        public IActionResult Index()
        {
            var userId = User.GetId();
            var (status, result) = _handler.HandleBacklogsShowRequest(userId);
            if (status is Failure failure) { 
                TempData[ErrorMessageString] = failure.ErrorMessage;
                return Redirect("/Home");
            }
            return View(new BacklogShowViewModel { Result = new Success(), Backlogs = result });
        }

        [HttpPost]
        [Route("Backlogs/Delete")]
        [Authorize(Policy = nameof(Permission.DeleteBacklog))]
        public IActionResult DeleteBacklog(string id)
        {
            int userId = User.GetId();
            var result = _handler.HandleBacklogDeleteRequest(id, userId);

            switch (result)
            {
                case Failure f:
                    TempData[ErrorMessageString] = f.ErrorMessage;
                    break;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}