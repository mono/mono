// 
// System.Web.Services.Protocols.HttpWebClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Net;
using System.Security.Cryptography.X509Certificates;
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
			userAgent = "Mono Web Services Client Protocol 1.0.1234.0"; // FIXME
		}
		
		#endregion // Constructors

		#region Properties

		public bool AllowAutoRedirect {
			get { return allowAutoRedirect; }
			set { allowAutoRedirect = value; }
		}

		public X509CertificateCollection ClientCertificates {
			get { return clientCertificates; }
		}

		public CookieContainer CookieContainer {
			get { return cookieContainer; }
			set { cookieContainer = value; }
		}

		public IWebProxy Proxy {
			get { return proxy; }
			set { proxy = value; }
		}

		public string UserAgent {
			get { return userAgent; }
			set { userAgent = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override WebRequest GetWebRequest (Uri uri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override WebResponse GetWebResponse (WebRequest request)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override WebResponse GetWebResponse (WebRequest request, IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
