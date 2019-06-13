using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Net.Http
{
    public partial class HttpClient
    {

#pragma warning disable 649
        private static Func<HttpMessageHandler> GetHttpMessageHandler;
        // Used to skip the type lookup on later calls.
        private static MethodInfo messageHandlerMethod;
#pragma warning restore 649

        internal static HttpMessageHandler CreateDefaultHandler()
        {

            if (GetHttpMessageHandler == null)
            {
                if (messageHandlerMethod == null) {
                    try {
                        Type type = Assembly.Load("WebAssembly.Net.Http")?.GetType("WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler");
                        if (type == null)
                            throw new PlatformNotSupportedException ("Invalid WebAssembly Module? Cannot find WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler");

                        messageHandlerMethod = type.GetMethod("GetHttpMessageHandler", BindingFlags.Static | BindingFlags.NonPublic);
                    }
                    catch { 
                        throw new PlatformNotSupportedException ("Invalid WebAssembly Module? Cannot find WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler");
                    }
                }

                if (messageHandlerMethod == null)
                    throw new PlatformNotSupportedException ("Your WebAssembly version does not support obtaining of the custom HttpClientHandler");

                object ret = messageHandlerMethod.Invoke(null, null);
                if (ret == null)
                    throw new PlatformNotSupportedException ("WebAssembly returned no custom HttpClientHandler");

                var handler = ret as HttpMessageHandler;
                if (handler == null)
                    throw new PlatformNotSupportedException ($"{ret?.GetType()} is not a valid HttpMessageHandler");

                return handler;

            }
            else
            {
                var handler = GetHttpMessageHandler();
                if (handler == null)
                    throw new PlatformNotSupportedException ($"Custom HttpMessageHandler is not valid");

                return handler;
            }
        }
    }
}
