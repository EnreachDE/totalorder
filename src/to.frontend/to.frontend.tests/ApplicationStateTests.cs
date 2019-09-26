using FluentAssertions;
using NUnit.Framework;
using to.frontend.Factories;
using to.frontend.Helper;

namespace to.frontend.tests
{
    [TestFixture]
    public class ApplicationStateTests
    {
        private ApplicationState _sut;
        
        [SetUp]
        public void Initialize()
        {
            _sut = new ApplicationState();
        }

        [Test]
        public void AddAndGetBooleanFromApplicationState()
        {
            _sut.Set<bool>("test", true);
            var result = _sut.Get<bool>("test");

            result.Should().Be(true);
        }
    }
}
