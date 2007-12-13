using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.context;
using System.Web;
using System.Web.UI;
using javax.servlet;
using javax.faces.lifecycle;

namespace Mainsoft.Web.Hosting
{
	public class AspNetFacesContext : FacesContext
	{
		readonly FacesContext _oldFacesContex;
		readonly FacesContext _facesContex;
		readonly HttpContext _httpContext;
		readonly ExternalContext _externalContext;

		public HttpContext Context {
			get { return _httpContext; }
		}

		protected AspNetFacesContext (FacesContext wrappedFacesContex, ExternalContext externalContext, HttpContext httpContext, FacesContext oldFacesContex) {
			_facesContex = wrappedFacesContex;
			_httpContext = httpContext;
			_oldFacesContex = oldFacesContex;
			_externalContext = externalContext;
		}

		public static AspNetFacesContext GetFacesContext (FacesContextFactory facesContextFactory,
															ServletContext servletContext,
															ServletRequest servletRequest,
															ServletResponse servletResponse,
															Lifecycle lifecycle,
															HttpContext httpContext,
															string executionFilePath) {
			FacesContext oldFacesContex = FacesContext.getCurrentInstance ();
			FacesContext wrappedFacesContex = facesContextFactory.getFacesContext (servletContext, servletRequest, servletResponse, lifecycle);
			ExternalContext externalContext = new AspNetExternalContext (wrappedFacesContex.getExternalContext (), executionFilePath);
			AspNetFacesContext context = new AspNetFacesContext (wrappedFacesContex, externalContext, httpContext, oldFacesContex);
			FacesContext.setCurrentInstance (context);
			return context;
		}

		public override void addMessage (string __p1, javax.faces.application.FacesMessage __p2) {
			_facesContex.addMessage (__p1, __p2);
		}

		public override javax.faces.application.Application getApplication () {
			return _facesContex.getApplication ();
		}

		public override java.util.Iterator getClientIdsWithMessages () {
			return _facesContex.getClientIdsWithMessages ();
		}

		public override ExternalContext getExternalContext () {
			return _externalContext;
		}

		public override javax.faces.application.FacesMessage.Severity getMaximumSeverity () {
			return _facesContex.getMaximumSeverity ();
		}

		public override java.util.Iterator getMessages (string __p1) {
			return _facesContex.getMessages (__p1);
		}

		public override java.util.Iterator getMessages () {
			return _facesContex.getMessages ();
		}

		public override javax.faces.render.RenderKit getRenderKit () {
			return _facesContex.getRenderKit ();
		}

		public override bool getRenderResponse () {
			return _facesContex.getRenderResponse ();
		}

		public override bool getResponseComplete () {
			return _facesContex.getResponseComplete ();
		}

		public override ResponseStream getResponseStream () {
			return _facesContex.getResponseStream ();
		}

		public override ResponseWriter getResponseWriter () {
			return _facesContex.getResponseWriter ();
		}

		public override javax.faces.component.UIViewRoot getViewRoot () {
			return _facesContex.getViewRoot ();
		}

		public override void release () {
			_facesContex.release ();
			FacesContext.setCurrentInstance (_oldFacesContex);
		}

		public override void renderResponse () {
			_facesContex.renderResponse ();
		}

		public override void responseComplete () {
			_facesContex.responseComplete ();
		}

		public override void setResponseStream (ResponseStream __p1) {
			_facesContex.setResponseStream (__p1);
		}

		public override void setResponseWriter (ResponseWriter __p1) {
			_facesContex.setResponseWriter (__p1);
		}

		public override void setViewRoot (javax.faces.component.UIViewRoot __p1) {
			_facesContex.setViewRoot (__p1);
		}

		sealed class AspNetExternalContext : ExternalContext
		{
			ExternalContext _externalContext;
			string _executionFilePath;

			public AspNetExternalContext (ExternalContext externalContext, string executionFilePath) {
				_externalContext = externalContext;
				_executionFilePath = executionFilePath;
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

			public override java.util.Map getRequestParameterMap () {
				return _externalContext.getRequestParameterMap ();
			}

			public override java.util.Iterator getRequestParameterNames () {
				return _externalContext.getRequestParameterNames ();
			}

			public override java.util.Map getRequestParameterValuesMap () {
				return _externalContext.getRequestParameterValuesMap ();
			}

			public override string getRequestPathInfo () {
				return _executionFilePath.Substring (getRequestContextPath ().Length);
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
	}
}
