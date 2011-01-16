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

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;

namespace Mono.Messaging.RabbitMQ {

	/// <summary>
	/// RabbitMQ Implementation of a message queue.  Currrently this implementation
	/// attempts to be as stateless as possible.  Connection the AMQP server
	/// are only created as needed.
	/// </summary>
	public class RabbitMQMessageQueue : MessageQueueBase, IMessageQueue {
		
		private readonly RabbitMQMessagingProvider provider;
		private readonly MessageFactory helper;
		private readonly bool transactional;
		private readonly TimeSpan noTime = new TimeSpan(0, 0, 0, 0, 500);
		
		private bool authenticate = false;
		private short basePriority = 0;
		private Guid category = Guid.Empty;
		private bool denySharedReceive = false;
		private EncryptionRequired encryptionRequired;
		private long maximumJournalSize = -1;
		private long maximumQueueSize = -1;
		private ISynchronizeInvoke synchronizingObject = null;
		private bool useJournalQueue = false;
		private QueueReference qRef = QueueReference.DEFAULT;
		
		public RabbitMQMessageQueue (RabbitMQMessagingProvider provider,
		                             bool transactional)
			: this (provider, QueueReference.DEFAULT, transactional)
		{
		}
		
		public RabbitMQMessageQueue (RabbitMQMessagingProvider provider,
		                             QueueReference qRef, 
		                             bool transactional)
		{
			this.provider = provider;
			this.helper = new MessageFactory (provider);
			this.qRef = qRef;
			this.transactional = transactional;
		}
		
		protected override IMessageQueue Queue {
			get { return this; }
		}

		public bool Authenticate {
			get { return authenticate; }
			set { authenticate = value; }
		}

		public short BasePriority {
			get { return basePriority; }
			set { basePriority = value; }
		}

		public bool CanRead {
			get { throw new NotImplementedException (); }
		}
		
		public bool CanWrite {
			get { throw new NotImplementedException (); }
		}
		
		public Guid Category {
			get { return category; }
			set { category = value; }
		}
		
		public DateTime CreateTime {
			get { throw new NotImplementedException (); }
		}
		
		public bool DenySharedReceive {
			get { return denySharedReceive; }
			set { denySharedReceive = value; }
		}
		
		public EncryptionRequired EncryptionRequired {
			get { return encryptionRequired; }
			set { encryptionRequired = value; }
		}
		
		public Guid Id {
			get { throw new NotImplementedException (); }
		}
		
		public DateTime LastModifyTime {
			get { throw new NotImplementedException (); }
		}
		
		public long MaximumJournalSize {
			get { return maximumJournalSize; }
			set { maximumJournalSize = value; }
		}
		
		public long MaximumQueueSize {
			get { return maximumQueueSize; }
			set { maximumQueueSize = value; }
		}
		
		public IntPtr ReadHandle {
			get { throw new NotImplementedException (); }
		}
		
		public ISynchronizeInvoke SynchronizingObject {
			get { return synchronizingObject; }
			set { synchronizingObject = value; }
		}
		
		public bool Transactional {
			get { return transactional; }
		}
		
		public bool UseJournalQueue {
			get { return useJournalQueue; }
			set { useJournalQueue = value; }
		}
		
		public IntPtr WriteHandle {
			get { throw new NotImplementedException (); }
		}
		
		public QueueReference QRef {
			get { return qRef; }
			set { qRef = value; }
		}
		
		private void SetDeliveryInfo (IMessage msg, string transactionId)
		{
			msg.SetDeliveryInfo (Acknowledgment.None,
			                     DateTime.MinValue,
			                     this,
			                     Guid.NewGuid ().ToString () + "\\0",
			                     MessageType.Normal,
			                     new byte[0],
			                     0,
			                     DateTime.UtcNow,
			                     null,
			                     transactionId);
		}
		
		public void Close ()
		{
		}
		
		public static void Delete (QueueReference qRef)
		{
			RabbitMQMessagingProvider provider = (RabbitMQMessagingProvider) MessagingProviderLocator.GetProvider ();
			using (IMessagingContext context = provider.CreateContext (qRef.Host)) {
				context.Delete (qRef);
			}
		}			
		
		public void Send (IMessage msg)
		{
			if (QRef == QueueReference.DEFAULT)
				throw new MonoMessagingException ("Path has not been specified");
			
			if (msg.BodyStream == null)
				throw new ArgumentException ("BodyStream is null, Message is not serialized properly");
			
			using (IMessagingContext context = CurrentContext) {
				try {
					SetDeliveryInfo (msg, null);
					context.Send (QRef, msg);
				} catch (BrokerUnreachableException e) {
					throw new ConnectionException (QRef, e);
				}
			}
		}
		
		public void Send (IMessage msg, IMessageQueueTransaction transaction)
		{
			if (QRef == QueueReference.DEFAULT)
				throw new MonoMessagingException ("Path has not been specified");
			
			if (msg.BodyStream == null)
				throw new ArgumentException ("Message is not serialized properly");
			
			RabbitMQMessageQueueTransaction tx = (RabbitMQMessageQueueTransaction) transaction;
			
			SetDeliveryInfo (msg, tx.Id);
			tx.Send (QRef, msg);
		}
		
		public void Send (IMessage msg, MessageQueueTransactionType transactionType)
		{
			switch (transactionType) {
			case MessageQueueTransactionType.Single:
				using (IMessageQueueTransaction tx = NewTx ()) {
					try {
						Send (msg, tx);
						tx.Commit ();
					} catch (Exception e) {
						tx.Abort ();
						throw new MonoMessagingException(e.Message, e);
					}
				}
				break;

			case MessageQueueTransactionType.None:
				Send (msg);
				break;

			case MessageQueueTransactionType.Automatic:
				throw new NotSupportedException("Automatic transaction types not supported");
			}
		}
		
		public void Purge ()
		{
			using (IMessagingContext context = CurrentContext) {
				context.Purge (QRef);
			}
		}
		
		public IMessage Peek ()
		{
			return DoReceive (TimeSpan.MaxValue, null, false);
		}
		
		public IMessage Peek (TimeSpan timeout)
		{
			return DoReceive (timeout, null, false);
		}
		
		public IMessage PeekById (string id)
		{
			return DoReceive (noTime, ById (id), false);
		}

		public IMessage PeekById (string id, TimeSpan timeout)
		{
			return DoReceive (timeout, ById (id), false);
		}
		
		public IMessage PeekByCorrelationId (string id)
		{
			return DoReceive (noTime, ByCorrelationId (id), false);
		}

		public IMessage PeekByCorrelationId (string id, TimeSpan timeout)
		{
			return DoReceive (timeout, ByCorrelationId (id), false);
		}
		
		public IMessage Receive ()
		{
			return DoReceive (TimeSpan.MaxValue, null, true);
		}
		
		public IMessage Receive (TimeSpan timeout)
		{
			return DoReceive (timeout, null, true);
		}
		
		public IMessage Receive (TimeSpan timeout,
		                         IMessageQueueTransaction transaction)
		{
			return DoReceive (transaction, timeout, null, true);
		}
		
		public IMessage Receive (TimeSpan timeout,
		                         MessageQueueTransactionType transactionType)
		{
			return DoReceive (transactionType, timeout, null, true);
		}
		
		public IMessage Receive (IMessageQueueTransaction transaction)
		{
			return DoReceive (transaction, TimeSpan.MaxValue, null, true);
		}
		
		public IMessage Receive (MessageQueueTransactionType transactionType)
		{
			return DoReceive (transactionType, TimeSpan.MaxValue, null, true);
		}

		public IMessage ReceiveById (string id)
		{
			return DoReceive (noTime, ById (id), true);
		}

		public IMessage ReceiveById (string id, TimeSpan timeout)
		{
			return DoReceive (timeout, ById (id), true);
		}
		
		public IMessage ReceiveById (string id,
		                             IMessageQueueTransaction transaction)
		{
			return DoReceive (transaction, noTime, ById (id), true);
		}
		
		public IMessage ReceiveById (string id,
		                             MessageQueueTransactionType transactionType)
		{
			return DoReceive (transactionType, noTime, ById (id), true);
		}
		
		public IMessage ReceiveById (string id, TimeSpan timeout,
		                             IMessageQueueTransaction transaction)
		{
			return DoReceive (transaction, timeout, ById (id), true);
		}
		
		public IMessage ReceiveById (string id, TimeSpan timeout,
		                             MessageQueueTransactionType transactionType)
		{
			return DoReceive (transactionType, timeout, ById (id), true);
		}
		
		public IMessage ReceiveByCorrelationId (string id)
		{
			return DoReceive (noTime, ByCorrelationId (id), true);
		}
		
		public IMessage ReceiveByCorrelationId (string id, TimeSpan timeout)
		{
			return DoReceive (timeout, ByCorrelationId (id), true);
		}
		
		public IMessage ReceiveByCorrelationId (string id,
		                                        IMessageQueueTransaction transaction)
		{
			return DoReceive (transaction, noTime, ByCorrelationId (id), true);
		}
		
		public IMessage ReceiveByCorrelationId (string id,
		                                        MessageQueueTransactionType transactionType)
		{
			return DoReceive (transactionType, noTime, ByCorrelationId (id), true);
		}
		
		public IMessage ReceiveByCorrelationId (string id, TimeSpan timeout,
		                                        IMessageQueueTransaction transaction)
		{
			return DoReceive (transaction, timeout, ByCorrelationId (id), true);
		}
		
		public IMessage ReceiveByCorrelationId (string id, TimeSpan timeout,
		                                        MessageQueueTransactionType transactionType)
		{
			return DoReceive (transactionType, timeout, ByCorrelationId (id), true);
		}
		
		public IMessageEnumerator GetMessageEnumerator ()
		{
			return new RabbitMQMessageEnumerator (helper, QRef);
		}
		
		private IMessage DoReceive (MessageQueueTransactionType transactionType,
									TimeSpan timeout, IsMatch matcher, bool ack)
		{
			switch (transactionType) {
			case MessageQueueTransactionType.Single:
				using (RabbitMQMessageQueueTransaction tx = NewTx ()) {
					bool success = false;
					try {
						IMessage msg = DoReceive ((IMessagingContext) tx, timeout, matcher, ack);
						tx.Commit ();
						success = true;
						return msg;
					} finally {
						if (!success)
							tx.Abort ();
					}
				}

			case MessageQueueTransactionType.None:
				return DoReceive (timeout, matcher, true);

			default:
				throw new NotSupportedException (transactionType + " not supported");
			}
		}
		
		private IMessage DoReceive (IMessageQueueTransaction transaction,
									TimeSpan timeout, IsMatch matcher, bool ack)
		{
			RabbitMQMessageQueueTransaction tx = (RabbitMQMessageQueueTransaction) transaction;
			return DoReceive ((IMessagingContext) tx, timeout, matcher, ack);
		}
		
		private IMessage DoReceive (TimeSpan timeout, IsMatch matcher, bool ack)
		{
			using (IMessagingContext context = CurrentContext) {
				return DoReceive (context, timeout, matcher, ack);
			}
		}
		
		private IMessage DoReceive (IMessagingContext context, TimeSpan timeout,
									IsMatch matcher, bool ack)
		{
			return context.Receive (QRef, timeout, matcher, ack);
		}
		
		private IMessagingContext CurrentContext {
			get {
				return provider.CreateContext (qRef.Host);
			}
		}
		
		private class IdMatcher
		{
			private readonly string id;
			public IdMatcher (string id)
			{
				this.id = id;
			}
			
			public bool MatchById (BasicDeliverEventArgs result)
			{
				return result.BasicProperties.MessageId == id;
			}
		}
		
		private static IsMatch ById (string id)
		{
			return new IdMatcher (id).MatchById;
		}
		
		private class CorrelationIdMatcher
		{
			private readonly string correlationId;
			public CorrelationIdMatcher (string correlationId)
			{
				this.correlationId = correlationId;
			}
			
			public bool MatchById (BasicDeliverEventArgs result)
			{
				return result.BasicProperties.CorrelationId == correlationId;
			}
		}
		
		private static IsMatch ByCorrelationId (string correlationId)
		{
			return new CorrelationIdMatcher (correlationId).MatchById;
		}
		
		private RabbitMQMessageQueueTransaction NewTx ()
		{
			return (RabbitMQMessageQueueTransaction) provider.CreateMessageQueueTransaction ();
		}
	}
}
