using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using to.contracts;
using to.contracts.data.result;
using to.frontend.Controllers;
using to.frontend.Factories;
using to.frontend.Models.Backlog;

namespace to.frontend.tests
{
    [TestFixture]
    public class BacklogControllerTests
    {
        private static readonly string[] DummyUserStories = { "UserStory1", "UserStory2" };
        private Mock<IRequestHandlerFactory> _handlerFactory;
        private Mock<IRequestHandler> _handler;
        private BacklogsController _sut;
        private const string Dummytitle = "dummyTitle";

        [SetUp]
        public void Initialize()
        {
            AutoMapper.Mapper.Initialize(config =>
            {
                config.CreateMap<BacklogCreationRequest, CreateBacklogViewModel>().ReverseMap();
            });
            _handlerFactory = new Mock<IRequestHandlerFactory>();
            _handler = new Mock<IRequestHandler>();
            _sut = new BacklogsController(_handlerFactory.Object);
        }

        [Test]
        public void PostCreationRequestIsHandled()
        {
            const string dummyid = "dummyId";
            BacklogCreationRequest actualRequest = null;
            var model = CreateDummyCreateModel();
            var expectedRequest = new BacklogCreationRequest
            {
                Title = Dummytitle,
                UserStories = DummyUserStories
            };
            _handler.Setup(p => p.HandleBacklogCreationRequest(It.IsAny<BacklogCreationRequest>()))
                .Returns((new Success(),new BacklogEvalQueryResult { Id = dummyid }))
                .Callback<BacklogCreationRequest>(r => actualRequest = r);
            _handlerFactory.Setup(p => p.GetHandler()).Returns(_handler.Object);
            _sut = new BacklogsController(_handlerFactory.Object);

            var result = (RedirectToActionResult)_sut.PostCreate(model);

            actualRequest.Should().BeEquivalentTo(expectedRequest);
            result.ActionName.Should().BeEquivalentTo(nameof(BacklogsController.Eval));
            result.RouteValues["Id"].Should().BeEquivalentTo(dummyid);
        }

        private static CreateBacklogViewModel CreateDummyCreateModel()
        {
            return new CreateBacklogViewModel
            {
                Title = Dummytitle,
                UserStories = DummyUserStories
            };
        }
    }
}
