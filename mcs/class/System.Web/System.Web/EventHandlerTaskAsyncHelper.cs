//
// System.Web.EventHandlerTaskAsyncHelper.cs
//
// Author:
//   Kornel Pal (kornelpal@gmail.com)
//
// Copyright (C) 2014 Kornel Pal
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

using System.Threading.Tasks;

namespace System.Web
{
	public sealed class EventHandlerTaskAsyncHelper
	{
		readonly TaskEventHandler taskEventHandler;
		readonly BeginEventHandler beginEventHandler;
		static readonly EndEventHandler endEventHandler = TaskAsyncResult.Wait;

		public BeginEventHandler BeginEventHandler {
			get { return beginEventHandler; }
		}

		public EndEventHandler EndEventHandler {
			get { return endEventHandler; }
		}

		public EventHandlerTaskAsyncHelper (TaskEventHandler handler)
		{
			if (handler == null)
				throw new ArgumentNullException ("handler");

			taskEventHandler = handler;
			beginEventHandler = GetAsyncResult;
		}

		IAsyncResult GetAsyncResult (object sender, EventArgs e, AsyncCallback callback, object state)
		{
			Task task = taskEventHandler (sender, e);
			return TaskAsyncResult.GetAsyncResult (task, callback, state);
		}
	}
}
