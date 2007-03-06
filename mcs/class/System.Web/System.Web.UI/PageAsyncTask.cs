//
// System.Web.UI.PageAsyncTask.cs
//
// Authors:
//   Adar Wesley (adarw@mainsoft.com)
//
// (C) 2007 Mainsoft, Inc (http://www.mainsoft.com)
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

#if NET_2_0

using System;

namespace System.Web.UI
{
	public sealed class PageAsyncTask
	{
		public PageAsyncTask (BeginEventHandler beginHandler, EndEventHandler endHandler,
			EndEventHandler timeoutHandler, Object state)
			: this(beginHandler, endHandler, timeoutHandler, state, false)
		{
		}

		public PageAsyncTask (BeginEventHandler beginHandler, EndEventHandler endHandler,
			EndEventHandler timeoutHandler, Object state, bool executeInParallel) 
		{
			this.beginHandler = beginHandler;
			this.endHandler = endHandler;
			this.timeoutHandler = timeoutHandler;
			this.state = state;
			this.executeInParallel = executeInParallel;
		}

		BeginEventHandler beginHandler;
		public BeginEventHandler BeginHandler {
			get { return beginHandler; } 
		}

		EndEventHandler endHandler;
		public EndEventHandler EndHandler {
			get { return endHandler; } 
		}

		EndEventHandler timeoutHandler;
		public EndEventHandler TimeoutHandler {
			get { return timeoutHandler; } 
		}

		bool executeInParallel;
		public bool ExecuteInParallel {
			get { return executeInParallel; } 
		}

		object state;
		public object State {
			get { return state; } 
		}
	}
}

#endif

