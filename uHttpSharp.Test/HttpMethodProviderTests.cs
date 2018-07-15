using System;
using System.Collections.Generic;
using NUnit.Framework;
using uhttpsharp;

namespace uHttpSharp.Test {
    public class HttpMethodProviderTests {
        public static IEnumerable<object> Methods => Enum.GetNames(typeof(HttpMethods));

        private static IHttpMethodProvider GetTarget() {
            return new HttpMethodProvider();
        }

        [Test]
        [TestCase(HttpMethods.Connect)]
        [TestCase(HttpMethods.Delete)]
        [TestCase(HttpMethods.Get)]
        [TestCase(HttpMethods.Head)]
        [TestCase(HttpMethods.Options)]
        [TestCase(HttpMethods.Patch)]
        [TestCase(HttpMethods.Post)]
        [TestCase(HttpMethods.Put)]
        [TestCase(HttpMethods.Trace)]
        public void Should_Get_Right_Method(HttpMethods method) {
            // Arrange
            var methodName = Enum.GetName(typeof(HttpMethods), method);
            var target = GetTarget();

            // Act
            var actual = target.Provide(methodName);

            // Assert
            Assert.AreEqual(methodName, actual.ToString());
        }
    }
}