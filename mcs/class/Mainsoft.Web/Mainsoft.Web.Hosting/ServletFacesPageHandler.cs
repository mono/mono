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
using vmw.common;
using System.Diagnostics;

namespace Mainsoft.Web.Hosting
{
	public sealed class ServletFacesPageHandler : IHttpHandler, IServiceProvider
	{
		readonly FacesContextFactory _facesContextFactory;
		readonly Lifecycle _lifecycle;
		readonly string _executionFilePath;
		readonly Type _pageType;

		public bool IsReusable {
			get { return false; }
		}

		public ServletFacesPageHandler (string executionFilePath, Type pageType, FacesContextFactory facesContextFactory, Lifecycle lifecycle) {
			_facesContextFactory = facesContextFactory;
			_lifecycle = lifecycle;
			_executionFilePath = executionFilePath;
			_pageType = pageType;
		}

		public void ProcessRequest (HttpContext context) {
			ServletWorkerRequest wr = (ServletWorkerRequest) ((IServiceProvider) context).GetService (typeof (HttpWorkerRequest));
			ServletContext servletContext = wr.GetContext ();
			HttpServletRequest request = wr.ServletRequest;
			HttpServletResponse response = wr.ServletResponse;

			FacesContext facesContext = ServletFacesContext.GetFacesContext (_facesContextFactory, servletContext, request, response, _lifecycle, context, _executionFilePath);
			try {
				try {
					Trace.WriteLine ("FacesPageHandler: before execute");
					_lifecycle.execute (facesContext);
					Trace.WriteLine ("FacesPageHandler: after execute");

					Trace.WriteLine ("FacesPageHandler: before render");
					_lifecycle.render (facesContext);
					Trace.WriteLine ("FacesPageHandler: after render");
				}
				catch (FacesException fex) {
					Exception inner = fex.InnerException;
					if (inner != null)
						TypeUtils.Throw (inner);
					throw;
				}
			}
			finally {
				facesContext.release ();
			}
		}

		public object GetService (Type serviceType) {
			if (serviceType == typeof (Type))
				return _pageType;
			return null;
		}
	}
}
