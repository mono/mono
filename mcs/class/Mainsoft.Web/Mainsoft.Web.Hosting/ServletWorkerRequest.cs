using System;
using System.Collections.Generic;
using System.Text;
using javax.servlet.http;
using java.security;
using javax.servlet;

namespace Mainsoft.Web.Hosting
{
	public sealed class ServletWorkerRequest : BaseWorkerRequest
	{
		readonly HttpServlet _HttpServlet;
		readonly HttpServletRequest _HttpServletRequest;
		readonly HttpServletResponse _HttpServletResponse;
		OutputStreamWrapper _outputStream;

		public ServletWorkerRequest (HttpServlet servlet, HttpServletRequest req, HttpServletResponse resp)
			: base (req.getContextPath(), req.getServletPath (), req.getRequestURI ()) {

			_HttpServlet = servlet;
			_HttpServletRequest = req;
			_HttpServletResponse = resp;

		}

		static readonly Type typeOfHttpServletRequest = typeof (HttpServletRequest);
		static readonly Type typeOfHttpServletResponse = typeof (HttpServletResponse);
		static readonly Type typeOfHttpServlet = typeof (HttpServlet);
		static readonly Type typeOfHttpSession = typeof (HttpSession);
		static readonly Type typeOfServletContext = typeof (ServletContext);
		static readonly Type typeOfServletConfig = typeof (ServletConfig);
		public override object GetService (Type serviceType) {
			if (serviceType == typeOfHttpServlet ||
				serviceType == typeOfServletConfig)
				return _HttpServlet;
			if (serviceType == typeOfHttpServletRequest)
				return _HttpServletRequest;
			if (serviceType == typeOfHttpServletResponse)
				return _HttpServletResponse;
			if (serviceType == typeOfHttpSession)
				return GetSession (false);
			if (serviceType == typeOfServletContext)
				return _HttpServlet.getServletContext ();
			return base.GetService (serviceType);
		}

		public HttpServlet Servlet {
			get {
				return _HttpServlet;
			}
		}

		public HttpServletRequest ServletRequest {
			get {
				return _HttpServletRequest;
			}
		}

		public HttpServletResponse ServletResponse {
			get {
				return _HttpServletResponse;
			}
		}

		public override string GetHttpVerbName () {
			return _HttpServletRequest.getMethod ();
		}

		public override string GetHttpVersion () {
			return _HttpServletRequest.getProtocol ();
		}

		public override string GetLocalAddress () {
			return _HttpServletRequest.getLocalAddr ();
		}

		public override int GetLocalPort () {
			return _HttpServletRequest.getServerPort ();
		}

		public override string GetPathInfo () {
			return base.GetPathInfo () ?? String.Empty;
		}

		public override string GetQueryString () {
			return _HttpServletRequest.getQueryString ();
		}

		public override string GetRemoteAddress () {
			return _HttpServletRequest.getRemoteAddr ();
		}

		public override string GetRemoteName () {
			return _HttpServletRequest.getRemoteHost ();
		}


		public override int GetRemotePort () {
			try {
				return _HttpServletRequest.getRemotePort ();
			}
			catch (Exception e) {
				// if servlet API is 2.3 and below - there is no method getRemotePort 
				// in ServletRequest interface... should be described as limitation.
				return 0;
			}
		}

		public override string GetProtocol () {
			return _HttpServletRequest.getScheme ();
		}

		public override string GetServerName () {
			return _HttpServletRequest.getServerName ();
		}

		public override bool IsSecure () {
			return _HttpServletRequest.isSecure ();
		}

		public override void SendStatus (int statusCode, string statusDescription) {
			// setStatus(int, string) is deprecated
			_HttpServletResponse.setStatus (statusCode/*, statusDescription*/);
		}

		public override void SendUnknownResponseHeader (string name, string value) {
			if (HeadersSent ())
				return;

			_HttpServletResponse.addHeader (name, value);
		}

		public override bool HeadersSent () {
			return _HttpServletResponse.isCommitted ();
		}

		public override void SendCalculatedContentLength (int contentLength) {
			_HttpServletResponse.setContentLength (contentLength);
		}

		public override string GetAuthType () {
			return _HttpServletRequest.getAuthType ();
		}

		protected override int getContentLength () {
			return _HttpServletRequest.getContentLength ();
		}

		protected override string getContentType () {
			return _HttpServletRequest.getContentType ();
		}

		public override string GetRemoteUser () {
			return _HttpServletRequest.getRemoteUser ();
		}

		protected override java.util.Enumeration getHeaderNames () {
			return _HttpServletRequest.getHeaderNames ();
		}

		protected override string getHeader (string name) {
			return _HttpServletRequest.getHeader (name);
		}

		protected override java.io.InputStream getInputStream () {
			return _HttpServletRequest.getInputStream ();
		}

		public override javax.servlet.ServletContext GetContext () {
			return _HttpServlet.getServletContext ();
		}

		protected override OutputStreamWrapper CreateOutputStream (bool binary) {
			return _outputStream ?? (_outputStream = binary ?
				new OutputStreamWrapper (_HttpServletResponse.getOutputStream ()) :
				new OutputStreamWrapper (_HttpServletResponse.getWriter ()));
		}

		public override HttpSession GetSession (bool create) {
			return _HttpServletRequest.getSession (create);
		}

		public override bool IsRequestedSessionIdValid () {
			return _HttpServletRequest.isRequestedSessionIdValid ();
		}
		public override string GetRequestedSessionId () {
			return _HttpServletRequest.getRequestedSessionId ();
		}

		public override bool IsUserInRole (string name) {
			return _HttpServletRequest.isUserInRole (name);
		}

		public override Principal GetUserPrincipal () {
			return _HttpServletRequest.getUserPrincipal ();
		}

		public override BaseHttpContext CreateContext (System.Web.HttpContext context) {
			return new ServletHttpContext (context);
		}
	}
}
