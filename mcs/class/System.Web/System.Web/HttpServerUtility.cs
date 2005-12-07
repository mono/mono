//
// System.Web.HttpRequest.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Web.UI;
using System.Web.Util;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Web {

	//
	// Methods exposed through HttpContext.Server property
	//
	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpServerUtility {
		HttpContext context;
		
		internal HttpServerUtility (HttpContext context)
		{
			this.context = context;
		}

		public void ClearError ()
		{
			context.ClearError ();
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public object CreateObject (string progID)
		{
			throw new HttpException (500, "COM is not supported");
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public object CreateObject (Type type)
		{
			throw new HttpException (500, "COM is not supported");
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public object CreateObjectFromClsid (string clsid)
		{
			throw new HttpException (500, "COM is not supported");
		}

		public void Execute (string path)
		{
			Execute (path, null);
		}

		public void Execute (string path, TextWriter writer)
		{
			Execute (path, writer, false);
		}

#if NET_2_0
		public
#else
		internal
#endif
		void Execute (string path, TextWriter writer, bool preserveQuery)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path.IndexOf (':') != -1)
				throw new ArgumentException ("Invalid path.");

			int qmark = path.IndexOf ('?');
			string query;
			if (qmark != -1) {
				query = path.Substring (qmark + 1);
				path = path.Substring (0, qmark);
			} else {
				query = "";
			}

			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

			string oldQuery = request.QueryStringRaw;
			request.QueryStringRaw = query;

			WebROCollection oldForm = null;
			if (!preserveQuery) {
				oldForm = request.Form as WebROCollection;
				request.SetForm (new WebROCollection ());
			}

			TextWriter output = writer;
			if (output == null)
			 	output = response.Output;

			string oldFilePath = request.FilePath;
			request.SetCurrentExePath (UrlUtils.Combine (request.BaseVirtualDir, path));
			IHttpHandler handler = context.ApplicationInstance.GetHandler (context);
			TextWriter previous = null;
			try {
				previous = response.SetTextWriter (output);
				if (!(handler is IHttpAsyncHandler)) {
					handler.ProcessRequest (context);
				} else {
					IHttpAsyncHandler asyncHandler = (IHttpAsyncHandler) handler;
					IAsyncResult ar = asyncHandler.BeginProcessRequest (context, null, null);
					ar.AsyncWaitHandle.WaitOne ();
					asyncHandler.EndProcessRequest (ar);
				}
			} finally {
				request.SetCurrentExePath (oldFilePath);
				request.QueryStringRaw = oldQuery;
				response.SetTextWriter (previous);
				if (!preserveQuery)
					request.SetForm (oldForm);
			}
		}

		public Exception GetLastError ()
		{
			if (context == null)
				return HttpContext.Current.Error;
			return context.Error;
		}

		public string HtmlDecode (string s)
		{
			return HttpUtility.HtmlDecode (s);
		}

		public void HtmlDecode (string s, TextWriter output)
		{
			HttpUtility.HtmlDecode (s, output);
		}

		public string HtmlEncode (string s)
		{
			return HttpUtility.HtmlEncode (s);
		}

		public void HtmlEncode (string s, TextWriter output)
		{
			HttpUtility.HtmlEncode (s, output);
		}

		public string MapPath (string path)
		{
			return context.Request.MapPath (path);
		}

		public void Transfer (string path)
		{
			// If it's a page and a postback, don't pass form data
			// See bug #65613.
			bool preserveForm = true;
			if (context.Handler is Page) {
				Page page = (Page) context.Handler;
				preserveForm = !page.IsPostBack;
			}

			Transfer (path, preserveForm);
		}

		public void Transfer (string path, bool preserveForm)
		{
			Execute (path, null, preserveForm);
			context.Response.End ();
		}
#if NET_2_0
		[MonoTODO]
		public void Transfer (IHttpHandler handler, bool preserveForm)
		{
			throw new NotImplementedException ();
		}
#endif
		public string UrlDecode (string s)
		{
			return HttpUtility.UrlDecode (s);
		}

		public void UrlDecode (string s, TextWriter output)
		{
			if (s != null)
				output.Write (HttpUtility.UrlDecode (s));
		}

		public string UrlEncode (string s)
		{
			return HttpUtility.UrlEncode (s);
		}

		public void UrlEncode (string s, TextWriter output)
		{
			if (s != null)
				output.Write (HttpUtility.UrlEncode (s));
		}

		public string UrlPathEncode (string s)
		{
			if (s == null)
				return null;

			int idx = s.IndexOf ("?");
			string s2 = null;
			if (idx != -1) {
				s2 = s.Substring (0, idx-1);
				s2 = HttpUtility.UrlEncode (s2) + s.Substring (idx);
			} else {
				s2 = HttpUtility.UrlEncode (s);
			}

			return s2;
		}

		public string MachineName {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium)]
			// Medium doesn't look heavy enough to replace this... reported as
			[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
			[EnvironmentPermission (SecurityAction.Assert, Read = "COMPUTERNAME")]
			get { return Environment.MachineName; }
		}

		public int ScriptTimeout {
			get { return (int) context.ConfigTimeout.TotalSeconds; }
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium)]
			set { context.ConfigTimeout = new TimeSpan (0, 0, value); }
		}
	}
}
