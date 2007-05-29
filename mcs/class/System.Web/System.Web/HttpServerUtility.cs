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
using System.Text;

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
			Execute (path, null, true);
		}

		public void Execute (string path, TextWriter writer)
		{
			Execute (path, writer, true);
		}

#if NET_2_0
		public void Execute (string path, bool preserveForm)
		{
			Execute (path, null, preserveForm);
		}
#endif
		
#if NET_2_0
		public
#else
		internal
#endif
		void Execute (string path, TextWriter writer, bool preserveForm)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path.IndexOf (':') != -1)
				throw new ArgumentException ("Invalid path.");

			HttpRequest request = context.Request;
			string oldQuery = request.QueryStringRaw;
			int qmark = path.IndexOf ('?');
			if (qmark != -1) {
				request.QueryStringRaw = path.Substring (qmark + 1);
				path = path.Substring (0, qmark);
			} else if (!preserveForm) {
				request.QueryStringRaw = "";
			}

			HttpResponse response = context.Response;
			WebROCollection oldForm = null;
			if (!preserveForm) {
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
#if NET_2_0
				context.PushHandler (handler);
#endif
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
				if (oldQuery != null && oldQuery != "" && oldQuery != request.QueryStringRaw) {
					oldQuery = oldQuery.Substring (1); // Ignore initial '?'
					request.QueryStringRaw = oldQuery; // which is added here.
				}
				response.SetTextWriter (previous);
				if (!preserveForm)
					request.SetForm (oldForm);
#if NET_2_0
				context.PopHandler ();
#endif
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
		public void Transfer (IHttpHandler handler, bool preserveForm)
		{
			if (handler == null)
				throw new ArgumentNullException ("handler");

			// TODO: see the MS doc and search for "enableViewStateMac": this method is not
			// allowed for pages when preserveForm is true and the page IsCallback property
			// is true.

			Execute (handler, null, preserveForm);
			context.Response.End ();
		}

		public void Execute (IHttpHandler handler, TextWriter writer, bool preserveForm)
		{
			if (handler == null)
				throw new ArgumentNullException ("handler");

			HttpRequest request = context.Request;
			string oldQuery = request.QueryStringRaw;
			if (!preserveForm) {
				request.QueryStringRaw = "";
			}

			HttpResponse response = context.Response;
			WebROCollection oldForm = null;
			if (!preserveForm) {
				oldForm = request.Form as WebROCollection;
				request.SetForm (new WebROCollection ());
			}

			TextWriter output = writer;
			if (output == null)
			 	output = response.Output;

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
				if (oldQuery != null && oldQuery != "" && oldQuery != request.QueryStringRaw) {
					oldQuery = oldQuery.Substring (1); // Ignore initial '?'
					request.QueryStringRaw = oldQuery; // which is added here.
				}
				response.SetTextWriter (previous);
				if (!preserveForm)
					request.SetForm (oldForm);
			}
		}

		public static byte[] UrlTokenDecode (string input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
			if (input.Length < 1)
				return new byte[0];
			byte[] bytes = Encoding.ASCII.GetBytes (input);
			int inputLength = input.Length - 1;
			int equalsCount = (int)(((char)bytes[inputLength]) - 0x30);
			char[] ret = new char[inputLength + equalsCount];
			int i = 0;
			for (; i < inputLength; i++) {
				switch ((char)bytes[i]) {
					case '-':
						ret[i] = '+';
						break;

					case '_':
						ret[i] = '/';
						break;

					default:
						ret[i] = (char)bytes[i];
						break;
				}
			}
			while (equalsCount > 0) {
				ret[i++] = '=';
				equalsCount--;
			}
			
			return Convert.FromBase64CharArray (ret, 0, ret.Length);
		}

		public static string UrlTokenEncode (byte[] input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
			if (input.Length < 1)
				return String.Empty;
			string base64 = Convert.ToBase64String (input);
			int retlen;
			if (base64 == null || (retlen = base64.Length) == 0)
				return String.Empty;

			// MS.NET implementation seems to process the base64
			// string before returning. It replaces the chars:
			//
			//  + with -
			//  / with _
			//
			// Then removes trailing ==, which may appear in the
			// base64 string, and replaces them with a single digit
			// that's the count of removed '=' characters (0 if none
			// were removed)
			int equalsCount = 0x30;
			while (retlen > 0 && base64[retlen - 1] == '=') {
				equalsCount++;
				retlen--;
			}
			char[] chars = new char[retlen + 1];
			chars[retlen] = (char)equalsCount;
			for (int i = 0; i < retlen; i++) {
				switch (base64[i]) {
					case '+':
						chars[i] = '-';
						break;

					case '/':
						chars[i] = '_';
						break;
					
					default:
						chars[i] = base64[i];
						break;
				}
			}
			return new string (chars);
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
