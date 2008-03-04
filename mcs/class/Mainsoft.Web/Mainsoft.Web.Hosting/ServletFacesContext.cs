using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.context;
using System.Web;
using System.Web.UI;
using javax.servlet;
using javax.faces.lifecycle;
using javax.faces.application;

namespace Mainsoft.Web.Hosting
{
	public class ServletFacesContext : AspNetFacesContext
	{
		protected ServletFacesContext (FacesContext wrappedFacesContext, ExternalContext externalContext, HttpContext httpContext, FacesContext oldFacesContext)
			: base (wrappedFacesContext, externalContext, httpContext, oldFacesContext) {
		}

		public static ServletFacesContext GetFacesContext (FacesContextFactory facesContextFactory,
																	ServletContext servletContext,
																	ServletRequest servletRequest,
																	ServletResponse servletResponse,
																	Lifecycle lifecycle,
																	HttpContext httpContext,
																	string executionFilePath) {
			FacesContext oldFacesContex = FacesContext.getCurrentInstance ();
			FacesContext wrappedFacesContex = facesContextFactory.getFacesContext (servletContext, servletRequest, servletResponse, lifecycle);
			ExternalContext externalContext = new ServletExternalContext (wrappedFacesContex.getExternalContext (), httpContext, executionFilePath);
			ServletFacesContext context = new ServletFacesContext (wrappedFacesContex, externalContext, httpContext, oldFacesContex);
			return context;
		}

	}
		#region ServletExternalContext

		public class ServletExternalContext : BaseExternalContext
		{
			readonly ExternalContext _externalContext;

			public ServletExternalContext (ExternalContext externalContext, HttpContext httpContext, string executionFilePath)
				: base (httpContext, executionFilePath) {
				_externalContext = externalContext;
			}

			public override void dispatch (string __p1) {
				_externalContext.dispatch (__p1);
			}

			public override string encodeActionURL (string __p1) {
				return _externalContext.encodeActionURL (__p1);
			}

			public override string encodeNamespace (string __p1) {
				return _externalContext.encodeNamespace (__p1);
			}

			public override string encodeResourceURL (string __p1) {
				return _externalContext.encodeResourceURL (__p1);
			}

			public override java.util.Map getApplicationMap () {
				return _externalContext.getApplicationMap ();
			}

			public override string getAuthType () {
				return _externalContext.getAuthType ();
			}

			public override object getContext () {
				return _externalContext.getContext ();
			}

			public override string getInitParameter (string __p1) {
				return _externalContext.getInitParameter (__p1);
			}

			public override java.util.Map getInitParameterMap () {
				return _externalContext.getInitParameterMap ();
			}

			public override string getRemoteUser () {
				return _externalContext.getRemoteUser ();
			}

			public override object getRequest () {
				return _externalContext.getRequest ();
			}

			public override string getRequestContextPath () {
				return _externalContext.getRequestContextPath ();
			}

			public override java.util.Map getRequestCookieMap () {
				return _externalContext.getRequestCookieMap ();
			}

			public override java.util.Map getRequestHeaderMap () {
				return _externalContext.getRequestHeaderMap ();
			}

			public override java.util.Map getRequestHeaderValuesMap () {
				return _externalContext.getRequestHeaderValuesMap ();
			}

			public override java.util.Locale getRequestLocale () {
				return _externalContext.getRequestLocale ();
			}

			public override java.util.Iterator getRequestLocales () {
				return _externalContext.getRequestLocales ();
			}

			public override java.util.Map getRequestMap () {
				return _externalContext.getRequestMap ();
			}

			public override string getRequestServletPath () {
				return _externalContext.getRequestServletPath ();
			}

			public override java.net.URL getResource (string __p1) {
				return _externalContext.getResource (__p1);
			}

			public override java.io.InputStream getResourceAsStream (string __p1) {
				return _externalContext.getResourceAsStream (__p1);
			}

			public override java.util.Set getResourcePaths (string __p1) {
				return _externalContext.getResourcePaths (__p1);
			}

			public override object getResponse () {
				return _externalContext.getResponse ();
			}

			public override object getSession (bool __p1) {
				return _externalContext.getSession (__p1);
			}

			public override java.util.Map getSessionMap () {
				return _externalContext.getSessionMap ();
			}

			public override java.security.Principal getUserPrincipal () {
				return _externalContext.getUserPrincipal ();
			}

			public override bool isUserInRole (string __p1) {
				return _externalContext.isUserInRole (__p1);
			}

			public override void log (string __p1, Exception __p2) {
				_externalContext.log (__p1, __p2);
			}

			public override void log (string __p1) {
				_externalContext.log (__p1);
			}

			public override void redirect (string __p1) {
				_externalContext.redirect (__p1);
			}
		}

		#endregion
}
