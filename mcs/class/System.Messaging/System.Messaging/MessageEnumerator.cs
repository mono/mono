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

namespace System.Messaging 
{
	public class MessageEnumerator: MarshalByRefObject, IEnumerator, IDisposable 
	{
		[MonoTODO]
		internal MessageEnumerator (MessageQueue owner)
		{
		}

		public Message Current {		
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		object IEnumerator.Current {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public IntPtr CursorHandle {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		[MonoTODO]
		public void Close()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Close();
		}

		[MonoTODO]
		public bool MoveNext()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public bool MoveNext(TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message RemoveCurrent()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message RemoveCurrent(MessageQueueTransaction transaction)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message RemoveCurrent(MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException();
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
