
using Cancel;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace BrowserGrpcTestsService.Services
{
    public class CancelTestService : CancelTest.CancelTestBase
    {

        static bool CounterTestCancelled = false;

        private readonly ILogger<CancelTestService> _logger;
        public CancelTestService(ILogger<CancelTestService> logger)
        {
            _logger = logger;
        }

        public override async Task StartTest(Empty request, IServerStreamWriter<CancelTestResponse> responseStream, ServerCallContext context)
        {
            var count = 0;

            // Attempt to run until canceled by the client
            // Blazor WA is unable to cancel a call that has started - https://github.com/mono/mono/issues/18717
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(new CancelTestResponse
                {
                    Count = ++count
                });
                Console.WriteLine($"Count: {count}");
                await Task.Delay(TimeSpan.FromSeconds(0.5));
            }
            Console.WriteLine($"We are here 1: {CounterTestCancelled}");
            CounterTestCancelled = true;
            Console.WriteLine($"We are here 2: {CounterTestCancelled}");
        }

        public override Task<CancelTestResponse> IsTestCancelled(Empty request, ServerCallContext context)
        {
            Console.WriteLine($"We are here 3: {CounterTestCancelled}");

            var cResponse = CounterTestCancelled ? new CancelTestResponse { Count = 1 } : new CancelTestResponse { Count = 0 };
            return Task.FromResult(cResponse);
        }
    }
}
