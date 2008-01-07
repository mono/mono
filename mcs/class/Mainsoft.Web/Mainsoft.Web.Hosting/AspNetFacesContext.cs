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
	public abstract class AspNetFacesContext : FacesContext
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
			setCurrentInstance (this);
		}

		public bool IsProcessingInclude () {
			if (_oldFacesContex == null)
				return false;

			if (getViewRoot ().getChildCount () == 0)
				throw new InvalidOperationException ();

			Page page = (Page) getViewRoot ().getChildren ().get (0);
			return !page.IsPostBack;
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
	}
}
