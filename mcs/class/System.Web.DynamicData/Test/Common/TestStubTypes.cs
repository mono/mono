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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web.Routing;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

using NUnit.Framework;
using MonoTests.DataSource;

namespace MonoTests.System.Web.DynamicData
{
    class MyDynamicDataRoute : DynamicDataRoute
    {
        public RouteValueDictionary GetVirtualPathValues {
            get; private set;
        }

        public bool GetVirtualPathCalled {
            get;
            set;
        }

        public MyDynamicDataRoute (string url)
            : base (url)
        { }

        public override VirtualPathData GetVirtualPath (RequestContext requestContext, RouteValueDictionary values)
        {
            GetVirtualPathValues = values;
            GetVirtualPathCalled = true;
            return base.GetVirtualPath (requestContext, values);
        }
    }

	class MyDataContext1 : DataContext
	{
		public MyDataContext1 ()
			: base (new SqlConnection ("Data Source=localhost"))
		{
		}
	}

	[Database (Name = "MyDB1")]
	class MyDataContext2 : DataContext
	{
		public MyDataContext2 ()
			: base (new SqlConnection ("Data Source=localhost"))
		{
		}

		public Table<Foo> FooTable { get { return GetTable<Foo> (); } }
	}

    class MyDataContext3 : MyDataContext2
    {
    }

	class UseOnlyInGetModelTestDataContext : MyDataContext2
	{
	}

	[Table (Name = "dbo...FooTable")]
	partial class Foo
	{
		public Foo () : this (false)
		{
		}

		public Foo (bool noThrow)
		{
			if (!noThrow)
				throw new Exception ("ERROR");
		}

		[Column (Name = "Col1")]
		public string Column1 { get; set; }
	}
    
	[Table (Name = "BarTable")]
	class Bar
	{
		[Column (Name = "Col1")]
		public string Column1 { get; set; }

		[Column (Name = "Col2")]
		public string Column2 { get; set; }
	}

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
		NameValueCollection query_string = new NameValueCollection ();

		public override string AppRelativeCurrentExecutionFilePath {
			get { return req_path ?? base.AppRelativeCurrentExecutionFilePath; }
		}

		public override string PathInfo {
			get { return path_info ?? base.PathInfo; }
		}

		public override NameValueCollection QueryString {
			get { return query_string; }
		}

		public override string HttpMethod {
			get { return Method; }
		}

		public string Method { get; set; }
	}

	class MyDictionary : Hashtable
	{
		public override ICollection Keys {
			get { return null; }
		}

		public override object this [object key] {
			get {
				//Console.Error.WriteLine ("Get: {0} {1}", key, key.GetHashCode ());
				return base [key];
			}
			set {
				//Console.Error.WriteLine ("Set: {0} {1} = {2}", key, key.GetHashCode (), value);
				base [key] = value; 
			}
		}
	}

	class HttpContextStub2 : HttpContextBase
	{
		public HttpContextStub2 ()
			: this (null, null)
		{
		}

		public HttpContextStub2 (string requestUrl, string path)
			: this (requestUrl, path, null)
		{
		}

		public HttpContextStub2 (string requestUrl, string path, string appPath)
		{
			request = new HttpRequestStub2 (requestUrl, path, appPath);
		}

		Hashtable items = new MyDictionary ();
		HttpRequestStub request;
		HttpResponseBase response;

		public override IDictionary Items {
			get { return items; }
		}

		public override HttpRequestBase Request {
			get { return request ?? base.Request; }
		}

		public override HttpResponseBase Response {
			get { return response ?? base.Response; }
		}

		public override void RewritePath (string path)
		{
			throw new ApplicationException (path);
		}

		public void SetResponse (HttpResponseBase response)
		{
			this.response = response;
		}
	}

	class HttpRequestStub2 : HttpRequestStub
	{
		public HttpRequestStub2 (string dummyRequestPath, string dummyPath, string appPath)
			: base (dummyRequestPath, String.Empty)
		{
			path = dummyPath;
			app_path = appPath;
		}

		string path, app_path;

		public override string ApplicationPath {
			get { return app_path ?? base.ApplicationPath; }
		}

		public override string Path {
			get { return path ?? base.Path; }
		}
	}

	public class HttpResponseStub : HttpResponseBase
	{
		public HttpResponseStub ()
			: this (0)
		{
		}

		int impl_type;

		public HttpResponseStub (int implType)
		{
			this.impl_type = implType;
		}

		public override string ApplyAppPathModifier (string virtualPath)
		{
			switch (impl_type) {
				case 3:
					return virtualPath; // pass thru
				case 2:
					return virtualPath + "_modified";
				case 1:
					throw new ApplicationException (virtualPath);
				default:
					return base.ApplyAppPathModifier (virtualPath);
			}
		}
	}

	class HttpContextStub3 : HttpContextStub2
	{
		public HttpContextStub3 (string requestUrl, string path, string appPath, bool supportHandler)
			: base (requestUrl, path, appPath)
		{
			this.support_handler = supportHandler;
		}

		public override void RewritePath (string path)
		{
			RewrittenPath = path;
		}

		bool support_handler;
		public IHttpHandler HttpHandler { get; set; }

		public override IHttpHandler Handler {
			get { return support_handler ? HttpHandler : base.Handler; }
			set {
				if (support_handler)
					HttpHandler = value;
				else
					base.Handler = value;
			}
		}

		public string RewrittenPath { get; set; }
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
			throw new ApplicationException ("MyUrlRoutingHandler");
		}
	}

	public class ErrorRouteHandler : IRouteHandler
	{
		public IHttpHandler GetHttpHandler (RequestContext requestContext)
		{
			throw new ApplicationException ("ErrorRouteHandler");
		}
	}

	public class MyRouteHandler : IRouteHandler
	{
		public MyRouteHandler ()
			: this (new MyHttpHandler ())
		{
		}

		public MyRouteHandler (IHttpHandler handler)
		{
			this.handler = handler;
		}

		IHttpHandler handler;

		public IHttpHandler GetHttpHandler (RequestContext requestContext)
		{
			return handler;
		}
	}

	public class MyHttpHandler : IHttpHandler
	{
		public bool IsReusable {
			get { return true; }
		}

		public void ProcessRequest (HttpContext ctx)
		{
			throw new MyException ("HOGE");
		}
	}

	public class MyException : Exception
	{
		public MyException (string msg) : base (msg) {}
	}

	public class NullRouteHandler : IRouteHandler
	{
		public IHttpHandler GetHttpHandler (RequestContext requestContext)
		{
			return null;
		}
	}

	public class MyRoute : Route
	{
		public MyRoute (string url, IRouteHandler handler)
			: this (url, handler, null)
		{
		}

		public MyRoute (string url, IRouteHandler handler, Exception ex)
			: base (url, handler)
		{
			this.ex = ex;
		}

		Exception ex;

		public override VirtualPathData GetVirtualPath (RequestContext requestContext, RouteValueDictionary values)
		{
			if (ex != null)
				throw ex;
			return base.GetVirtualPath (requestContext, values);
		}
	}

	class MyHttpWorkerRequest : HttpWorkerRequest
	{
		public override void EndOfRequest ()
		{
			throw new NotImplementedException ();
		}

		public override void FlushResponse (bool b)
		{
			throw new NotImplementedException ();
		}

		public override string GetHttpVerbName ()
		{
			throw new NotImplementedException ();
		}

		public override string GetHttpVersion ()
		{
			throw new NotImplementedException ();
		}

		public override string GetLocalAddress ()
		{
			throw new NotImplementedException ();
		}

		public override int GetLocalPort ()
		{
			throw new NotImplementedException ();
		}

		public override string GetQueryString ()
		{
			throw new NotImplementedException ();
		}

		public override string GetRawUrl ()
		{
			throw new NotImplementedException ();
		}

		public override string GetRemoteAddress ()
		{
			throw new NotImplementedException ();
		}

		public override int GetRemotePort ()
		{
			throw new NotImplementedException ();
		}

		public override string GetUriPath ()
		{
			return "/Foo/Bar";
		}

		public override void SendKnownResponseHeader (int idx, string v)
		{
			throw new NotImplementedException ();
		}

		public override void SendResponseFromFile (IntPtr file, long offset, long len)
		{
			throw new NotImplementedException ();
		}

		public override void SendResponseFromFile (string name, long offset, long len)
		{
			throw new NotImplementedException ();
		}

		public override void SendResponseFromMemory (byte[] buf, int index)
		{
			throw new NotImplementedException ();
		}

		public override void SendStatus (int i, string s)
		{
			throw new NotImplementedException ();
		}

		public override void SendUnknownResponseHeader (string n, string v)
		{
			throw new NotImplementedException ();
		}
	}
}
