//
// Mono.Messaging.RabbitMQ
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
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;

using Mono.Messaging;

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;

namespace Mono.Messaging.RabbitMQ {

	public class RabbitMQMessageQueueTransaction : IMessageQueueTransaction, IMessagingContext {
		
		private readonly string txId;
		private readonly MessagingContextPool contextPool;
		
		private MessageQueueTransactionStatus status = MessageQueueTransactionStatus.Initialized;
		private String host = null;
		private bool isDisposed = false;
		private Object syncObj = new Object ();
		private bool isActive = false;
		private MessagingContext context = null;
		
		public RabbitMQMessageQueueTransaction (string txId, MessagingContextPool contextPool)
		{
			this.txId = txId;
			this.contextPool = contextPool;
		}
		
		private IModel Model {
			get { return Context.Model; }
		}
		
		private IConnection Connection {
			get { return Context.Connection; }
		}
		
		private MessagingContext Context {
			get { 
				if (null == context) {
					context = contextPool.GetContext (host);
				}
				return context;
			}
		}
		
		public IMessage Receive (QueueReference qRef, TimeSpan timeout, IsMatch matcher, bool ack)
		{
			lock (syncObj) {
				ValidateHost (qRef);
					
				Model.TxSelect ();
				isActive = true;
				
				return Context.Receive (qRef, timeout, matcher, ack);
			}
		}	
		
		public void Send (QueueReference qRef, IMessage msg)
		{
			lock (syncObj) {
				ValidateHost (qRef);
				
				Model.TxSelect ();
				isActive = true;
				
				
				Context.Send (qRef, msg);
			}
		}
		
		private void ValidateHost (QueueReference qRef)
		{
			if (null == host) {
				host = qRef.Host;
			} else if (host != qRef.Host) {
				throw new MonoMessagingException ("Transactions can not span multiple hosts");
			}
		}
		
		public MessageQueueTransactionStatus Status {
			get { 
				lock (syncObj) 
					return status;
			}
		}
		
		public void Abort ()
		{
			lock (syncObj) {
				if (isActive)
					Context.Model.TxRollback ();
				status = MessageQueueTransactionStatus.Aborted;
			}
		}
		
		public void Begin ()
		{
			lock (syncObj) {
				if (status == MessageQueueTransactionStatus.Pending)
					throw new InvalidOperationException ("Transaction already started");
				status = MessageQueueTransactionStatus.Pending;
			}
		}
		
		public void Commit ()
		{
			lock (syncObj) {
				Context.Model.TxCommit ();
				status = MessageQueueTransactionStatus.Committed;
			}
		}
		
		public void Delete (QueueReference qRef)
		{
			lock (syncObj) {
				Context.Delete (qRef);
			}
		}
		
		public void Purge (QueueReference qRef)
		{
			lock (syncObj) {
				Context.Purge (qRef);
			}
		}
		
		public string Id {
			get { return txId; }
		}
		
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			lock (syncObj) {
				if (!isDisposed && disposing) {
					if (context != null)
						context.Dispose ();
					isDisposed = true;
				}
			}
		}
	}
}
