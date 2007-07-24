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

		#region NameValueCollectionDictionary

		sealed class NameValueCollectionDictionary : JavaScriptSerializer.LazyDictionary
		{
			readonly NameValueCollection _nmc;
			public NameValueCollectionDictionary (NameValueCollection nmc) {
				_nmc = nmc;
			}
			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				for (int i = 0, max = _nmc.Count; i < max; i++)
					yield return new KeyValuePair<string, object> (_nmc.GetKey (i), _nmc.Get (i));
			}
		}

		#endregion

		#region ExceptionSerializer

		sealed class ExceptionSerializer : JavaScriptSerializer.LazyDictionary
		{
			readonly Exception _e;
			public ExceptionSerializer (Exception e) {
				_e = e;
			}
			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				yield return new KeyValuePair<string, object> ("Message", _e.Message);
				yield return new KeyValuePair<string, object> ("StackTrace", _e.StackTrace);
				yield return new KeyValuePair<string, object> ("ExceptionType", _e.GetType ().FullName);
			}
		}

		#endregion

		readonly LogicalTypeInfo.LogicalMethodInfo _logicalMethodInfo;

		private RestHandler (HttpContext context, Type type, string filePath) {
			LogicalTypeInfo logicalTypeInfo = LogicalTypeInfo.GetLogicalTypeInfo (type, filePath);
			HttpRequest request = context.Request;
			if (logicalTypeInfo == null || request.PathInfo.Length < 2)
				ThrowInvalidOperationException (request.PathInfo);

			_logicalMethodInfo = logicalTypeInfo [request.PathInfo.Substring (1)];
			if (_logicalMethodInfo == null)
				ThrowInvalidOperationException (request.PathInfo);
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
				? new NameValueCollectionDictionary (request.QueryString) :
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

		#endregion
	}
}
