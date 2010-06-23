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
		private readonly RabbitMQMessagingProvider provider;
		private readonly MessageFactory helper;
		private readonly bool transactional;
		
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
		
		private static long GetVersion (IConnection cn)
		{
			long version = cn.Protocol.MajorVersion;
			version = version << 32;
			version += cn.Protocol.MinorVersion;
			return version;
		}
		
		private void SetDeliveryInfo (IMessage msg, long senderVersion,
		                              string transactionId)
		{
			msg.SetDeliveryInfo (Acknowledgment.None,
			                     DateTime.MinValue,
			                     this,
			                     Guid.NewGuid ().ToString () + "\\0",
			                     MessageType.Normal,
			                     new byte[0],
			                     senderVersion,
			                     DateTime.UtcNow,
			                     null,
			                     transactionId);
		}
		
		public void Close ()
		{
			// No-op (Queue are currently stateless)
		}
		
		public static void Delete (QueueReference qRef)
		{
			using (IConnection cn = CreateConnection (qRef)) {
				using (IModel model = cn.CreateModel ()) {
					model.QueueDelete (qRef.Queue, false, false, false);
				}
			}
		}			
		
		public void Send (IMessage msg)
		{
			if (QRef == QueueReference.DEFAULT)
				throw new MonoMessagingException ("Path has not been specified");
			
			if (msg.BodyStream == null)
				throw new ArgumentException ("Message is not serialized properly");
		
			try {
				using (IConnection cn = CreateConnection (QRef)) {
					SetDeliveryInfo (msg, GetVersion (cn), null);
					using (IModel ch = cn.CreateModel ()) {
						Send (ch, msg);
					}
				}
			} catch (BrokerUnreachableException e) {
				throw new ConnectionException (QRef, e);
			}
		}
		
		public void Send (IMessage msg, IMessageQueueTransaction transaction)
		{
			if (QRef == QueueReference.DEFAULT)
				throw new MonoMessagingException ("Path has not been specified");
			
			if (msg.BodyStream == null)
				throw new ArgumentException ("Message is not serialized properly");
			
			RabbitMQMessageQueueTransaction tx = (RabbitMQMessageQueueTransaction) transaction;
			
			tx.RunSend (SendInContext, msg);
		}
		
		public void Send (IMessage msg, MessageQueueTransactionType transactionType)
		{
			switch (transactionType) {
			case MessageQueueTransactionType.Single:
				using (IMessageQueueTransaction tx = provider.CreateMessageQueueTransaction ()) {
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
		
		private void SendInContext (ref string host, ref IConnection cn, 
		                            ref IModel model, IMessage msg, string txId)
		{
			if (host == null)
				host = QRef.Host;
			else if (host != QRef.Host)
				throw new MonoMessagingException ("Transactions can not span multiple hosts");
			
			if (cn == null)
				cn = CreateConnection (QRef);
			
			if (model == null) {
				model = cn.CreateModel ();
				model.TxSelect ();
			}
			
			SetDeliveryInfo (msg, GetVersion (cn), txId);
			Send (model, msg);
		}
		
		private void Send (IModel model, IMessage msg)
		{
			string finalName = model.QueueDeclare (QRef.Queue, true);
			IMessageBuilder mb = helper.WriteMessage (model, msg);

			model.BasicPublish ("", finalName,
			                    (IBasicProperties) mb.GetContentHeader(),
			                    mb.GetContentBody ());
		}
		
		public void Purge ()
		{
			using (IConnection cn = CreateConnection (QRef)) {
				using (IModel model = cn.CreateModel ()) {
					model.QueuePurge (QRef.Queue, false);
				}
			}
		}
		
		public IMessage Peek ()
		{
			using (IConnection cn = CreateConnection (QRef)) {
				using (IModel ch = cn.CreateModel ()) {
					return Receive (ch, -1, false);
				}
			}
		}
		
		public IMessage Peek (TimeSpan timeout)
		{
			return Run (Peeker (timeout));
		}
		
		public IMessage PeekById (string id)
		{
			return Run (Peeker (ById (id)));
		}

		public IMessage PeekById (string id, TimeSpan timeout)
		{
			return Run (Peeker (timeout, ById (id)));
		}
		
		public IMessage PeekByCorrelationId (string id)
		{
			return Run (Peeker (ByCorrelationId (id)));
		}

		public IMessage PeekByCorrelationId (string id, TimeSpan timeout)
		{
			return Run (Peeker (timeout, ByCorrelationId (id)));
		}
		
		public IMessage Receive ()
		{
			return Run (Receiver ());
		}
		
		public IMessage Receive (TimeSpan timeout)
		{
			return Run (Receiver (timeout));
		}
		
		public IMessage Receive (TimeSpan timeout,
		                         IMessageQueueTransaction transaction)
		{
			return Run (transaction, Receiver (timeout));
		}
		
		public IMessage Receive (TimeSpan timeout,
		                         MessageQueueTransactionType transactionType)
		{
			return Run (transactionType, Receiver (timeout));
		}
		
		public IMessage Receive (IMessageQueueTransaction transaction)
		{
			return Run (transaction, Receiver());
		}
		
		public IMessage Receive (MessageQueueTransactionType transactionType)
		{
			return Run (transactionType, Receiver ());
		}		

		public IMessage ReceiveById (string id)
		{
			return Run (Receiver (ById (id)));
		}

		public IMessage ReceiveById (string id, TimeSpan timeout)
		{
			return Run (Receiver (timeout, ById (id)));
		}
		
		public IMessage ReceiveById (string id,
		                             IMessageQueueTransaction transaction)
		{
			return Run (transaction, Receiver (ById (id)));
		}
		
		public IMessage ReceiveById (string id,
		                             MessageQueueTransactionType transactionType)
		{
			return Run (transactionType, Receiver (ById (id)));
		}
		
		public IMessage ReceiveById (string id, TimeSpan timeout,
		                             IMessageQueueTransaction transaction)
		{
			return Run (transaction, Receiver (timeout, ById (id)));
		}
		
		public IMessage ReceiveById (string id, TimeSpan timeout,
		                             MessageQueueTransactionType transactionType)
		{
			return Run (transactionType, Receiver (timeout, ById (id)));
		}
		
		public IMessage ReceiveByCorrelationId (string id)
		{
			return Run (Receiver (ByCorrelationId (id)));
		}
		
		public IMessage ReceiveByCorrelationId (string id, TimeSpan timeout)
		{
			return Run (Receiver (timeout, ByCorrelationId (id)));
		}
		
		public IMessage ReceiveByCorrelationId (string id,
		                                        IMessageQueueTransaction transaction)
		{
			return Run (transaction, Receiver (ByCorrelationId (id)));
		}
		
		public IMessage ReceiveByCorrelationId (string id,
		                                        MessageQueueTransactionType transactionType)
		{
			return Run (transactionType, Receiver (ByCorrelationId (id)));
		}
		
		public IMessage ReceiveByCorrelationId (string id, TimeSpan timeout,
		                                        IMessageQueueTransaction transaction)
		{
			return Run (transaction, Receiver (timeout, ByCorrelationId (id)));
		}
		
		public IMessage ReceiveByCorrelationId (string id, TimeSpan timeout,
		                                        MessageQueueTransactionType transactionType)
		{
			return Run (transactionType, Receiver (timeout, ByCorrelationId (id)));
		}
		
		public IMessageEnumerator GetMessageEnumerator ()
		{
			return new RabbitMQMessageEnumerator (helper, QRef);
		}
		
		private delegate IMessage RecieveDelegate (RabbitMQMessageQueue q,
		                                           IModel model);
		
		private IMessage Run (MessageQueueTransactionType transactionType,
		                      RecieveDelegate r)
		{
			switch (transactionType) {
			case MessageQueueTransactionType.Single:
				using (RabbitMQMessageQueueTransaction tx = GetTx ()) {
					bool success = false;
					try {
						IMessage msg = Run (tx, r);
						tx.Commit ();
						success = true;
						return msg;
					} finally {
						if (!success)
							tx.Abort ();
					}
				}

			case MessageQueueTransactionType.None:
				return Run (r);

			default:
				throw new NotSupportedException(transactionType + " not supported");
			}
		}		

		private IMessage Run (IMessageQueueTransaction transaction, 
		                      RecieveDelegate r)
		{
			TxReceiver txr = new TxReceiver (this, r);
			RabbitMQMessageQueueTransaction tx = 
				(RabbitMQMessageQueueTransaction) transaction;
			return tx.RunReceive (txr.ReceiveInContext);			
		}
		
		private IMessage Run (RecieveDelegate r)
		{
			using (IConnection cn = CreateConnection (QRef)) {
				using (IModel model = cn.CreateModel ()) {
					return r (this, model);
				}
			}
		}
		
		private class TxReceiver
		{
			private readonly RecieveDelegate doReceive;
			private readonly RabbitMQMessageQueue q;
			
			public TxReceiver(RabbitMQMessageQueue q, RecieveDelegate doReceive) {
				this.q = q;
				this.doReceive = doReceive;
			}
						
			public IMessage ReceiveInContext (ref string host, ref IConnection cn, 
			                                  ref IModel model, string txId)
			{
				if (host == null)
					host = q.QRef.Host;
				else if (host != q.QRef.Host)
					throw new MonoMessagingException ("Transactions can not span multiple hosts");
				
				if (cn == null)
					cn = CreateConnection (q.QRef);
				
				if (model == null) {
					model = cn.CreateModel ();
					model.TxSelect ();
				}
				
				return doReceive (q, model);
			}
		}
		
		private class RecieveDelegateFactory
		{
			private readonly int timeout;
			private readonly IsMatch matcher;
			private readonly bool ack;
			
			public RecieveDelegateFactory (int timeout, IsMatch matcher)
				: this (timeout, matcher, true)
			{
			}
			
			public RecieveDelegateFactory (int timeout, IsMatch matcher, bool ack)
			{
				if (matcher != null && timeout == -1)
					this.timeout = 500;
				else 
					this.timeout = timeout;
				this.matcher = matcher;
				this.ack = ack;
			}
			
			public IMessage RecieveDelegate (RabbitMQMessageQueue q, IModel model)
			{
				if (matcher == null)
					return q.Receive (model, timeout, ack);
				else
					return q.Receive (model, timeout, ack, matcher);
			}
		}
		
		private static RecieveDelegate Receiver (TimeSpan timeout,
		                                         IsMatch matcher)
		{
			int to = MessageFactory.TimeSpanToInt32 (timeout);
			return new RecieveDelegateFactory (to, matcher).RecieveDelegate;
		}
		
		private static RecieveDelegate Receiver (IsMatch matcher)
		{
			return new RecieveDelegateFactory (-1, matcher).RecieveDelegate;
		}
		
		private static RecieveDelegate Receiver (TimeSpan timeout)
		{
			int to = MessageFactory.TimeSpanToInt32 (timeout);
			return new RecieveDelegateFactory (to, null).RecieveDelegate;
		}

		private RecieveDelegate Receiver ()
		{
			return new RecieveDelegateFactory (-1, null).RecieveDelegate;
		}		
		
		private RecieveDelegate Peeker (TimeSpan timeout)
		{
			int to = MessageFactory.TimeSpanToInt32 (timeout);
			return new RecieveDelegateFactory (to, null, false).RecieveDelegate;
		}		
		
		private RecieveDelegate Peeker (IsMatch matcher)
		{
			return new RecieveDelegateFactory (-1, matcher, false).RecieveDelegate;
		}		
		
		private RecieveDelegate Peeker (TimeSpan timeout, IsMatch matcher)
		{
			int to = MessageFactory.TimeSpanToInt32 (timeout);
			return new RecieveDelegateFactory (to, matcher, false).RecieveDelegate;
		}
		
		delegate bool IsMatch (BasicDeliverEventArgs result);		
		
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
		
		private IMessage Receive (IModel model, int timeout, bool doAck)
		{
			string finalName = model.QueueDeclare (QRef.Queue, false);
			
			using (Subscription sub = new Subscription (model, finalName)) {
				BasicDeliverEventArgs result;
				if (sub.Next (timeout, out result)) {
					IMessage m = helper.ReadMessage (QRef, result);
					if (doAck)
						sub.Ack (result);
					return m;
				} else {
					throw new MonoMessagingException ("No Message Available");
				}
			}
		}
				
		private IMessage Receive (IModel model, int timeout, 
		                          bool doAck, IsMatch matcher)
		{
			string finalName = model.QueueDeclare (QRef.Queue, false);
			
			using (Subscription sub = new Subscription (model, finalName)) {
				BasicDeliverEventArgs result;
				while (sub.Next (timeout, out result)) {
					
					if (matcher (result)) {
						IMessage m = helper.ReadMessage (QRef, result);
						if (doAck)
							sub.Ack (result);
						return m;
					}
				}
				
				throw new MessageUnavailableException ("Message not available");
			}
		}
		
		private RabbitMQMessageQueueTransaction GetTx ()
		{
			return (RabbitMQMessageQueueTransaction) provider.CreateMessageQueueTransaction ();
		}

		private static IConnection CreateConnection (QueueReference qRef)
		{
			ConnectionFactory cf = new ConnectionFactory ();
			cf.Address = qRef.Host;
			return cf.CreateConnection ();
		}		
	}
}
