//
// TestStubTypes.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Web;
using System.Web.Routing;
using NUnit.Framework;

namespace MonoTests.System.Web.Routing
{
	class HttpContextStub : HttpContextBase
	{
		HttpRequestStub req;

		public HttpContextStub ()
			: this (null)
		{
		}

		public HttpContextStub (string dummyRequestPath)
			: this (dummyRequestPath, null)
		{
		}

		public HttpContextStub (string dummyRequestPath, string pathInfo)
			: this (dummyRequestPath, pathInfo, null)
		{
		}

		public HttpContextStub (string dummyRequestPath, string pathInfo, string method)
		{
			if (dummyRequestPath != null) {
				req = new HttpRequestStub (dummyRequestPath, pathInfo);
				req.Method = method;
			}
		}

		public override HttpRequestBase Request {
			get { return req != null ? req : base.Request; }
		}
	}

	class HttpRequestStub : HttpRequestBase
	{
		public HttpRequestStub (string dummyRequestPath, string pathInfo)
		{
			req_path = dummyRequestPath;
			path_info = pathInfo;
		}

		string req_path, path_info;

		public override string AppRelativeCurrentExecutionFilePath {
			get { return req_path ?? base.AppRelativeCurrentExecutionFilePath; }
		}

		public override string PathInfo {
			get { return path_info ?? base.PathInfo; }
		}

		public override string HttpMethod {
			get { return Method; }
		}

		public string Method { get; set; }
	}

	public class MyStopRoutingHandler : StopRoutingHandler
	{
		public IHttpHandler CallGetHttpHandler (RequestContext rc)
		{
			return GetHttpHandler (rc);
		}
	}

	public class MyUrlRoutingHandler : UrlRoutingHandler
	{
		public void DoProcessRequest (HttpContextBase httpContext)
		{
			ProcessRequest (httpContext);
		}

		protected override void VerifyAndProcessRequest (IHttpHandler httpHandler, HttpContextBase httpContext)
		{
			throw new ApplicationException ();
		}
	}

	public class ErrorRouteHandler : IRouteHandler
	{
		public IHttpHandler GetHttpHandler (RequestContext requestContext)
		{
			throw new ApplicationException ();
		}
	}
}
