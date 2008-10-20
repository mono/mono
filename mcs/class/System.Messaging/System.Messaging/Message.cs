//
// System.Messaging
//
// Authors:
//	  Peter Van Isacker (sclytrack@planetinternet.be)
//	  Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
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
using System.IO;
using System.ComponentModel;

using Mono.Messaging;

namespace System.Messaging 
{
	[DesignerAttribute ("System.Messaging.Design.MessageDesigner, " + Consts.AssemblySystem_Design)]
	public class Message: Component 
	{
		private readonly IMessage delegateMessage;
		private IMessageFormatter formatter;
		//private int bodyType = 0;
		private object body;
		
		#region Constructor
		
		public Message() : this (CreateMessage (), null, null)
		{
		}

		public Message (object body) : this (CreateMessage (), body, null)
		{
		}

		public Message (object body, IMessageFormatter formatter) 
			: this (CreateMessage (), body, formatter)
		{
		}
		
		internal Message (IMessage delegateMessage, object body, 
			IMessageFormatter formatter)
		{
			this.delegateMessage = delegateMessage;
			this.body = body;
			this.formatter = formatter;
		}
		
		#endregion //Constructor
		
		[MonoTODO]
		public static readonly TimeSpan InfiniteTimeout;
		
		#region Properties

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAcknowledgeType")]
		public AcknowledgeTypes AcknowledgeType {
			get { 
				return (AcknowledgeTypes) delegateMessage.AcknowledgeType;
			}
			set { 
				delegateMessage.AcknowledgeType = (Mono.Messaging.AcknowledgeTypes) value; 
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAcknowledgement")]
		public Acknowledgment Acknowledgment {
			get { 
				return (Acknowledgment) (int) delegateMessage.Acknowledgment;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAdministrationQueue")]
		public MessageQueue AdministrationQueue {
			get {
				if (delegateMessage.AdministrationQueue == null)
					return null;
					
				return new MessageQueue 
					(delegateMessage.AdministrationQueue);
			}
			set { 
				delegateMessage.AdministrationQueue 
					= value.DelegateQueue;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAppSpecific")]
		public int AppSpecific {
			get { return delegateMessage.AppSpecific; }
			set { delegateMessage.AppSpecific = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgArrivedTime")]
		public DateTime ArrivedTime {
			get { return delegateMessage.ArrivedTime; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAttachSenderId")]
		public bool AttachSenderId {
			get { return delegateMessage.AttachSenderId; }
			set { delegateMessage.AttachSenderId = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgAuthenticated")]
		public bool Authenticated {
			get { return delegateMessage.Authenticated; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAuthenticationProviderName")]
		public string AuthenticationProviderName {
			get { return delegateMessage.AuthenticationProviderName; }
			set { delegateMessage.AuthenticationProviderName = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAuthenticationProviderType")]
		public CryptographicProviderType AuthenticationProviderType {
			get { 
				return (CryptographicProviderType)
					delegateMessage.AuthenticationProviderType;
			}
			set { 
				delegateMessage.AuthenticationProviderType = 
					(Mono.Messaging.CryptographicProviderType) value; 
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public object Body {
			get {
				if (body == null && delegateMessage.BodyStream == null)
					return null;
				else if (body == null)
					body = formatter.Read (this);
					
				return body;
			}
			set { body = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[Editor ("System.ComponentModel.Design.BinaryEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MessagingDescription ("MsgBodyStream")]
		public Stream BodyStream {
			get { return delegateMessage.BodyStream; }
			set { delegateMessage.BodyStream = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgBodyType")]
		[ReadOnly (true)]
		public int BodyType {
			get { return delegateMessage.BodyType; }
			set { delegateMessage.BodyType = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgConnectorType")]
		public Guid ConnectorType {
			get { return delegateMessage.ConnectorType; }
			set { delegateMessage.ConnectorType = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgCorrelationId")]
		public string CorrelationId {
			get { return delegateMessage.CorrelationId; }
			set { delegateMessage.CorrelationId = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MessagingDescription("MsgDestinationQueue")]
		public MessageQueue DestinationQueue {
			get { 
				return new MessageQueue(delegateMessage.DestinationQueue);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgDestinationSymmetricKey")]
		public byte[] DestinationSymmetricKey {
			get { return delegateMessage.DestinationSymmetricKey; }
			set { delegateMessage.DestinationSymmetricKey = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgDigitalSignature")]
		public byte[] DigitalSignature {
			get { return delegateMessage.DigitalSignature; }
			set { delegateMessage.DigitalSignature = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgEncryptionAlgorithm")]
		public EncryptionAlgorithm EncryptionAlgorithm {
			get { 
				return (EncryptionAlgorithm) delegateMessage.EncryptionAlgorithm;
			}
			set {
				delegateMessage.EncryptionAlgorithm = 
					(Mono.Messaging.EncryptionAlgorithm) value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgExtension")]
		public byte[] Extension {
			get { return delegateMessage.Extension; }
			set { delegateMessage.Extension = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public IMessageFormatter Formatter {
			get { return formatter; }
			set { formatter = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgHashAlgorithm")]
		public HashAlgorithm HashAlgorithm {
			get { 
				return (HashAlgorithm) delegateMessage.HashAlgorithm;
			}
			set { 
				delegateMessage.HashAlgorithm = 
					(Mono.Messaging.HashAlgorithm) value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgId")]
		public string Id {
			get { return delegateMessage.Id; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MessagingDescription("MsgIsFirstInTransaction")]
		public bool IsFirstInTransaction {
			get { return delegateMessage.IsFirstInTransaction; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MessagingDescription("MsgIsLastInTransaction")]
		public bool IsLastInTransaction {
			get { return delegateMessage.IsLastInTransaction; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgLabel")]
		public string Label {
			get { return delegateMessage.Label; }
			set { delegateMessage.Label = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MessagingDescription("MsgMessageType")]
		public MessageType MessageType {
			get { 
				return (MessageType) delegateMessage.MessageType;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgPriority")]
		public MessagePriority Priority {
			get {
				return (MessagePriority) delegateMessage.Priority;
			}
			set { 
				delegateMessage.Priority = (Mono.Messaging.MessagePriority) value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgRecoverable")]
		public bool Recoverable {
			get { return delegateMessage.Recoverable; }
			set { delegateMessage.Recoverable = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgResponseQueue")]
		public MessageQueue ResponseQueue {
			get { 
				if (delegateMessage.ResponseQueue == null)
					return null;
					
				return new MessageQueue 
					(delegateMessage.ResponseQueue);
			}
			set { 
				delegateMessage.ResponseQueue 
					= value.DelegateQueue;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgSenderCertificate")]
		public byte[] SenderCertificate {
			get { return delegateMessage.SenderCertificate; }
			set { delegateMessage.SenderCertificate = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgSenderId")]
		public byte[] SenderId {
			get { return delegateMessage.SenderId; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgSenderVersion")]
		public long SenderVersion {
			get { return delegateMessage.SenderVersion; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgSentTime")]
		public DateTime SentTime {
			get { return delegateMessage.SentTime; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgSourceMachine")]
		public string SourceMachine {
			get { return delegateMessage.SourceMachine; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgTimeToBeReceived")]
		[TypeConverter (typeof(TimeoutConverter))]
		public TimeSpan TimeToBeReceived {
			get { return delegateMessage.TimeToBeReceived; }
			set { delegateMessage.TimeToBeReceived = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgTimeToReachQueue")]
		[TypeConverter (typeof(TimeoutConverter))]
		public TimeSpan TimeToReachQueue {
			get { return delegateMessage.TimeToReachQueue; }
			set { delegateMessage.TimeToReachQueue = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgTransactionId")]
		public string TransactionId {
			get { return delegateMessage.TransactionId; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgTransactionStatusQueue")]
		public MessageQueue TransactionStatusQueue {
			get { 
				return new MessageQueue(delegateMessage.TransactionStatusQueue);
			}
			set { 
				delegateMessage.TransactionStatusQueue = value.DelegateQueue;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseAuthentication")]
		public bool UseAuthentication {
			get { return delegateMessage.UseAuthentication; }
			set { delegateMessage.UseAuthentication = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseDeadLetterQueue")]
		public bool UseDeadLetterQueue {
			get { return delegateMessage.UseDeadLetterQueue; }
			set { delegateMessage.UseDeadLetterQueue = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseEncryption")]
		public bool UseEncryption {
			get { return delegateMessage.UseEncryption; }
			set { delegateMessage.UseEncryption = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseJournalQueue")]
		public bool UseJournalQueue {
			get { return delegateMessage.UseJournalQueue; }
			set { delegateMessage.UseJournalQueue = value;}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseTracing")]
		public bool UseTracing {
			get { return delegateMessage.UseTracing; }
			set { delegateMessage.UseTracing = value; }
		}
		
		internal IMessage DelegateMessage {
			get { return delegateMessage; }
		}

		#endregion //Properties
		
		internal static IMessage CreateMessage ()
		{
			return MessagingProviderLocator.GetProvider ()
				.CreateMessage ();
		}	
	}
}
