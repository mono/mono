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

	public class RabbitMQMessageQueue : IMessageQueue {
		
		private static readonly string SENDER_VERSION_KEY = "SenderVersion";
		private static readonly string SOURCE_MACHINE_KEY = "SourceMachine";
		private static readonly string BODY_TYPE_KEY = "BodyType";
		
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
		
		public RabbitMQMessageQueue ()
		{
		}
		
		public RabbitMQMessageQueue (QueueReference qRef)
		{
			this.qRef = qRef;
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
			get { throw new NotImplementedException (); }
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
		
		private void SetDeliveryInfo (IMessage msg, IConnection cn)
		{
			long senderVersion = GetVersion (cn);
			msg.SetDeliveryInfo (Acknowledgment.None,
			                     DateTime.MinValue,
			                     this,
			                     Guid.NewGuid ().ToString (),
			                     MessageType.Normal,
			                     new byte[0],
			                     senderVersion,
			                     null,
			                     null);
		}
		
		public void Send (IMessage msg)
		{
			if (msg.BodyStream == null)
				throw new ArgumentException ("Message is not serialized properly");
		
			ConnectionFactory cf = new ConnectionFactory ();
			
			try {
				using (IConnection cn = cf.CreateConnection (QRef.Host)) {
					using (IModel ch = cn.CreateModel ()) {
						ushort ticket = ch.AccessRequest ("/data");
						string finalName = ch.QueueDeclare (ticket, QRef.Queue, false);
						SetDeliveryInfo (msg, cn);
						IMessageBuilder mb = MessageFactory.WriteMessage (ch, msg);
						Console.WriteLine("Body.Length In {0}", mb.GetContentBody ().Length);
												
						ch.BasicPublish (ticket, "",
						                 finalName,
						                 (IBasicProperties) mb.GetContentHeader(),
						                 mb.GetContentBody ());
					}
				}
			} catch (BrokerUnreachableException e) {
				throw new ConnectionException (QRef);
			}
		}
		
		public IMessage Receive ()
		{
			ConnectionFactory cf = new ConnectionFactory ();

			using (IConnection cn = cf.CreateConnection (QRef.Host)) {
				using (IModel ch = cn.CreateModel ()) {
					ushort ticket = ch.AccessRequest ("/data");
					string finalName = ch.QueueDeclare (ticket, QRef.Queue, false);
					
					Subscription sub = new Subscription (ch, ticket, finalName);
					BasicDeliverEventArgs result = sub.Next ();
					sub.Ack (result);
					sub.Close ();
					if (result == null) {
						throw new MonoMessagingException ("No Message Available");
					} else {
						DebugUtil.DumpProperties(result, Console.Out, 0);
						
						IMessage m = MessageFactory.ReadMessage (result);
						return m;
					}
				}
			}
		}
		
		public IMessageEnumerator GetMessageEnumerator ()
		{
			return new RabbitMQMessageEnumerator (QRef);
		}
	}
}
