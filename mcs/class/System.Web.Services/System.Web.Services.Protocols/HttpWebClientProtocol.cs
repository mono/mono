// 
// System.Web.Services.Protocols.HttpWebClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web.Services;
using System.Collections;

namespace System.Web.Services.Protocols {
	public abstract class HttpWebClientProtocol : WebClientProtocol {

		#region Fields

		bool allowAutoRedirect;
		X509CertificateCollection clientCertificates;
		CookieContainer cookieContainer;
		IWebProxy proxy;
		string userAgent;
		CookieCollection prevCookies;
		
#if NET_1_1
		bool _unsafeAuthenticated;
#endif
		#endregion

		#region Constructors

		protected HttpWebClientProtocol () 
		{
			allowAutoRedirect = false;
			clientCertificates = null;
			cookieContainer = null;
			proxy = null; // FIXME
			userAgent = String.Format ("Mono Web Services Client Protocol {0}", Environment.Version);
		}
		
		#endregion // Constructors

		#region Properties

		[DefaultValue (false)]
		[WebServicesDescription ("Enable automatic handling of server redirects.")]
		public bool AllowAutoRedirect {
			get { return allowAutoRedirect; }
			set { allowAutoRedirect = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebServicesDescription ("The client certificates that will be sent to the server, if the server requests them.")]
		public X509CertificateCollection ClientCertificates {
			get {
				if (clientCertificates == null)
					clientCertificates = new X509CertificateCollection ();
				return clientCertificates;
			}
		}

		[DefaultValue (null)]
		[WebServicesDescription ("A container for all cookies received from servers in the current session.")]
		public CookieContainer CookieContainer {
			get { return cookieContainer; }
			set { cookieContainer = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IWebProxy Proxy {
			get { return proxy; }
			set { proxy = value; }
		}

		[WebServicesDescription ("Sets the user agent http header for the request.")]
		public string UserAgent {
			get { return userAgent; }
			set { userAgent = value; }
		}
		
#if NET_1_1
		public bool UnsafeAuthenticatedConnectionSharing
		{
			get { return _unsafeAuthenticated; }
			set { _unsafeAuthenticated = value; }
		}
#endif

		#endregion // Properties

		#region Methods

		internal virtual void AddCookies (Uri uri)
		{
			if (cookieContainer == null)
				cookieContainer = new CookieContainer ();

			if (prevCookies == null || prevCookies.Count == 0)
				return;

			CookieCollection coll = cookieContainer.GetCookies (uri);
			foreach (Cookie prev in prevCookies) {
				bool dont = false;
				foreach (Cookie c in coll) {
					if (c.Equals (prev)) {
						dont = true;
						break;
					}
				}

				if (dont == false)
					cookieContainer.Add (prev);
			}
		}

		internal virtual void CheckForCookies (HttpWebResponse response)
		{
			CookieCollection cookies = response.Cookies;
			if (cookies.Count == 0)
				return;

			if (prevCookies == null)
				prevCookies = new CookieCollection ();

			foreach (Cookie c in cookies)
				prevCookies.Add (c);
		}
		
		protected override WebRequest GetWebRequest (Uri uri)
		{
			WebRequest req = base.GetWebRequest (uri);
			HttpWebRequest request = req as HttpWebRequest;
			if (request == null)
				return req;

			request.AllowAutoRedirect = allowAutoRedirect;
			if (clientCertificates != null)
				request.ClientCertificates.AddRange (clientCertificates);

			AddCookies (uri);
			request.CookieContainer = cookieContainer;
			if (proxy != null)
				request.Proxy = proxy;

			request.UserAgent = userAgent;

#if NET_1_1
		//	request.UnsafeAuthenticatedConnectionSharing = _unsafeAuthenticated;
#endif

			return request;
		}

		protected override WebResponse GetWebResponse (WebRequest request)
		{
			WebResponse response = base.GetWebResponse (request);
			HttpWebResponse wr = response as HttpWebResponse;
			if (wr != null)
				CheckForCookies (wr);
				
			return response;
		}

		protected override WebResponse GetWebResponse (WebRequest request, IAsyncResult result)
		{
			WebResponse response = base.GetWebResponse (request, result);
			HttpWebResponse wr = response as HttpWebResponse;
			if (wr != null)
				CheckForCookies (wr);
				
			return response;
		}
		
#if NET_2_0

		[MonoTODO]
		protected void CancelAsync (object userState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool GenerateXmlMappings (Type type, ArrayList mapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Hashtable GenerateXmlMappings (Type[] types, ArrayList mapping)
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion // Methods
	}
}
