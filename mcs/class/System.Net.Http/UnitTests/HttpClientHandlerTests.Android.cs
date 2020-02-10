// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Net.Http;

using Xunit;
using Xunit.Abstractions;

namespace System.Net.Http.Tests
{
	public class HttpClientHandlerTestsAndroid
	{
		static Type GetInnerHandlerType (HttpClient httpClient)
		{
			BindingFlags bflasgs = BindingFlags.Instance | BindingFlags.NonPublic;
			FieldInfo handlerField = typeof (HttpMessageInvoker).GetField("_handler", bflasgs);
			Assert.NotNull (handlerField);
			object handler = handlerField.GetValue (httpClient);
			FieldInfo innerHandlerField = handler.GetType ().GetField ("_delegatingHandler", bflasgs);
			Assert.NotNull (handlerField);
			object innerHandler = innerHandlerField.GetValue (handler);
			return innerHandler.GetType ();
		}

		[Fact]
		public void TestEnvVarSwitchForInnerHttpHandler ()
		{
			const string xaHandlerKey = "XA_HTTP_CLIENT_HANDLER_TYPE";
			var prevHandler = Environment.GetEnvironmentVariable (xaHandlerKey);

			// ""
			Environment.SetEnvironmentVariable (xaHandlerKey, "");
			var httpClient1 = new HttpClient ();
			Assert.Equal ("SocketsHttpHandler", GetInnerHandlerType (httpClient1).Name);

			var handler2 = new HttpClientHandler ();
			var httpClient2 = new HttpClient (handler2);
			Assert.Equal ("SocketsHttpHandler", GetInnerHandlerType (httpClient2).Name);

			// "System.Net.Http.MonoWebRequestHandler"
			Environment.SetEnvironmentVariable (xaHandlerKey, "System.Net.Http.MonoWebRequestHandler");
			var httpClient3 = new HttpClient ();
			Assert.Equal ("MonoWebRequestHandler", GetInnerHandlerType (httpClient3).Name);

			var handler4 = new HttpClientHandler ();
			var httpClient4 = new HttpClient (handler4);
			Assert.Equal ("MonoWebRequestHandler", GetInnerHandlerType (httpClient4).Name);

			// "System.Net.Http.MonoWebRequestHandler, System.Net.Http"
			Environment.SetEnvironmentVariable (xaHandlerKey, "System.Net.Http.MonoWebRequestHandler, System.Net.Http");
			var httpClient5 = new HttpClient ();
			Assert.Equal ("MonoWebRequestHandler", GetInnerHandlerType (httpClient5).Name);

			var handler6 = new HttpClientHandler ();
			var httpClient6 = new HttpClient (handler6);
			Assert.Equal ("MonoWebRequestHandler", GetInnerHandlerType (httpClient6).Name);

			// "System.Net.Http.HttpClientHandler"
			Environment.SetEnvironmentVariable (xaHandlerKey, "System.Net.Http.HttpClientHandler");
			var httpClient7 = new HttpClient ();
			Assert.Equal ("SocketsHttpHandler", GetInnerHandlerType (httpClient7).Name);

			var handler8 = new HttpClientHandler ();
			var httpClient8 = new HttpClient (handler8);
			Assert.Equal ("SocketsHttpHandler", GetInnerHandlerType (httpClient8).Name);

			Environment.SetEnvironmentVariable (xaHandlerKey, prevHandler);
		}
	}
}
