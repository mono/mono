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
using System.Configuration;
using System.Web;
using Mainsoft.Web.Hosting;
using javax.faces.lifecycle;

namespace Mainsoft.Web
{
	/// <summary>
	/// BaseHttpContext contains all of the per-request state information related to the processing of a single request, 
	/// and the rendering of the corresponding response.
	/// </summary>
	public abstract class BaseHttpContext
	{
		protected readonly HttpContext _context;
		static readonly object _contextKey = new object ();

		protected BaseHttpContext (HttpContext context) {
			_context = context;
			context.Items [_contextKey] = this;
		}

		/// <summary>
		/// Gets the Mainsoft.Web.BaseHttpContext object for the current request.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static BaseHttpContext GetCurrent(HttpContext context) {
			if (context == null)
				throw new ArgumentNullException ("context");

			BaseHttpContext baseContext = (BaseHttpContext) context.Items [_contextKey];
			return baseContext ?? GetWorker (context).CreateContext (context);
		}

		/// <summary>
		/// Returns the javax.faces.lifecycle.Lifecycle instance.
		/// </summary>
		public Lifecycle Lifecycle {
			get { return BaseHttpServlet.Lifecycle; }
		}

		/// <summary>
		/// Returns the Mainsoft.Web.Hosting.BaseWorkerRequest object for the current request.
		/// </summary>
		protected BaseWorkerRequest Worker {
			get { return GetWorker (_context); }
		}

		static BaseWorkerRequest GetWorker (HttpContext context) {
			return (BaseWorkerRequest) ((IServiceProvider) context).GetService (typeof (HttpWorkerRequest));
		}
	}
}
