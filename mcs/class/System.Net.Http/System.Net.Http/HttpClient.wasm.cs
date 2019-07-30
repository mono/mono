using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Net.Http
{
    public partial class HttpClient
    {

        internal static HttpMessageHandler CreateDefaultHandler()
        {
                object ret = WebAssembly.RuntimeOptions.GetHttpMessageHandler ();
                if (ret == null)
                    throw new PlatformNotSupportedException ($"Custom HttpMessageHandler is not valid");

                var handler = ret as HttpMessageHandler;
                if (handler == null)
                    throw new PlatformNotSupportedException ($"{ret?.GetType()} is not a valid HttpMessageHandler");
                return handler;
        }
    }
}
