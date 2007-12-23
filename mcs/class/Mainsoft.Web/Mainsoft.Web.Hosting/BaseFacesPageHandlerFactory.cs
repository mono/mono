using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using javax.faces.context;
using javax.faces.lifecycle;
using javax.servlet;
using javax.faces;
using javax.faces.webapp;

namespace Mainsoft.Web
{
	public abstract class BaseFacesPageHandlerFactory : IHttpHandlerFactory
	{
		readonly FacesContextFactory _facesContextFactory;
		readonly Lifecycle _lifecycle;

		protected FacesContextFactory FacesContextFactory {
			get { return _facesContextFactory; }
		}

		protected Lifecycle Lifecycle {
			get { return _lifecycle; }
		}

		public BaseFacesPageHandlerFactory () {

			_facesContextFactory = (FacesContextFactory) FactoryFinder.getFactory (FactoryFinder.FACES_CONTEXT_FACTORY);

			HttpWorkerRequest wr = (HttpWorkerRequest) ((IServiceProvider) HttpContext.Current).GetService (typeof (HttpWorkerRequest));
			ServletContext servletContext = (ServletContext) ((IServiceProvider) wr).GetService (typeof (ServletContext));

			String lifecycleId = servletContext.getInitParameter (FacesServlet.LIFECYCLE_ID_ATTR);
			lifecycleId = lifecycleId ?? LifecycleFactory.DEFAULT_LIFECYCLE;
			//TODO: null-check for Weblogic, that tries to initialize Servlet before ContextListener

			//Javadoc says: Lifecycle instance is shared across multiple simultaneous requests, it must be implemented in a thread-safe manner.
			//So we can acquire it here once:
			LifecycleFactory lifecycleFactory = (LifecycleFactory) FactoryFinder.getFactory (FactoryFinder.LIFECYCLE_FACTORY);
			_lifecycle = lifecycleFactory.getLifecycle (lifecycleId);

		}

		public abstract IHttpHandler GetHandler (HttpContext context, string requestType, string url, string pathTranslated);

		public virtual void ReleaseHandler (IHttpHandler handler) {
		}
	}
}
