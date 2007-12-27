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

namespace Mainsoft.Web
{
	public abstract class BaseHttpContext
	{
		protected readonly HttpContext _context;
		static readonly object _contextKey = new object ();

		protected BaseHttpContext (HttpContext context) {
			_context = context;
			context.Items [_contextKey] = this;
		}

		protected static BaseHttpContext GetBaseHttpContext(HttpContext context) {
			if (context == null)
				throw new ArgumentNullException ("context");

			BaseHttpContext baseContext = (BaseHttpContext) context.Items [_contextKey];
			return baseContext ?? GetWorker (context).CreateContext (context);
		}


		protected BaseWorkerRequest Worker {
			get { return GetWorker (_context); }
		}

		static BaseWorkerRequest GetWorker (HttpContext context) {
			return (BaseWorkerRequest) ((IServiceProvider) context).GetService (typeof (HttpWorkerRequest));
		}
	}
}
