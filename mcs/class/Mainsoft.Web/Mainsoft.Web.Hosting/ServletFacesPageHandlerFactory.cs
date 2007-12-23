using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using javax.faces.component;
using javax.servlet;
using javax.faces.context;
using javax.faces.lifecycle;
using javax.servlet.http;
using System;
using javax.faces.webapp;
using javax.faces;

namespace Mainsoft.Web.Hosting
{
	public sealed class ServletFacesPageHandlerFactory : BaseFacesPageHandlerFactory
	{
		public override IHttpHandler GetHandler (HttpContext context, string requestType, string url, string path) {
			IHttpHandler handler = new ServletFacesPageHandler (url, FacesContextFactory, Lifecycle);
			return SessionWrapper.WrapHandler (handler, context, url);
		}
	}
}

