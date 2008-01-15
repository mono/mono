using System;
using System.Collections.Generic;
using System.Text;
using javax.servlet;
using System.Web;
using javax.servlet.http;

namespace Mainsoft.Web.Hosting
{
	public class ServletIncludeUtils
	{
		public static void includeServlet (String servletPath, Object writer, Object aspPage, Object [] servletParams) {
			// Need to define logic for resolving the servletPath. Share code with portlet createRenderUrl.
			HttpContext context = HttpContext.Current;
			HttpWorkerRequest wr = (HttpWorkerRequest) ((IServiceProvider) context).GetService (typeof (HttpWorkerRequest));
			RequestDispatcher dispatcher = ((ServletContext) ((IServiceProvider) wr).GetService (typeof (ServletContext))).getRequestDispatcher (SERVLET_INCLUDE_HELPER_PATH);
			ServletResponse response = (ServletResponse) ((IServiceProvider) wr).GetService (typeof (ServletResponse));
			ServletRequest request = (ServletRequest) ((IServiceProvider) wr).GetService (typeof (ServletRequest));

			// Setup params for the include call.
			String oldServletPath = (String) setAttribute (request, SERVLET_PATH_ATTRIBUTE_NAME, servletPath);
			Object oldAspPage = setAttribute (request, ASPPAGE_ATTRIBUTE_NAME, aspPage);
			Object [] oldServletParams = (Object []) setAttribute (request, SERVLET_PARAMS_ATTRIBUTE_NAME, servletParams);
			Object oldWriter = setAttribute (request, TEXT_WRITER_ATTRIBUTE_NAME, writer);

			// Do the include call.
			dispatcher.include (request, response);

			// Restore previous attribute values after the call.
			request.setAttribute (SERVLET_PATH_ATTRIBUTE_NAME, oldServletPath);
			request.setAttribute (ASPPAGE_ATTRIBUTE_NAME, oldAspPage);
			request.setAttribute (SERVLET_PARAMS_ATTRIBUTE_NAME, oldServletParams);
			request.setAttribute (TEXT_WRITER_ATTRIBUTE_NAME, oldWriter);
		}

		public static String getServletPath (ServletRequest request) {
			return (String) request.getAttribute (SERVLET_PATH_ATTRIBUTE_NAME);
		}
		public static Object getAspPage (ServletRequest request) {
			return request.getAttribute (ASPPAGE_ATTRIBUTE_NAME);
		}
		public static Object [] getServletParams (ServletRequest request) {
			return (Object []) request.getAttribute (SERVLET_PARAMS_ATTRIBUTE_NAME);
		}
		public static Object getTextWriter (ServletRequest request) {
			return request.getAttribute (TEXT_WRITER_ATTRIBUTE_NAME);
		}

		private static Object setAttribute (ServletRequest request, String attrname, Object newval) {
			Object oldval = request.getAttribute (attrname);
			request.setAttribute (attrname, newval);
			return oldval;
		}

		public static readonly String SERVLET_INCLUDE_HELPER_PATH = "/servletincludehelper";
		public static readonly String ASPPAGE_ATTRIBUTE_NAME = "vmw.servlet.include.asppage";
		public static readonly String SERVLET_PATH_ATTRIBUTE_NAME = "vmw.servlet.include.servlet.path";
		public static readonly String SERVLET_PARAMS_ATTRIBUTE_NAME = "vmw.servlet.include.servlet.params";
		public static readonly String TEXT_WRITER_ATTRIBUTE_NAME = "vmw.servlet.include.text.writer";
	}
}
