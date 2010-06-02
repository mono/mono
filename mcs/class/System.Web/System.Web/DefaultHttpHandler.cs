//
// System.Web.DefaultHttpHandler
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
//
// Copyright (C) 2006-2009 Novell, Inc (http://www.novell.com)
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

using System.Collections.Specialized;
using System.Threading;

namespace System.Web
{
	public class DefaultHttpHandler : IHttpAsyncHandler
	{
		sealed class DefaultHandlerAsyncResult : IAsyncResult
		{
			public object AsyncState {
				get;
				private set;
			}

			public WaitHandle AsyncWaitHandle {
				get { return null; }
			}
			
			public bool CompletedSynchronously {
				get { return true; }
			}

			public bool IsCompleted {
				get { return true; }
			}
			
			public DefaultHandlerAsyncResult (AsyncCallback callback, object state)
			{
				this.AsyncState = state;

				if (callback != null)
					callback (this);
			}
		}

		NameValueCollection executeUrlHeaders;
		
		protected HttpContext Context {
			get;
			private set;
		}

		public virtual bool IsReusable {
			get { return false; }
		}
		
		protected NameValueCollection ExecuteUrlHeaders {
			get {
				HttpContext context = Context;
				HttpRequest req = context != null ? context.Request : null;
				if (req != null && executeUrlHeaders != null)	
					executeUrlHeaders = new NameValueCollection (req.Headers);
				
				return executeUrlHeaders;
			}
		}

		public virtual IAsyncResult BeginProcessRequest (HttpContext context, AsyncCallback callback, object state)
		{
			this.Context = context;

			HttpRequest req = context != null ? context.Request : null;
			string filePath = req != null ? req.FilePath : null;

			if (!String.IsNullOrEmpty (filePath) && String.Compare (".asp", VirtualPathUtility.GetExtension (filePath), StringComparison.OrdinalIgnoreCase) == 0)
				throw new HttpException (String.Format ("Access to file '{0}' is forbidden.", filePath));

			if (req != null && String.Compare ("POST", req.HttpMethod, StringComparison.OrdinalIgnoreCase) == 0)
				throw new HttpException (String.Format ("Method '{0}' is not allowed when accessing file '{1}'", req.HttpMethod, filePath));

			var sfh = new StaticFileHandler ();
			sfh.ProcessRequest (context);
			
			return new DefaultHandlerAsyncResult (callback, state);
		}

		public virtual void EndProcessRequest (IAsyncResult result)
		{
			// nothing to do
		}
		
		public virtual void ProcessRequest (HttpContext context)
		{
			throw new InvalidOperationException ("The ProcessRequest cannot be called synchronously.");
		}

		public virtual void OnExecuteUrlPreconditionFailure ()
		{
			// nothing to do
		}

		public virtual string OverrideExecuteUrlPath ()
		{
			return null;
		}
	}
}

