//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
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
