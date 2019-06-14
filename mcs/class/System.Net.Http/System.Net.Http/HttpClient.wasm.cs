using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using WebAssembly.Net.Http.HttpClient;

namespace System.Net.Http
{
    public partial class HttpClient
    {

#pragma warning disable 649
        private static Func<HttpMessageHandler> GetHttpMessageHandler;
#pragma warning restore 649

        internal static HttpMessageHandler CreateDefaultHandler()
        {

            if (GetHttpMessageHandler == null)
            {
                return WasmHttpMessageHandler.GetHttpMessageHandler();
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
