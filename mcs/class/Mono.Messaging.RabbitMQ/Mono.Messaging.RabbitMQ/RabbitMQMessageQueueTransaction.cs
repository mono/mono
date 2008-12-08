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

	public class RabbitMQMessageQueueTransaction : IMessageQueueTransaction {
		
		private readonly string txId;
		private MessageQueueTransactionStatus status = MessageQueueTransactionStatus.Initialized;
		private IConnection cn = null;
		private IModel model = null;
		private String host = null;
		private bool isDisposed = false;
		private Object syncObj = new Object ();
		
		public RabbitMQMessageQueueTransaction (string txId)
		{
			this.txId = txId;
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
				if (model != null)
					model.TxRollback ();
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
				model.TxCommit ();
				status = MessageQueueTransactionStatus.Committed;
			}
		}
		
		public string Id {
			get { return txId; }
		}
		
		public delegate void Send (ref string host, ref IConnection cn, 
		                           ref IModel model, IMessage msg, string txId);
		
		public delegate IMessage Receive (ref string host, ref IConnection cn, 
		                                  ref IModel model, string txId);
		
		public void RunSend (Send sendDelegate, IMessage msg)
		{
			lock (syncObj) {
				sendDelegate (ref host, ref cn, ref model, msg, Id);
			}
		}
		
		public IMessage RunReceive (Receive receiveDelegate)
		{
			lock (syncObj) {
				return receiveDelegate (ref host, ref cn, ref model, Id);
			}
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
					if (model != null)
						model.Dispose ();
					if (cn != null)
						cn.Dispose ();
					isDisposed = true;
				}
			}
		}

	}
}
