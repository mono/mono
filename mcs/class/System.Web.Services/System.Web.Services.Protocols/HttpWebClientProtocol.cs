// 
// System.Web.Services.Protocols.HttpWebClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class HttpWebClientProtocol : WebClientProtocol {

		#region Fields

		bool allowAutoRedirect;
		X509CertificateCollection clientCertificates;
		CookieContainer cookieContainer;
		IWebProxy proxy;
		string userAgent;
		CookieCollection prevCookies;

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

		#endregion // Methods
	}
}
