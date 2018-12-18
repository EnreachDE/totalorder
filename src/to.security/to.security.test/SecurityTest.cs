using System;
using NUnit.Framework;

namespace to.security.test
{
    [TestFixture]
    public class SecurityTest
    {
        [Test]
        public void TestMethod1()
        {
            string passwordToValidate = "abc";
            string hashedPassword = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

            Security security = new Security();
            var hash = security.HashPassword(passwordToValidate);

            Assert.AreEqual(hashedPassword, hash);
        }
        [Test]
        public void TestMethod2()
        {
            string passwordToValidate = "kaese";
            string hashedPassword = "3f8a6d01e9cc99d6106a3eae08b2cd00d3ce1152edd1589b213156c7f6c41923";

            Security security = new Security();
            var hash = security.HashPassword(passwordToValidate);

            Assert.AreEqual(hashedPassword, hash);
        }
    }
}
