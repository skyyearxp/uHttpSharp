using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace uhttpsharp.Demo.Handlers {
    public class TimingHandler : IHttpRequestHandler {
        private static ILogger _logger;

        public TimingHandler(ILogger logger) {
            _logger = logger;
        }

        public async Task Handle(IHttpContext context, Func<Task> next) {
            var stopWatch = Stopwatch.StartNew();
            await next();

            _logger.Info($"request {context.Request.Uri} took {stopWatch.Elapsed}");
        }
    }
}