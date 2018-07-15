using System;

namespace uhttpsharp.Attributes {
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class HttpMethodAttribute : Attribute {
        public HttpMethods HttpMethod { get; }

        public HttpMethodAttribute(HttpMethods httpMethod) {
            HttpMethod = httpMethod;
        }
    }
}