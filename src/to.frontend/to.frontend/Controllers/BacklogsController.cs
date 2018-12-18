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
    [Authorize(Roles = nameof(UserRole.Developer) +", "+ nameof(UserRole.ProductOwner) +", "+ nameof(UserRole.Administrator))]
    public class BacklogsController : Controller
    {
        private readonly IRequestHandler _handler;
        private const string ErrorMessageString = "errorMessage";

        public BacklogsController(IRequestHandlerFactory factory)
        {
            _handler = factory.GetHandler();
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

            var result = _handler.HandleBacklogCreationRequest(request);

            var evalModel = Mapper.Map<BacklogEvalQueryResult>(result);
            return RedirectToAction(nameof(Eval), new { evalModel.Id });
        }

        [HttpGet]
        [Route("Backlogs/Order/{id}")]
        [Authorize(Policy = nameof(Permission.OrderBacklog))]
        public ActionResult GetOrder(string id)
        {
            var backlog = _handler.HandleBacklogOrderQuery(id);
            var viewModel = Mapper.Map<BacklogEvalViewModel>(backlog);

            return View("Order", viewModel);
        }

        [HttpPost]
        [Route("Backlogs/Order")]
        [Authorize(Policy = nameof(Permission.OrderBacklog))]
        public ActionResult PostOrder(BacklogOrderRequestViewModel model)
        {
            var orderRequest = Mapper.Map<BacklogOrderRequest>(model);
            var result = _handler.HandleBacklogOrderSubmissionRequest(orderRequest);

            var viewModel = Mapper.Map<BacklogEvalViewModel>(result);

            return RedirectToAction(nameof(Eval), new { viewModel.Id });
        }

        [HttpGet]
        [Route("Backlogs/Eval/{id}")]
        [Authorize(Policy = nameof(Permission.EvaluateBacklog))]
        public ActionResult Eval(string id)
        {
            var result = _handler.HandleBacklogEvalQuery(id);
            var viewModel = Mapper.Map<BacklogEvalViewModel>(result);
            return View(viewModel);
        }

        [HttpGet]
        [Route("Backlogs")]
        [Authorize(Policy = nameof(Permission.ListBacklog))]
        public IActionResult Index()
        {
            switch(_handler.HandleBacklogsShowRequest())
            {
                case Success<BacklogShowQueryResult> s:
                    return View(new BacklogShowViewModel { Result = new Success(), Backlogs = s.Data });
                default:
                    return View(new BacklogShowViewModel { Result = new Failure(), Backlogs = null });
            }
        }

        [HttpPost]
        [Route("Backlogs/Delete")]
        [Authorize(Policy = nameof(Permission.DeleteBacklog))]
        public IActionResult DeleteBacklog(string id)
        {
            var result = _handler.HandleBacklogDeleteRequest(id);

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