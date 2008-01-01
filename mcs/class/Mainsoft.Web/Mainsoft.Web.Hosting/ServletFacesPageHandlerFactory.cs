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
using System.Web.J2EE;

namespace Mainsoft.Web.Hosting
{
	public sealed class ServletFacesPageHandlerFactory : BaseFacesPageHandlerFactory
	{
		public override IHttpHandler GetHandler (HttpContext context, string requestType, string url, string path) {
			Type pageType = PageMapper.GetObjectType (context, url);
			IHttpHandler handler = new ServletFacesPageHandler (url, pageType, BaseHttpServlet.FacesContextFactory, BaseHttpServlet.Lifecycle);
			return SessionWrapper.WrapHandler (handler);
		}
	}
}

