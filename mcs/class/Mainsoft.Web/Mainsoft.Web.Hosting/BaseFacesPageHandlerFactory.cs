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
		public abstract IHttpHandler GetHandler (HttpContext context, string requestType, string url, string pathTranslated);

		public virtual void ReleaseHandler (IHttpHandler handler) {
		}
	}
}
