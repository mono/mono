using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebAssembly;
using WebAssembly.Net.Http.HttpClient;

namespace HttpStreamingTestSuite
{
    public class Program
    {
        private static GrpcChannel Channel { get; set; }

        const string target = "http://localhost:50051";
        public static void SetupGrpcChannel ()
        {
            var wasmHttpMessageHandlerType = System.Reflection.Assembly.Load("WebAssembly.Net.Http").GetType("WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler");
            var streamingProperty = wasmHttpMessageHandlerType.GetProperty("StreamingEnabled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            streamingProperty.SetValue(null, true, null);
 
            // Create a gRPC-Web channel pointing to the backend server.
            //
            // GrpcWebText is used because server streaming requires it. If server streaming is not used in your app
            // then GrpcWeb is recommended because it produces smaller messages.
            var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler()));

            Channel = GrpcChannel.ForAddress(target,
                new GrpcChannelOptions { HttpClient = httpClient });

        }

        private static int currentCount = 0;
        private static CancellationTokenSource? cts;

        private async Task<object> StartCancelTest()
        {
            var requestTcs = new TaskCompletionSource<object>();

            try
            {
                SetupGrpcChannel();

                cts = new CancellationTokenSource();

                var client = new Cancel.CancelTest.CancelTestClient(Channel);
                var call = client.StartTest(new Empty(), cancellationToken: cts.Token);

                await foreach (var message in call.ResponseStream.ReadAllAsync())
                {
                    currentCount = message.Count;
                    Console.WriteLine(currentCount);
                    if (currentCount >= 3)
                        break;
                }
                requestTcs.SetResult(true);

            }
            catch (Exception excd) when ((excd as Grpc.Core.RpcException)?.StatusCode == StatusCode.Internal)
            {
                requestTcs.SetException(excd);
            }
            catch (Exception excd)
            {
                //requestTcs.SetException(exc2);
                Console.WriteLine($"Exception only here: {excd}");
                requestTcs.SetResult(false);
            }
            
            return requestTcs.Task;

        }

        public async Task<int> StopAndTestIfCancelled ()
        {
            cts?.Cancel();
            cts = null;

            var client = new Cancel.CancelTest.CancelTestClient(Channel);
            var call = await client.IsTestCancelledAsync(new Empty());
            Console.WriteLine(call.Count);
            return call.Count;
        }
    }
}
