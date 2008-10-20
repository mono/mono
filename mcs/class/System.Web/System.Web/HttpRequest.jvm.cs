//
// System.Web.HttpRequest.jvm.cs 
//
// 
// Author:
//	Eyal Alaluf <eyala@mainsoft.com>
//

//
// Copyright (C) 2006 Mainsoft, Co. (http://www.mainsoft.com)
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
using System.Web.Hosting;
using javax.servlet.http;
using System.Web.Configuration;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using Mainsoft.Web;

namespace System.Web
{
	public sealed partial class HttpRequest
	{
		const string SessionLock = "vmw.session.lock";
		const string SessionCookies = "vmw.session.cookies";

		static object GetJavaSessionLock (HttpSession javaSession)
		{
			lock (SessionLock) {
				object sessionLock = javaSession.getAttribute (SessionLock);
				if (sessionLock == null) {				
					sessionLock = String.Copy (SessionLock);
					javaSession.setAttribute (SessionLock, sessionLock);
				}
				return sessionLock;
			}
		}

		void LoadWwwForm ()
		{
			HttpServletRequest servletReq = context.ServletRequest;
			if (servletReq == null) {
				NameValueCollection requestParameters = context.RequestParameters;
				if (requestParameters != null)
					form.Add (requestParameters);
				else
					RawLoadWwwForm ();
				return;
			}

			servletReq.setCharacterEncoding (ContentEncoding.WebName);

			for (java.util.Enumeration e = servletReq.getParameterNames(); e.hasMoreElements() ;) {
				string key = (string) e.nextElement();
				string [] qvalue = QueryString.GetValues (key);
				string [] qfvalue = servletReq.getParameterValues (key);

				for (int i = (qvalue != null) ? qvalue.Length : 0; i < qfvalue.Length; i++)
					form.Add (key, qfvalue [i]);
			}
		}

		const int INPUT_BUFFER_SIZE = 1024;

		void MakeInputStream ()
		{
			if (worker_request == null)
				throw new HttpException ("No HttpWorkerRequest");

			// consider for perf:
			//    return ((ServletWorkerRequest)worker_request).InputStream();

			//
			// Use an unmanaged memory block as this might be a large
			// upload
			//
			int content_length = ContentLength;
#if NET_2_0
			HttpRuntimeSection config = (HttpRuntimeSection) WebConfigurationManager.GetSection ("system.web/httpRuntime");
#else
			HttpRuntimeConfig config = (HttpRuntimeConfig) HttpContext.GetAppConfig ("system.web/httpRuntime");
#endif
			if (content_length > (config.MaxRequestLength * 1024))
				throw new HttpException ("File exceeds httpRuntime limit");
			
			byte[] content = new byte[content_length];
			if (content == null)
				throw new HttpException (String.Format ("Not enough memory to allocate {0} bytes", content_length));

			int total;
			byte [] buffer;
			buffer = worker_request.GetPreloadedEntityBody ();
			if (buffer != null){
				total = buffer.Length;
				if (content_length > 0)
					total = Math.Min (content_length, total);
				Array.Copy (buffer, content, total);
			}
			else
				total = 0;

			buffer = new byte [INPUT_BUFFER_SIZE];
			while (total < content_length) {
				int n;
				n = worker_request.ReadEntityBody (buffer, Math.Min (content_length-total, INPUT_BUFFER_SIZE));
				if (n <= 0)
					break;
				Array.Copy (buffer, 0, content, total, n);
				total += n;
			} 
			if (total < content_length)
				throw new HttpException (411, "The uploaded file is incomplete");
							 
			input_stream = new MemoryStream (content, 0, content.Length, false, true);

			DoFilter (buffer);
		}

		internal void GetSessionCookiesForPortal (HttpCookieCollection cookies)
		{
			if (context == null)
				return;
			if (!(context.WorkerRequest is IHttpExtendedWorkerRequest))
				return;
			IHttpExtendedWorkerRequest exWorker = (IHttpExtendedWorkerRequest) context.WorkerRequest;
			HttpSession javaSession = exWorker.GetSession (false);
			if (javaSession == null)
				return;

			object sessionLock = GetJavaSessionLock (javaSession);
			lock (sessionLock) {
				Hashtable sessionCookies = (Hashtable) javaSession.getAttribute (SessionCookies);
				if (sessionCookies == null)
					return;

				ArrayList expiredCookies = null;
				foreach (string key in sessionCookies.Keys) {
					HttpCookie sessionCookie = (HttpCookie) sessionCookies [key];
					if (sessionCookie.Expires.Ticks != 0 &&
						sessionCookie.Expires.Ticks < DateTime.Now.Ticks) {
						if (cookies [key] != null)
							cookies.Remove (key);
						else {
							if (expiredCookies == null)
								expiredCookies = new ArrayList();
							expiredCookies.Add (key);
						}
					}
					else
						cookies.Set (sessionCookie);
				}

				if (expiredCookies != null)
					foreach (object key in expiredCookies)
						sessionCookies.Remove (key);
			}
		}

		internal void SetSessionCookiesForPortal (HttpCookieCollection cookies)
		{
			if (cookies == null || cookies.Count == 0)
				return;

			if (!(context.WorkerRequest is IHttpExtendedWorkerRequest))
				return;
			IHttpExtendedWorkerRequest exWorker = (IHttpExtendedWorkerRequest) context.WorkerRequest;
			bool inPortletMode = !context.IsServletRequest;
			bool shouldStoreCookiesCollection = false;
			HttpSession javaSession = exWorker.GetSession (false);

			if (javaSession == null && inPortletMode)
				javaSession = exWorker.GetSession (true);

			if (javaSession == null)
				return;

			object sessionLock = GetJavaSessionLock (javaSession);
			lock (sessionLock) {
				Hashtable sessionCookies = (Hashtable)javaSession.getAttribute (SessionCookies);			
				if (sessionCookies == null)
					if (inPortletMode) {
						sessionCookies = new Hashtable ();
						shouldStoreCookiesCollection = true;
					}
					else
						return;

				ArrayList sessionStoredCookies = null;
				for (int i=0; i < cookies.Count; i++) {
					HttpCookie cookie = cookies[i];
					if (sessionCookies [cookie.Name] != null || inPortletMode) {
						sessionCookies [cookie.Name] = cookie;
						if (sessionStoredCookies == null)
							sessionStoredCookies = new ArrayList();
						sessionStoredCookies. Add (cookie.Name);
					}
				}

				if (sessionStoredCookies != null)
					foreach (object key in sessionStoredCookies)
						cookies.Remove ((string) key);

				if (shouldStoreCookiesCollection)
					javaSession.setAttribute (SessionCookies, sessionCookies);
			}
		}

		internal void SetWorkerRequest (HttpWorkerRequest wr) {
			worker_request = wr;
			current_exe_path = null;
			file_path = null;
			base_virtual_dir = null;
			form = null;
			all_params = null;
		}

	}
}
