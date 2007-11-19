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
		readonly AspNetFacesContext _facesContext;
		readonly Lifecycle _lifecycle;

		public bool IsReusable {
			get { return false; }
		}

		public ServletFacesPageHandler (AspNetFacesContext facesContext, Lifecycle lifecycle) {
			_facesContext = facesContext;
			_lifecycle = lifecycle;
		}

		public void ProcessRequest (HttpContext context) {
			try {
				_lifecycle.execute (_facesContext);
#if DEBUG
				Console.WriteLine ("FacesPageHandler: before render");
#endif
				_lifecycle.render (_facesContext);
#if DEBUG
				Console.WriteLine ("FacesPageHandler: after render");
#endif
			}
			catch (Exception e) {
				throw;
			}
			finally {
				_facesContext.release ();
			}
		}
	}
}
