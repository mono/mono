//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
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
using System.Collections;

using Mono.Messaging;

namespace System.Messaging 
{
	public class MessageEnumerator: MarshalByRefObject, IEnumerator, IDisposable 
	{
		private IMessageEnumerator delegateEnumerator;
		private IMessageFormatter formatter;
		
		internal MessageEnumerator (IMessageEnumerator delegateEnumerator, IMessageFormatter formatter)
		{
			this.delegateEnumerator = delegateEnumerator;
			this.formatter = formatter;
		}

		public Message Current {
			get {
				IMessage iMsg = delegateEnumerator.Current;
				if (iMsg == null)
					return null;
				
				return new Message (iMsg, null, formatter);
			}
		}
		
		object IEnumerator.Current {
			get { return Current; }
		}
		
		public IntPtr CursorHandle {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		public void Close()
		{
			delegateEnumerator.Close ();
		}

		public void Dispose()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose(bool disposing)
		{
			delegateEnumerator.Dispose ();
			Close();
		}


		public bool MoveNext()
		{
			return delegateEnumerator.MoveNext ();
		}
		[MonoTODO]
		public bool MoveNext(TimeSpan timeout)
		{
			throw new NotImplementedException();
		}

		public Message RemoveCurrent()
		{
			IMessage iMsg = delegateEnumerator.RemoveCurrent ();
			if (iMsg == null)
				return null;
			return new Message (iMsg, null, formatter);
		}

		public Message RemoveCurrent (MessageQueueTransaction transaction)
		{
			
			IMessage iMsg = delegateEnumerator.RemoveCurrent (transaction.DelegateTx);
			if (iMsg == null)
				return null;
			return new Message (iMsg, null, formatter);
		}

		public Message RemoveCurrent(MessageQueueTransactionType transactionType)
		{
			IMessage iMsg = delegateEnumerator.RemoveCurrent ((Mono.Messaging.MessageQueueTransactionType) transactionType);
			if (iMsg == null)
				return null;
			return new Message (iMsg, null, formatter);
		}
		[MonoTODO]
		public Message RemoveCurrent(TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message RemoveCurrent(TimeSpan timeout, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message RemoveCurrent(TimeSpan timeout, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Reset()
		{
			Close ();
		}

		[MonoTODO]
		~MessageEnumerator()
		{
			Dispose(false);
		}
	}
}
