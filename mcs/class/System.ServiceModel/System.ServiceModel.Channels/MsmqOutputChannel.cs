//
// MsmqOutputChannel.cs
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
using System.IO;
using System.Messaging;
using System.ServiceModel;
using System.Threading;

namespace System.ServiceModel.Channels
{
	internal class MsmqOutputChannel : OutputChannelBase
	{
		MsmqChannelFactory<IOutputChannel> source;
		MessageQueue queue;

		public MsmqOutputChannel (MsmqChannelFactory<IOutputChannel> factory,
			EndpointAddress address, Uri via)
			: base (factory, address, via)
		{
			this.source = factory;
		}

		// Send

		public override IAsyncResult BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			ThrowIfDisposedOrNotOpen ();

			return new MsmqChannelOutputAsyncResult (this, message, timeout, callback, state);
		}

		public override void EndSend (IAsyncResult result)
		{
			if (result == null)
				throw new ArgumentNullException ("result");
			MsmqChannelOutputAsyncResult r = result as MsmqChannelOutputAsyncResult;
			if (r == null)
				throw new InvalidOperationException ("Wrong IAsyncResult");
			r.WaitEnd ();
		}

		public override void Send (Message message, TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();

			MemoryStream ms = new MemoryStream ();
			source.MessageEncoder.WriteMessage (message, ms);

			queue.Send (ms);

			//throw new NotImplementedException ();
		}

		// Abort

		protected override void OnAbort ()
		{
			throw new NotImplementedException ();
		}

		// Close

		protected override void OnClose (TimeSpan timeout)
		{
			if (queue != null)
				queue.Close ();
			queue = null;
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		// Open

		protected override void OnOpen (TimeSpan timeout)
		{
			// FIXME: is distination really like this?
			Uri destination = Via != null ? Via : RemoteAddress.Uri;

			queue = new MessageQueue (destination.GetLeftPart (UriPartial.Scheme));
			// FIXME: setup queue
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		class MsmqChannelOutputAsyncResult : IAsyncResult
		{
			MsmqOutputChannel channel;
			Message message;
			TimeSpan timeout;
			AsyncCallback callback;
			object state;
			AutoResetEvent wait;
			bool done, waiting;
			Exception error;

			public MsmqChannelOutputAsyncResult (MsmqOutputChannel channel, Message message, TimeSpan timeout, AsyncCallback callback, object state)
			{
				this.channel = channel;
				this.message = message;
				this.timeout = timeout;
				this.callback = callback;
				this.state = state;

				wait = new AutoResetEvent (false);
				Thread t = new Thread (delegate () {
					try {
						channel.Send (message, timeout);
						if (callback != null)
							callback (this);
					} catch (Exception ex) {
						error = ex;
					} finally {
						done = true;
						wait.Set ();
					}
				});
				t.Start ();
			}

			public WaitHandle AsyncWaitHandle {
				get { return wait; }
			}

			public object AsyncState {
				get { return state; }
			}

			public bool CompletedSynchronously {
				get { return done && !waiting; }
			}

			public bool IsCompleted {
				get { return done; }
			}

			public void WaitEnd ()
			{
				if (!done) {
					waiting = true;
					wait.WaitOne (timeout, true);
				}
				if (error != null)
					throw error;
			}
		}
	}
}
