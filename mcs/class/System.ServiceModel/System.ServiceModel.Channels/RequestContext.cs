//
// RequestContext.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.ServiceModel.Channels
{
	internal abstract class InternalRequestContext : RequestContext
	{
		protected InternalRequestContext (IDefaultCommunicationTimeouts timeouts)
		{
			this.timeouts = timeouts;
		}

		IDefaultCommunicationTimeouts timeouts;

		public override IAsyncResult BeginReply (Message message, AsyncCallback callback, object state)
		{
			return BeginReply (message, timeouts.SendTimeout, callback, state);
		}

		Action<Message,TimeSpan> reply_delegate;

		public override IAsyncResult BeginReply (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			if (reply_delegate == null)
				reply_delegate = new Action<Message,TimeSpan> (Reply);
			return reply_delegate.BeginInvoke (message, timeout, callback, state);
		}

		public override void EndReply (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");
			if (reply_delegate == null)
				throw new InvalidOperationException ("Async reply operation has not started");
			reply_delegate.EndInvoke (result);
		}

		public override void Close ()
		{
			Close (timeouts.CloseTimeout);
		}

		public override void Reply (Message message)
		{
			Reply (message, timeouts.SendTimeout);
		}
	}

	public abstract class RequestContext : IDisposable
	{
		public abstract Message RequestMessage { get; }
		public abstract void Abort ();
		public abstract IAsyncResult BeginReply (Message message, AsyncCallback callback, object state);
		public abstract IAsyncResult BeginReply (Message message, TimeSpan timeout, AsyncCallback callback, object state);
		public abstract void Close ();
		public abstract void Close (TimeSpan timeout);
		public abstract void EndReply (IAsyncResult result);
		public abstract void Reply (Message message);
		public abstract void Reply (Message message, TimeSpan timeout);

		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				Close ();
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
		}
	}
}
