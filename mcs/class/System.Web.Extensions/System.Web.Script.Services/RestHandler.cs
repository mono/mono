//
// RestHandler.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;
using System.Collections.Specialized;
using System.IO;
using System.Web.SessionState;
using System.Reflection;

namespace System.Web.Script.Services
{
	sealed class RestHandler : IHttpHandler
	{
		#region SessionWrappers

		class SessionWrapperHandler : IHttpHandler, IRequiresSessionState
		{
			readonly IHttpHandler _handler;

			public SessionWrapperHandler (IHttpHandler handler) {
				_handler = handler;
			}

			public bool IsReusable {
				get { return _handler.IsReusable; }
			}

			public void ProcessRequest (HttpContext context) {
				_handler.ProcessRequest (context);
			}
		}

		sealed class ReadOnlySessionWrapperHandler : SessionWrapperHandler, IReadOnlySessionState
		{
			public ReadOnlySessionWrapperHandler (IHttpHandler handler) : base (handler) { }
		}

		#endregion

		#region ExceptionSerializer

		sealed class ExceptionSerializer
		{
			readonly Exception _e;
			public ExceptionSerializer (Exception e) {
				_e = e;
			}

			public string Message {
				get { return _e.Message; }
			}

			public string StackTrace {
				get { return _e.StackTrace; }
			}

			public string ExceptionType {
				get { return _e.GetType ().FullName; }
			}			
		}

		#endregion

		readonly LogicalTypeInfo.LogicalMethodInfo _logicalMethodInfo;

		private RestHandler (HttpContext context, Type type, string filePath) {
			LogicalTypeInfo logicalTypeInfo = LogicalTypeInfo.GetLogicalTypeInfo (type, filePath);
			HttpRequest request = context.Request;
			string methodName = request.PathInfo.Substring (1);
			if (logicalTypeInfo == null || String.IsNullOrEmpty(methodName))
				ThrowInvalidOperationException (methodName);

			_logicalMethodInfo = logicalTypeInfo [methodName];
			if (_logicalMethodInfo == null)
				ThrowInvalidOperationException (methodName);
		}

		static void ThrowInvalidOperationException (string pathInfo) {
			throw new InvalidOperationException (
					string.Format ("Request format is unrecognized unexpectedly ending in '{0}'.", pathInfo));
		}

		static readonly Type IRequiresSessionStateType = typeof (IRequiresSessionState);
		static readonly Type IReadOnlySessionStateType = typeof (IReadOnlySessionState);
		public static IHttpHandler GetHandler (HttpContext context, Type type, string filePath) {
			RestHandler handler = new RestHandler (context, type, filePath);
			LogicalTypeInfo.LogicalMethodInfo mi = handler._logicalMethodInfo;
			if (mi.MethodInfo.IsStatic) {
				if (IRequiresSessionStateType.IsAssignableFrom (type))
					return IReadOnlySessionStateType.IsAssignableFrom (type) ?
						new ReadOnlySessionWrapperHandler (handler) : new SessionWrapperHandler (handler);
			}
			else
				if (mi.WebMethod.EnableSession)
					return new SessionWrapperHandler (handler);

			return handler;

		}
		#region IHttpHandler Members

		public bool IsReusable {
			get { return false; }
		}
		
		public void ProcessRequest (HttpContext context) {
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			response.ContentType =
				_logicalMethodInfo.ScriptMethod.ResponseFormat == ResponseFormat.Json ?
				"application/json" : "text/xml";
			response.Cache.SetCacheability (HttpCacheability.Private);
			response.Cache.SetMaxAge (TimeSpan.Zero);

			IDictionary<string, object> @params =
				"GET".Equals (request.RequestType, StringComparison.OrdinalIgnoreCase)
				? GetNameValueCollectionDictionary (request.QueryString) :
				(IDictionary<string, object>) JavaScriptSerializer.DefaultSerializer.DeserializeObjectInternal
				(new StreamReader (request.InputStream, request.ContentEncoding));
			
			try {
				_logicalMethodInfo.Invoke (@params, response.Output);
			}
			catch (TargetInvocationException e) {
				response.AddHeader ("jsonerror", "true");
				response.ContentType = "application/json";
				response.StatusCode = 500;
				JavaScriptSerializer.DefaultSerializer.Serialize (new ExceptionSerializer (e.GetBaseException ()), response.Output);
				response.End ();
			}
		}

		IDictionary <string, object> GetNameValueCollectionDictionary (NameValueCollection nvc)
		{
			var ret = new Dictionary <string, object> ();

			for (int i = nvc.Count - 1; i >= 0; i--)
				ret.Add (nvc.GetKey (i), JavaScriptSerializer.DefaultSerializer.DeserializeObjectInternal (nvc.Get (i)));

			return ret;
		}
		
		#endregion
	}
}
