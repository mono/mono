//
// Mono.Messaging
//
// Authors:
//		Michael Barker (mike@middlesoft.co.uk)
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
using System.ComponentModel;

namespace Mono.Messaging {

	public interface IMessageQueue {
		
		bool Authenticate {
			get; set;
		}

		short BasePriority {
			get; set;
		}

		bool CanRead {
			get;
		}

		bool CanWrite {
			get;
		}

		Guid Category {
			get; set;
		}

		DateTime CreateTime {
			get;
		}

		bool DenySharedReceive {
			get; set;
		}

		EncryptionRequired EncryptionRequired {
			get; set;
		}

		Guid Id {
			get;
		}

		DateTime LastModifyTime {
			get;
		}

		long MaximumJournalSize {
			get; set;
		}

		long MaximumQueueSize {
			get; set;
		}

		IntPtr ReadHandle {
			get;
		}

		ISynchronizeInvoke SynchronizingObject {
			get; set;
		}

		bool Transactional {
			get;
		}

		bool UseJournalQueue {
			get; set;
		}

		IntPtr WriteHandle {
			get;
		}
		
		QueueReference QRef {
			get; set;
		}
		
		void Close ();
		
		void Purge ();
		
		void Send (IMessage message);
		
		void Send (IMessage message, IMessageQueueTransaction transaction);
		
		void Send (IMessage message, MessageQueueTransactionType transactionType);
		
		IMessage Peek ();
		
		IMessage Peek (TimeSpan timeout);
		
		IMessage PeekById (string id);
		
		IMessage PeekById (string id, TimeSpan timeout);
		
		IMessage PeekByCorrelationId (string correlationId);
		
		IMessage PeekByCorrelationId (string correlationId, TimeSpan timeout);
		
		IMessage Receive ();
		
		IMessage Receive (TimeSpan timeout);
		
		IMessage Receive (IMessageQueueTransaction transaction);
		
		IMessage Receive (TimeSpan timeout, IMessageQueueTransaction transaction);
		
		IMessage Receive (MessageQueueTransactionType transactionType);
		
		IMessage Receive (TimeSpan timeout, MessageQueueTransactionType transactionType);
		
		IMessage ReceiveById (string id);
		
		IMessage ReceiveById (string id, TimeSpan timeout);
		
		IMessage ReceiveById (string id, IMessageQueueTransaction transaction);
		
		IMessage ReceiveById (string id, MessageQueueTransactionType transactionType);
		
		IMessage ReceiveById (string id, TimeSpan timeout, IMessageQueueTransaction transaction);
		
		IMessage ReceiveById (string id, TimeSpan timeout, MessageQueueTransactionType transactionType);
			
		IMessage ReceiveByCorrelationId (string correlationId);
		
		IMessage ReceiveByCorrelationId (string correlationId, TimeSpan timeout);
		
		IMessage ReceiveByCorrelationId (string correlationId, IMessageQueueTransaction transaction);

		IMessage ReceiveByCorrelationId (string correlationId, MessageQueueTransactionType transactionType);
		
		IMessage ReceiveByCorrelationId (string correlationId, TimeSpan timeout, IMessageQueueTransaction transaction);
			
		IMessage ReceiveByCorrelationId (string correlationId, TimeSpan timeout, MessageQueueTransactionType transactionType);
		
		IAsyncResult BeginPeek ();

		IAsyncResult BeginPeek (TimeSpan timeout);

		IAsyncResult BeginPeek (TimeSpan timeout, object stateObject);

		IAsyncResult BeginPeek (TimeSpan timeout, object stateObject, AsyncCallback callback);
		
		IMessage EndPeek (IAsyncResult asyncResult);
		
		IAsyncResult BeginReceive ();

		IAsyncResult BeginReceive (TimeSpan timeout);

		IAsyncResult BeginReceive (TimeSpan timeout, object stateObject);

		IAsyncResult BeginReceive (TimeSpan timeout, object stateObject, AsyncCallback callback);
		
		IMessage EndReceive (IAsyncResult asyncResult);
		
		IMessageEnumerator GetMessageEnumerator ();

		event CompletedEventHandler PeekCompleted;
		
		event CompletedEventHandler ReceiveCompleted;
		
		void SendReceiveCompleted (IAsyncResult result);
		
		void SendPeekCompleted (IAsyncResult result);
	}

}
