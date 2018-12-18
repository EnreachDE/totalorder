using FluentAssertions;
using NUnit.Framework;
using to.contracts.data.domain;
using to.totalorder;

namespace TotalOrderTest
{
    [TestFixture]
    public class TotalOrderTest
    {
        [Test]
        public void TestOrder1()
        {
            TotalOrder totalOrder = new TotalOrder();
            Submission[] submissions = new[]
            {
                 new Submission{ Indexes =  new[]{ 1,2,0 } },
                 new Submission{ Indexes =  new[]{ 0,2,1 } }
            };
            int[] testRef = new int[] { 0, 1, 2 };

            var result = totalOrder.Order(submissions);
            result.Should().Equal(testRef);
        }

        [Test]
        public void TestOrder2()
        {
            TotalOrder totalOrder = new TotalOrder();
            Submission[] submissions = new[]
            {
                 new Submission{ Indexes =  new[]{ 3,4,1,0,2 } },
                 new Submission{ Indexes =  new[]{ 0,2,1,3,4 } }
            };
            int[] testRef = new int[] { 0, 3, 1, 2, 4 };

            var result = totalOrder.Order(submissions);
            result.Should().Equal(testRef);
        }

        [Test]
        public void TestOrder3()
        {
            TotalOrder totalOrder = new TotalOrder();
            Submission[] submissions = new[]
            {
                 new Submission{ Indexes =  new[]{ 2,1,3,0,4 } },
                 new Submission{ Indexes =  new[]{ 1,2,3,4,0 } },
                 new Submission{ Indexes =  new[]{ 3,2,1,4,0 } }
            };
            int[] testRef = new int[] { 2, 1, 3, 4, 0 };

            var result = totalOrder.Order(submissions);
            result.Should().Equal(testRef);
        }
    }
}
