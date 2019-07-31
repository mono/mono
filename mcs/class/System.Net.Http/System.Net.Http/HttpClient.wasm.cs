using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Net.Http
{
    public partial class HttpClient
    {
        internal static HttpMessageHandler CreateDefaultHandler()
        {

            string envvar = Environment.GetEnvironmentVariable ("WASM_HTTP_CLIENT_HANDLER_TYPE")?.Trim ();
            if (String.IsNullOrEmpty (envvar))
            {
                Type type = Type.GetType("WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler, WebAssembly.Net.Http");
                if (type == null)
                    throw new PlatformNotSupportedException ("Invalid WebAssembly Module? Cannot find WebAssembly.Net.Http.HttpClient.WasmHttpMessageHandler");

                MethodInfo method = type.GetMethod("GetHttpMessageHandler", BindingFlags.Static | BindingFlags.NonPublic);
                if (method == null)
                    throw new PlatformNotSupportedException ("Your WebAssembly version does not support obtaining of the custom HttpClientHandler.");

                object ret = method.Invoke(null, null);
                if (ret == null)
                    throw new PlatformNotSupportedException ("WebAssembly returned no custom HttpClientHandler.");

                var handler = ret as HttpMessageHandler;
                if (handler == null)
                    throw new PlatformNotSupportedException ($"{ret?.GetType()} is not a valid HttpMessageHandler.");
                    
                return handler;

            }
            else
            {
                Type handlerType = Type.GetType (envvar, false);
                if (handlerType == null)
                    throw new PlatformNotSupportedException ($"{handlerType} can not be found.");
                object ret = Activator.CreateInstance (handlerType);
                if (ret == null)
                    throw new PlatformNotSupportedException ("WebAssembly returned no custom HttpClientHandler.");

                var handler = ret as HttpMessageHandler;
                if (handler == null)
                    throw new PlatformNotSupportedException ($"{ret?.GetType()} is not a valid HttpMessageHandler.");
                    
                return handler;
            }                
        }
    }
}
