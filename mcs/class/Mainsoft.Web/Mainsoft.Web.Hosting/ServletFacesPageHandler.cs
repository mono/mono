using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using javax.faces.component;
using System.Web.UI;
using javax.faces.context;
using javax.faces.lifecycle;
using javax.faces;
using javax.servlet;
using javax.faces.webapp;
using javax.servlet.http;

namespace Mainsoft.Web.Hosting
{
	class ServletFacesPageHandler : IHttpHandler
	{
		readonly FacesContextFactory _facesContextFactory;
		readonly Lifecycle _lifecycle;

		public bool IsReusable {
			get { return false; }
		}

		public ServletFacesPageHandler (FacesContextFactory facesContextFactory, Lifecycle lifecycle) {
			_facesContextFactory = facesContextFactory;
			_lifecycle = lifecycle;
		}

		public void ProcessRequest (HttpContext context) {
			ServletWorkerRequest wr = (ServletWorkerRequest) ((IServiceProvider) context).GetService (typeof (HttpWorkerRequest));
			ServletContext servletContext = wr.GetContext ();
			HttpServletRequest request = wr.ServletRequest;
			HttpServletResponse response = wr.ServletResponse;

			FacesContext facesContext = AspNetFacesContext.GetFacesContext (_facesContextFactory, servletContext, request, response, _lifecycle, context);
			try {
				_lifecycle.execute (facesContext);
#if DEBUG
				Console.WriteLine ("FacesPageHandler: before render");
#endif
				_lifecycle.render (facesContext);
#if DEBUG
				Console.WriteLine ("FacesPageHandler: after render");
#endif
			}
			catch (Exception e) {
				throw;
			}
			finally {
				facesContext.release ();
			}
		}
	}
}
