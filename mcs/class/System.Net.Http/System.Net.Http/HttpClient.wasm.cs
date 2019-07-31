using System;
using System.Reflection;
using System.Runtime.CompilerServices;

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
                Type type = Type.GetType("WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler, WebAssembly.Net.Http");
                if (type == null)
                    return GetFallback ("Invalid WebAssembly Module? Cannot find WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler");

                MethodInfo method = type.GetMethod("GetHttpMessageHandler", BindingFlags.Static | BindingFlags.NonPublic);
                if (method == null)
                    return GetFallback ("Your WebAssembly version does not support obtaining of the custom HttpClientHandler");

                object ret = method.Invoke(null, null);
                if (ret == null)
                    return GetFallback ("WebAssembly returned no custom HttpClientHandler");

                var handler = ret as HttpMessageHandler;
                if (handler == null)
                    return GetFallback ($"{ret?.GetType()} is not a valid HttpMessageHandler");
                    
                return handler;

            }
            else
            {
                var handler = GetHttpMessageHandler();
                if (handler == null)
                    return GetFallback($"Custom HttpMessageHandler is not valid");

                return handler;
            }                
        }

        static HttpMessageHandler GetFallback(string message)
        {
            //Console.WriteLine(message + ". Defaulting to System.Net.Http.HttpClientHandler");
            return new HttpClientHandler();
        }
    }
}
