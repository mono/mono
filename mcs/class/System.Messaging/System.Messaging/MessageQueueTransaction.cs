//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
//
using System;

namespace System.Messaging 
{

	// TODO: have to comply with 'This type is safe for multithreaded operations'
	public class MessageQueueTransaction : IDisposable 
	{
        // To avoid multiple disposals
        private bool disposed = false;

		public MessageQueueTransaction()
		{
			status = MessageQueueTransactionStatus.Initialized;
		}
		
		MessageQueueTransactionStatus status;
		
		public MessageQueueTransactionStatus Status 
		{
			get { return status; }
		}
			
		[MonoTODO]
		public void Abort()
		{
			if (status != MessageQueueTransactionStatus.Pending)
				throw new InvalidOperationException();
			status = MessageQueueTransactionStatus.Aborted;
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void Begin()
		{
			if (status != MessageQueueTransactionStatus.Initialized)
				throw new InvalidOperationException();
			status = MessageQueueTransactionStatus.Pending;
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void Commit()
		{
			if (status != MessageQueueTransactionStatus.Pending)
				throw new InvalidOperationException();
			status = MessageQueueTransactionStatus.Committed;
			throw new NotImplementedException();
		}

		public virtual void Dispose()
		{
			if (status == MessageQueueTransactionStatus.Pending)
				Abort();
            // Do this only at the first time
            if (!this.disposed)
				Dispose(true);
            disposed = true;         
            // Take this object off the finalization queue 
            GC.SuppressFinalize(this);
		}
		
		[MonoTODO]
		protected virtual void Dispose(bool disposing)
		{
			//if (disposing)
			//  free managed resources, by calling dispose on them

			// free external resources
			throw new NotImplementedException();
		}
		
		~MessageQueueTransaction()
		{
 		 	if (status == MessageQueueTransactionStatus.Pending)
				Abort();
           	if (!this.disposed)
				Dispose(false);
		}
	}
}
