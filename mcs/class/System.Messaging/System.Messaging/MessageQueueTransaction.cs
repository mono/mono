//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
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
