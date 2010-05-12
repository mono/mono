//
// System.Web.HttpRequest.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//

//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.Threading;
using System.Web.Configuration;
using System.Web.SessionState;

namespace System.Web
{
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

		public void Execute (string path, bool preserveForm)
		{
			Execute (path, null, preserveForm);
		}

		public void Execute (string path, TextWriter writer, bool preserveForm)
		{			
			Execute (path, writer, preserveForm, false);
		}

		void Execute (string path, TextWriter writer, bool preserveForm, bool isTransfer)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path.IndexOf (':') != -1)
				throw new ArgumentException ("Invalid path.");

			string queryString = null;
			int qmark = path.IndexOf ('?');
			if (qmark != -1) {
				queryString = path.Substring (qmark + 1);
				path = path.Substring (0, qmark);
			}

			string exePath = UrlUtils.Combine (context.Request.BaseVirtualDir, path);
			bool cookieless = false;
			SessionStateSection config = WebConfigurationManager.GetWebApplicationSection ("system.web/sessionState") as SessionStateSection;
			cookieless = SessionStateModule.IsCookieLess (context, config);
			
			if (cookieless)
				exePath = UrlUtils.RemoveSessionId (VirtualPathUtility.GetDirectory (exePath), exePath);
			
			IHttpHandler handler = context.ApplicationInstance.GetHandler (context, exePath, true);
			Execute (handler, writer, preserveForm, exePath, queryString, isTransfer, true);
		}

		internal void Execute (IHttpHandler handler, TextWriter writer, bool preserveForm, string exePath, string queryString, bool isTransfer, bool isInclude)
		{
#if !TARGET_J2EE
			// If the target handler is not Page, the transfer must not occur.
			// InTransit == true means we're being called from Transfer
			bool is_static = (handler is StaticFileHandler);
			if (isTransfer && !(handler is Page) && !is_static)
				throw new HttpException ("Transfer is only allowed to .aspx and static files");
#endif

			HttpRequest request = context.Request;
			string oldQuery = request.QueryStringRaw;
			if (queryString != null) {
				request.QueryStringRaw = queryString;
			} else if (!preserveForm) {
				request.QueryStringRaw = String.Empty;
			}

			HttpResponse response = context.Response;
			WebROCollection oldForm = request.Form as WebROCollection;
			if (!preserveForm) {
				request.SetForm (new WebROCollection ());
			}

			TextWriter output = writer;
			if (output == null)
			 	output = response.Output;
			
			TextWriter previous = response.SetTextWriter (output);
			string oldExePath = request.CurrentExecutionFilePath;
			bool oldIsInclude = context.IsProcessingInclude;
			try {
				context.PushHandler (handler);
				if (is_static) // Not sure if this should apply to Page too
					request.SetFilePath (exePath);

				request.SetCurrentExePath (exePath);
				context.IsProcessingInclude = isInclude;
				
				if (!(handler is IHttpAsyncHandler)) {
					handler.ProcessRequest (context);
				} else {
					IHttpAsyncHandler asyncHandler = (IHttpAsyncHandler) handler;
					IAsyncResult ar = asyncHandler.BeginProcessRequest (context, null, null);
					WaitHandle asyncWaitHandle = ar != null ? ar.AsyncWaitHandle : null;
					if (asyncWaitHandle != null)
						asyncWaitHandle.WaitOne ();
					asyncHandler.EndProcessRequest (ar);
				}
			} finally {
				if (oldQuery != request.QueryStringRaw) {
					if (oldQuery != null && oldQuery.Length > 0) {
						oldQuery = oldQuery.Substring (1); // Ignore initial '?'
						request.QueryStringRaw = oldQuery; // which is added here.
					} else
						request.QueryStringRaw = String.Empty;
				}
				
				response.SetTextWriter (previous);
				if (!preserveForm)
					request.SetForm (oldForm);

				context.PopHandler ();
				request.SetCurrentExePath (oldExePath);
				context.IsProcessingInclude = oldIsInclude;
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

		
		public void TransferRequest (string path)
		{
			TransferRequest (path, false, null, null);
		}
		
		public void TransferRequest (string path, bool preserveForm)
		{
			TransferRequest (path, preserveForm, null, null);
		}

		[MonoTODO ("Always throws PlatformNotSupportedException.")]
		public void TransferRequest (string path, bool preserveForm, string method, NameValueCollection headers)
		{
			throw new PlatformNotSupportedException ();
		}
		
		public void Transfer (string path)
		{
			Transfer (path, true);
		}

		public void Transfer (string path, bool preserveForm) {
			Execute (path, null, preserveForm, true);
			context.Response.End ();
		}

		public void Transfer (IHttpHandler handler, bool preserveForm)
		{
			if (handler == null)
				throw new ArgumentNullException ("handler");

			// TODO: see the MS doc and search for "enableViewStateMac": this method is not
			// allowed for pages when preserveForm is true and the page IsCallback property
			// is true.
			Execute (handler, null, preserveForm, context.Request.CurrentExecutionFilePath, null, true, true);
			context.Response.End ();
		}

		public void Execute (IHttpHandler handler, TextWriter writer, bool preserveForm)
		{
			if (handler == null)
				throw new ArgumentNullException ("handler");

			Execute (handler, writer, preserveForm, context.Request.CurrentExecutionFilePath, null, false, true);
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

		public string UrlDecode (string s)
		{
			HttpRequest request = context.Request;
			if(request != null)
				return HttpUtility.UrlDecode (s, request.ContentEncoding);
			else
				return HttpUtility.UrlDecode (s);
		}

		public void UrlDecode (string s, TextWriter output)
		{
			if (s != null)
				output.Write (UrlDecode (s));
		}

		public string UrlEncode (string s)
		{
			HttpResponse response = context.Response;
			if (response != null)
				return HttpUtility.UrlEncode (s, response.ContentEncoding);
			else
				return HttpUtility.UrlEncode (s);
		}

		public void UrlEncode (string s, TextWriter output)
		{
			if (s != null)
				output.Write (UrlEncode (s));
		}

		public string UrlPathEncode (string s)
		{
			if (s == null)
				return null;

			int idx = s.IndexOf ('?');
			string s2 = null;
			if (idx != -1) {
				s2 = s.Substring (0, idx);
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
			set { context.ConfigTimeout = TimeSpan.FromSeconds (value); }
		}
	}
}
