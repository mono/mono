using System;
using System.Collections.Generic;
using System.Text;
using javax.faces.context;
using System.Web;
using System.Web.UI;

namespace Mainsoft.Web.Hosting
{
	public class AspNetFacesContext : FacesContext
	{
		readonly FacesContext _facesContex;
		readonly HttpContext _httpContext;
		//readonly string _url;
		//readonly string _path;
		//Page _handler;

		//public Page Handler {
		//    get {
		//        if (_handler == null)
		//            _handler = (Page) PageParser.GetCompiledPageInstance (_url, _path, _httpContext);
		//        return _handler;
		//    }
		//}

		public HttpContext Context {
			get { return _httpContext; }
		}

		protected AspNetFacesContext (FacesContext facesContex, HttpContext httpContext) {
			_facesContex = facesContex;
			_httpContext = httpContext;
		}

		//protected AspNetFacesContext (FacesContext facesContex, HttpContext httpContext, Page handler) {
		//    _facesContex = facesContex;
		//    _httpContext = httpContext;
		//    _handler = handler;
		//}

		//protected AspNetFacesContext (FacesContext facesContex, HttpContext httpContext, string url, string path) {
		//    _facesContex = facesContex;
		//    _httpContext = httpContext;
		//    _url = url;
		//    _path = path;
		//}

		public static AspNetFacesContext WrapFacesContext (FacesContext facesContex, HttpContext httpContext) {
			AspNetFacesContext ctx = new AspNetFacesContext (facesContex, httpContext);
			FacesContext.setCurrentInstance (ctx);
			return ctx;
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
			return _facesContex.getExternalContext ();
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
