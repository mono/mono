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

		#endregion

		#region Constructors

		protected HttpWebClientProtocol () 
		{
			allowAutoRedirect = false;
			clientCertificates = new X509CertificateCollection ();
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
			get { return clientCertificates; }
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

		protected override WebRequest GetWebRequest (Uri uri)
		{
			if (null == uri)
				throw new InvalidOperationException ("The uri parameter is a null reference.");
			return WebRequest.Create (uri);
		}

		protected override WebResponse GetWebResponse (WebRequest request)
		{
			return request.GetResponse ();
		}

		protected override WebResponse GetWebResponse (WebRequest request, IAsyncResult result)
		{
                        IAsyncResult ar = request.BeginGetResponse (null, null);
                        ar.AsyncWaitHandle.WaitOne ();
                        return request.EndGetResponse (result);
		}

		#endregion // Methods
	}
}
