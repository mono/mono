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
	public sealed class ServletFacesPageHandlerFactory : IHttpHandlerFactory
	{
		readonly ServletConfig _servletConfig;
		readonly FacesContextFactory _facesContextFactory;
		readonly Lifecycle _lifecycle;

		string getLifecycleId () {
			String lifecycleId = _servletConfig.getServletContext ().getInitParameter (FacesServlet.LIFECYCLE_ID_ATTR);
			return lifecycleId != null ? lifecycleId : LifecycleFactory.DEFAULT_LIFECYCLE;
		}

		public ServletFacesPageHandlerFactory () {

			HttpWorkerRequest wr = (HttpWorkerRequest) ((IServiceProvider) HttpContext.Current).GetService (typeof (HttpWorkerRequest));
			HttpServlet servlet = (HttpServlet) ((IServiceProvider) wr).GetService (typeof (HttpServlet));

			_servletConfig = servlet.getServletConfig ();
			_facesContextFactory = (FacesContextFactory) FactoryFinder.getFactory (FactoryFinder.FACES_CONTEXT_FACTORY);
			//TODO: null-check for Weblogic, that tries to initialize Servlet before ContextListener

			//Javadoc says: Lifecycle instance is shared across multiple simultaneous requests, it must be implemented in a thread-safe manner.
			//So we can acquire it here once:
			LifecycleFactory lifecycleFactory = (LifecycleFactory) FactoryFinder.getFactory (FactoryFinder.LIFECYCLE_FACTORY);
			_lifecycle = lifecycleFactory.getLifecycle (getLifecycleId ());

		}

		public IHttpHandler GetHandler (HttpContext context, string requestType, string url, string path) {
			IHttpHandler handler = new ServletFacesPageHandler (url, _facesContextFactory, _lifecycle);
			return SessionWrapper.WrapHandler (handler, context, url);
		}

		public void ReleaseHandler (IHttpHandler handler) {
		}
	}
}

