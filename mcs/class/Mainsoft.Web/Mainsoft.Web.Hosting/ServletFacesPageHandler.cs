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
using javax.faces.render;
using System.IO;

namespace Mainsoft.Web.Hosting
{
	public abstract class BaseFacesPageHandler
	{
		static protected void SetupResponseWriter (AspNetFacesContext facesContext, string contentType, string characterEncoding) {
			RenderKit renderKit = BaseHttpServlet.RenderKitFactory.getRenderKit (facesContext, facesContext.getViewRoot ().getRenderKitId ());
			ResponseWriter writer = renderKit.createResponseWriter (new AspNetResponseWriter (facesContext.Context.Response.Output), contentType, characterEncoding);
			facesContext.setResponseWriter (writer);
		}

		#region AspNetResponseWriter
		private sealed class AspNetResponseWriter : java.io.Writer
		{
			readonly TextWriter _writer;
			public AspNetResponseWriter (TextWriter writer) {
				_writer = writer;
			}
			public override void close () {
				_writer.Close ();
			}

			public override void flush () {
				_writer.Flush ();
			}

			public override void write (char [] __p1, int __p2, int __p3) {
				_writer.Write (__p1, __p2, __p3);
			}

			public override void write (int __p1) {
				_writer.Write ((char) __p1);
			}

			public override void write (char [] __p1) {
				_writer.Write (__p1);
			}

			public override void write (string __p1) {
				_writer.Write (__p1);
			}

			public override void write (string __p1, int __p2, int __p3) {
				_writer.Write (__p1, __p2, __p3);
			}
		}
		#endregion
	}

	public class ServletFacesPageHandler : BaseFacesPageHandler, IHttpHandler, IServiceProvider
	{
		readonly FacesContextFactory _facesContextFactory;
		readonly Lifecycle _lifecycle;
		readonly string _executionFilePath;
		readonly Type _pageType;
		Page _page;

		public bool IsReusable {
			get { return false; }
		}

		public ServletFacesPageHandler (string executionFilePath, Type pageType, FacesContextFactory facesContextFactory, Lifecycle lifecycle) {
			_facesContextFactory = facesContextFactory;
			_lifecycle = lifecycle;
			_executionFilePath = executionFilePath;
			_pageType = pageType;
		}

		protected virtual ServletFacesContext GetFacesContext (FacesContextFactory facesContextFactory,
																	ServletContext servletContext,
																	ServletRequest servletRequest,
																	ServletResponse servletResponse,
																	Lifecycle lifecycle,
																	HttpContext httpContext,
																	string executionFilePath) {
			return ServletFacesContext.GetFacesContext (facesContextFactory, servletContext, servletRequest, servletResponse, lifecycle, httpContext, executionFilePath);
		}

		public void ProcessRequest (HttpContext context) {
			ServletWorkerRequest wr = (ServletWorkerRequest) ((IServiceProvider) context).GetService (typeof (HttpWorkerRequest));
			ServletContext servletContext = wr.GetContext ();
			HttpServletRequest request = wr.ServletRequest;
			HttpServletResponse response = wr.ServletResponse;

			ServletFacesContext facesContext = GetFacesContext (_facesContextFactory, servletContext, request, response, _lifecycle, context, _executionFilePath);
			try {
				try {
					try {
						Trace.WriteLine ("FacesPageHandler: before execute");
						_lifecycle.execute (facesContext);
						Trace.WriteLine ("FacesPageHandler: after execute");
					}
					finally {
						UIViewRoot viewRoot = facesContext.getViewRoot ();
						if (viewRoot != null && viewRoot.getChildCount () > 0)
							_page = (Page) viewRoot.getChildren ().get (0);
					}
					Trace.WriteLine ("FacesPageHandler: before render");
					SetupResponseWriter (facesContext, response.getContentType (), response.getCharacterEncoding ());
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
			if (serviceType == typeof (Page))
				return _page;
			return null;
		}
	}
}
