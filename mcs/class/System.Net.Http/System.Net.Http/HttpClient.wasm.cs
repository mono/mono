using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Net.Http
{
    public partial class HttpClient
    {

        private static Func<HttpMessageHandler> GetHttpMessageHandler;

        public HttpClient()
            : this(GetDefaultHandler(), true)
        {
        }

        static HttpMessageHandler GetDefaultHandler()
        {

            if (GetHttpMessageHandler == null)
                return GetFallback("No custom HttpClientHandler registered");

            var handler = GetHttpMessageHandler();
            if (handler == null)
                return GetFallback($"Custom HttpMessageHandler is not valid");

            return handler;
        }

        static HttpMessageHandler GetFallback(string message)
        {
            //Console.WriteLine(message + ". Defaulting to System.Net.Http.HttpClientHandler");
            return new HttpClientHandler();
        }
    }
}