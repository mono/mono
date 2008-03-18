//
// (C) 2007 Mainsoft Corporation (http://www.mainsoft.com)
// Author: Konstantin Triger <kostat@mainsoft.com>
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Mainsoft.Web.Hosting;
using javax.servlet;
using javax.servlet.http;

namespace Mainsoft.Web
{
	/// <summary>
	/// ServletHttpContext contains all of the per-request state information related to the processing of a single request, 
	/// and the rendering of the corresponding response.
	/// </summary>
	public sealed class ServletHttpContext : BaseHttpContext
	{
		internal ServletHttpContext (HttpContext context)
			: base (context) {
		}

		/// <summary>
		/// Gets the Mainsoft.Web.ServletHttpContext object for the current sevlet request.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static new ServletHttpContext GetCurrent (HttpContext context) {
			return BaseHttpContext.GetCurrent (context) as ServletHttpContext;
		}

		private new ServletWorkerRequest Worker {
			get { return (ServletWorkerRequest) base.Worker; }
		}

		/// <summary>
		/// Returns the current javax.servlet.http.HttpServlet object.
		/// </summary>
		public HttpServlet Servlet {
			get { return Worker.Servlet; }
		}

		/// <summary>
		/// Returns the current javax.servlet.http.HttpServletRequest object.
		/// </summary>
		public HttpServletRequest ServletRequest {
			get { return Worker.ServletRequest; }
		}


		/// <summary>
		/// Returns the current javax.servlet.http.HttpServletResponse object.
		/// </summary>
		public HttpServletResponse ServletResponse {
			get { return Worker.ServletResponse; }
		}

		/// <summary>
		/// Returns the javax.servlet.ServletConfig object for the current sevlet.
		/// </summary>
		public ServletConfig ServletConfig {
			get { return Servlet.getServletConfig (); }
		}

		/// <summary>
		/// Returns the javax.servlet.ServletContext object for the current application.
		/// </summary>
		public ServletContext ServletContext {
			get { return Servlet.getServletContext (); }
		}


		/// <summary>
		/// Returns the current servlet name.
		/// </summary>
		public string ServletName {
			get {
				return Servlet.getServletName ();
			}
		}
	}
}
