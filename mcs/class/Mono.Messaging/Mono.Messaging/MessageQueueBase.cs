//
// Mono.Messaging
//
// Authors:
//	  Michael Barker (mike@middlesoft.co.uk)
//
// (C) 2008 Michael Barker
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
using System.Threading;
using System.Collections;

namespace Mono.Messaging
{                                              
	
	public abstract class MessageQueueBase
	{
		protected abstract IMessageQueue Queue {
			get;
		}
		
		public event CompletedEventHandler PeekCompleted;
		
		public event CompletedEventHandler ReceiveCompleted;
		
		public IAsyncResult BeginPeek ()
		{
			return new PeekAsyncResult (null, Queue, MessagingProviderLocator.InfiniteTimeout, 
			                            NullAsyncCallback);
		}

		public IAsyncResult BeginPeek (TimeSpan timeout)
		{
			return new PeekAsyncResult (null, Queue, timeout, NullAsyncCallback);
		}

		public IAsyncResult BeginPeek (TimeSpan timeout, object stateObject)
		{
			return new PeekAsyncResult (stateObject, Queue,
			                            timeout, NullAsyncCallback);
		}

		public IAsyncResult BeginPeek (TimeSpan timeout,
									  object stateObject,
									  AsyncCallback callback)
		{
			return new PeekAsyncResult (stateObject, Queue, timeout, callback);
		}
		
		public IMessage EndPeek (IAsyncResult asyncResult)
		{
			PeekAsyncResult result = (PeekAsyncResult) asyncResult;
			return result.Message;			
		}
		
		public IAsyncResult BeginReceive ()
		{
			return new ReceiveAsyncResult (null, Queue, MessagingProviderLocator.InfiniteTimeout, 
			                               NullAsyncCallback);
		}

		public IAsyncResult BeginReceive (TimeSpan timeout)
		{
			return new ReceiveAsyncResult (null, Queue, timeout, NullAsyncCallback);
		}

		public IAsyncResult BeginReceive (TimeSpan timeout, object stateObject)
		{
			return new ReceiveAsyncResult (stateObject, Queue, timeout, NullAsyncCallback);
		}

		public IAsyncResult BeginReceive (TimeSpan timeout,
		                                  object stateObject,
		                                  AsyncCallback callback)
		{
			return new ReceiveAsyncResult (stateObject, Queue, timeout, callback);
		}
		
		public IMessage EndReceive (IAsyncResult asyncResult)
		{
			ReceiveAsyncResult result = (ReceiveAsyncResult) asyncResult;
			return result.Message;			
		}
		
		public void SendReceiveCompleted (IAsyncResult result)
		{
			if (ReceiveCompleted == null)
				return;
			
			ReceiveCompleted (this, new CompletedEventArgs (result));
		}
		
		public void SendPeekCompleted (IAsyncResult result)
		{
			if (PeekCompleted == null)
				return;
			
			PeekCompleted (this, new CompletedEventArgs (result));
		}
		
		internal void NullAsyncCallback (IAsyncResult result)
		{
		}
		
		internal class ThreadWaitHandle : WaitHandle {
			
			private readonly Thread t;
			
			public ThreadWaitHandle (Thread t)
			{
				this.t = t;
			}
			
			public override bool WaitOne ()
			{
				t.Join ();
				return true;
			}
			
			public override bool WaitOne (Int32 timeout, bool exitContext)
			{
				t.Join (timeout);
				return true;
			}
			
			public override bool WaitOne (TimeSpan timeout, bool exitContext)
			{
				t.Join (timeout);
				return true;
			}
		}
		
		internal abstract class AsyncResultBase : IAsyncResult {
			
			private readonly object asyncState;
			protected readonly WaitHandle asyncWaitHandle;
			protected volatile bool isCompleted;
			protected readonly IMessageQueue q;
			private readonly Thread t;
			protected IMessage message;
			protected readonly TimeSpan timeout;
			protected readonly AsyncCallback callback;
			protected MonoMessagingException ex = null;
			
			public AsyncResultBase (object asyncState,
			                        IMessageQueue q,
			                        TimeSpan timeout,
			                        AsyncCallback callback)
			{
				this.asyncState = asyncState;
				this.asyncWaitHandle = new Mutex (false);
				this.q = q;
				this.timeout = timeout;
				this.callback = callback;
				this.t = new Thread(run);
				t.Start ();
				asyncWaitHandle = new ThreadWaitHandle(t);
			}
			
			public object AsyncState {
				get { return asyncState; }
			}
			
			public WaitHandle AsyncWaitHandle {
				get { return asyncWaitHandle; }
			}
			
			public bool CompletedSynchronously {
				get { return false; }
			}
			
			public bool IsCompleted {
				get { return isCompleted; }
			}
			
			internal IMessage Message {
				get { 
					if (ex != null)
						throw new MonoMessagingException ("Asynchronous Wrapped Exception", ex);
				
					return message;
				}
			}
			
			protected abstract IMessage GetMessage ();
			
			protected abstract void SendCompletedEvent (IAsyncResult result);
			
			private void run ()
			{
				try {
					message = GetMessage ();					
					isCompleted = true;
					callback (this);
					SendCompletedEvent (this);
				} catch (MonoMessagingException ex) {
					this.ex = ex;
				}
			}
		}
		
		internal class ReceiveAsyncResult : AsyncResultBase {
			
			public ReceiveAsyncResult (object asyncState,
			                           IMessageQueue q,
			                           TimeSpan timeout,
			                           AsyncCallback callback)
				: base (asyncState, q, timeout, callback)
			{
			}
			
			protected override IMessage GetMessage ()
			{
				if (timeout == MessagingProviderLocator.InfiniteTimeout)
					return q.Receive ();
				else
					return q.Receive (timeout);
			}
			
			protected override void SendCompletedEvent (IAsyncResult result)
			{
				q.SendReceiveCompleted (result);
			}
		}

		internal class PeekAsyncResult : AsyncResultBase {
						
			public PeekAsyncResult (object asyncState,
			                        IMessageQueue q,
			                        TimeSpan timeout,
			                        AsyncCallback callback)
				: base (asyncState, q, timeout, callback)
			{
			}
			
			protected override void SendCompletedEvent (IAsyncResult result)
			{
				Console.WriteLine ("Send Peek Completed");
				q.SendPeekCompleted (result);
			}
			
			protected override IMessage GetMessage ()
			{
				if (timeout == MessagingProviderLocator.InfiniteTimeout)
					return q.Peek ();
				else
					return q.Peek (timeout);
			}
		}
	}
}
