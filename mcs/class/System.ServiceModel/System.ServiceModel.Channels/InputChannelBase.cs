//
// InputChannelBase.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;

namespace System.ServiceModel.Channels
{
	internal abstract class InputChannelBase : ChannelBase, IInputChannel
	{
		ChannelListenerBase channel_listener;

		public InputChannelBase (ChannelListenerBase listener)
			: base (listener)
		{
			this.channel_listener = listener;
		}

		public abstract EndpointAddress LocalAddress { get; }

		public IAsyncResult BeginReceive (AsyncCallback callback, object state)
		{
			return BeginReceive (DefaultReceiveTimeout, callback, state);
		}

		public abstract IAsyncResult BeginReceive (TimeSpan timeout, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginWaitForMessage (TimeSpan timeout, AsyncCallback callback, object state);

		public abstract Message EndReceive (IAsyncResult result);

		public abstract bool EndTryReceive (IAsyncResult result, out Message message);

		public abstract bool EndWaitForMessage (IAsyncResult result);

		public Message Receive ()
		{
			return Receive (DefaultReceiveTimeout);
		}

		public virtual Message Receive (TimeSpan timeout)
		{
			return EndReceive (BeginReceive (timeout, null, null));
		}

		public virtual bool TryReceive (TimeSpan timeout, out Message message)
		{
			return EndTryReceive (BeginTryReceive (timeout, null, null), out message);
		}

		public virtual bool WaitForMessage (TimeSpan timeout)
		{
			return EndWaitForMessage (BeginWaitForMessage (timeout, null, null));
		}
	}
}
