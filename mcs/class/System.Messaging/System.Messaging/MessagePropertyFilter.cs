//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
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
using System.ComponentModel;

namespace System.Messaging
{
	[TypeConverter (typeof(ExpandableObjectConverter))]
	public class MessagePropertyFilter
	{
		private bool acknowledgeType = false;
		private bool acknowledgment = false;
		private bool administrationQueue = false;
		private bool appSpecific = false;
		private bool arrivedTime = false;
		private bool attachSenderId = false;
		private bool authenticated = false;
		private bool authenticationProviderName = false;
		private bool authenticationProviderType = false;
		private bool body = false;
		private bool connectorType = false;
		private bool correlationId = false;
		private int defaultBodySize = 1024;
		private int defaultExtensionSize = 255;
		private int defaultLabelSize = 255;
		private bool destinationQueue = false;
		private bool destinationSymmetricKey = false;
		private bool digitalSignature = false;
		private bool encryptionAlgorithm = false;
		private bool extension = false;
		private bool hashAlgorithm = false;
		private bool id = false;
		private bool isFirstInTransaction = false;
		private bool isLastInTransaction = false;
		private bool label = false;
		private bool messageType = false;
		private bool priority = false;
		private bool recoverable = false;
		private bool responseQueue = false;
		private bool senderCertificate = false;
		private bool senderId = false;
		private bool senderVersion = false;
		private bool sentTime = false;
		private bool sourceMachine = false;
		private bool timeToBeReceived = false;
		private bool timeToReachQueue = false;
		private bool transactionId = false;
		private bool transactionStatusQueue = false;
		private bool useAuthentication = false;
		private bool useDeadLetterQueue = false;
		private bool useEncryption = false;
		private bool useJournalQueue = false;
		private bool useTracing = false;

		[MonoTODO]
		public MessagePropertyFilter ()
		{

		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgAcknowledgeType")]
		public bool AcknowledgeType
		{
			get { return acknowledgeType; }
			set { acknowledgeType = value; }
		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgAcknowledgement")]
		public bool Acknowledgment
		{
			get { return acknowledgment; }
			set { acknowledgment = value; }
		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgAdministrationQueue")]
		public bool AdministrationQueue
		{
			get { return administrationQueue; }
			set { administrationQueue = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgAppSpecific")]
		public bool AppSpecific
		{
			get { return appSpecific; }
			set { appSpecific = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgArrivedTime")]
		public bool ArrivedTime
		{
			get { return arrivedTime; }
			set { arrivedTime = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgAttachSenderId")]
		public bool AttachSenderId
		{
			get { return attachSenderId; }
			set { attachSenderId = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgAuthenticated")]
		public bool Authenticated
		{
			get { return authenticated; }
			set { authenticated = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgAuthenticationProviderName")]
		public bool AuthenticationProviderName
		{
			get { return authenticationProviderName; }
			set { authenticationProviderName = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgAuthenticationProviderType")]
		public bool AuthenticationProviderType
		{
			get { return authenticationProviderType; }
			set { authenticationProviderType = value; }
		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgBody")]
		public bool Body
		{
			get { return body; }
			set { body = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgConnectorType")]
		public bool ConnectorType
		{
			get { return connectorType; }
			set { connectorType = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgCorrelationId")]
		public bool CorrelationId
		{
			get { return correlationId; }
			set { correlationId = value; }
		}

		[DefaultValue (1024)]
		[MessagingDescription ("MsgDefaultBodySize")]
		public int DefaultBodySize
		{
			get { return defaultBodySize; }
			set
			{
				if (value < 0) 
					throw new ArgumentException ("DefaultBodySize");
				defaultBodySize = value;
			}
		}

		[DefaultValue (255)]
		[MessagingDescription ("MsgDefaultExtensionSize")]
		public int DefaultExtensionSize
		{
			get { return defaultExtensionSize; }
			set
			{
				if (value < 0) 
					throw new ArgumentException ("DefaultExtensionSize");
				defaultExtensionSize = value;
			}
		}

		[DefaultValue (255)]
		[MessagingDescription ("MsgDefaultLabelSize")]
		public int DefaultLabelSize
		{
			get { return defaultLabelSize; }
			set
			{
				if (value < 0) throw new ArgumentException ("DefaultLabelSize");
				defaultLabelSize = value;
			}
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgDestinationQueue")]
		public bool DestinationQueue
		{
			get { return destinationQueue; }
			set { destinationQueue = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgDestinationSymmetricKey")]
		public bool DestinationSymmetricKey
		{
			get { return destinationSymmetricKey; }
			set { destinationSymmetricKey = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgDigitalSignature")]
		public bool DigitalSignature
		{
			get { return digitalSignature; }
			set { digitalSignature = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgEncryptionAlgorithm")]
		public bool EncryptionAlgorithm
		{
			get { return encryptionAlgorithm; }
			set { encryptionAlgorithm = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgExtension")]
		public bool Extension
		{
			get { return extension; }
			set { extension = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgHashAlgorithm")]
		public bool HashAlgorithm
		{
			get { return hashAlgorithm; }
			set { hashAlgorithm = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgId")]
		public bool Id
		{
			get { return id; }
			set { id = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgIsFirstInTransaction")]
		public bool IsFirstInTransaction
		{
			get { return isFirstInTransaction; }
			set { isFirstInTransaction = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgIsLastInTransaction")]
		public bool IsLastInTransaction
		{
			get { return isLastInTransaction; }
			set { isLastInTransaction = value; }
		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgLabel")]
		public bool Label
		{
			get { return label; }
			set { label = value; }
		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgMessageType")]
		public bool MessageType
		{
			get { return messageType; }
			set { messageType = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgPriority")]
		public bool Priority
		{
			get { return priority; }
			set { priority = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgRecoverable")]
		public bool Recoverable
		{
			get { return recoverable; }
			set { recoverable = value; }
		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgResponseQueue")]
		public bool ResponseQueue
		{
			get { return responseQueue; }
			set { responseQueue = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgSenderCertificate")]
		public bool SenderCertificate
		{
			get { return senderCertificate; }
			set { senderCertificate = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgSenderId")]
		public bool SenderId
		{
			get { return senderId; }
			set { senderId = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgSenderVersion")]
		public bool SenderVersion
		{
			get { return senderVersion; }
			set { senderVersion = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgSentTime")]
		public bool SentTime
		{
			get { return sentTime; }
			set { sentTime = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgSourceMachine")]
		public bool SourceMachine
		{
			get { return sourceMachine; }
			set { sourceMachine = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgTimeToBeReceived")]
		public bool TimeToBeReceived
		{
			get { return timeToBeReceived; }
			set { timeToBeReceived = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgTimeToReachQueue")]
		public bool TimeToReachQueue
		{
			get { return timeToReachQueue; }
			set { timeToReachQueue = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgTransactionId")]
		public bool TransactionId
		{
			get { return transactionId; }
			set { transactionId = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgTransactionStatusQueue")]
		public bool TransactionStatusQueue
		{
			get { return transactionStatusQueue; }
			set { transactionStatusQueue = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgUseAuthentication")]
		public bool UseAuthentication
		{
			get { return useAuthentication; }
			set { useAuthentication = value; }
		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgUseDeadLetterQueue")]
		public bool UseDeadLetterQueue
		{
			get { return useDeadLetterQueue; }
			set { useDeadLetterQueue = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgUseEncryption")]
		public bool UseEncryption
		{
			get { return useEncryption; }
			set { useEncryption = value; }
		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgUseJournalQueue")]
		public bool UseJournalQueue
		{
			get { return useJournalQueue; }
			set { useJournalQueue = value; }
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgUseTracing")]
		public bool UseTracing
		{
			get { return useTracing; }
			set { useTracing = value; }
		}

		public void ClearAll ()
		{
			acknowledgeType = false;
			acknowledgment = false;
			administrationQueue = false;
			appSpecific = false;
			arrivedTime = false;
			attachSenderId = false;
			authenticated = false;
			authenticationProviderName = false;
			authenticationProviderType = false;
			body = false;
			connectorType = false;
			correlationId = false;
			destinationQueue = false;
			destinationSymmetricKey = false;
			digitalSignature = false;
			encryptionAlgorithm = false;
			extension = false;
			hashAlgorithm = false;
			id = false;
			isFirstInTransaction = false;
			isLastInTransaction = false;
			label = false;
			messageType = false;
			priority = false;
			recoverable = false;
			responseQueue = false;
			senderCertificate = false;
			senderId = false;
			senderVersion = false;
			sentTime = false;
			sourceMachine = false;
			timeToBeReceived = false;
			timeToReachQueue = false;
			transactionId = false;
			transactionStatusQueue = false;
			useAuthentication = false;
			useDeadLetterQueue = false;
			useEncryption = false;
			useJournalQueue = false;
			useTracing = false;
		}

		public void SetAll ()
		{
			acknowledgeType = true;
			acknowledgment = true;
			administrationQueue = true;
			appSpecific = true;
			arrivedTime = true;
			attachSenderId = true;
			authenticated = true;
			authenticationProviderName = true;
			authenticationProviderType = true;
			body = true;
			connectorType = true;
			correlationId = true;
			destinationQueue = true;
			destinationSymmetricKey = true;
			digitalSignature = true;
			encryptionAlgorithm = true;
			extension = true;
			hashAlgorithm = true;
			id = true;
			isFirstInTransaction = true;
			isLastInTransaction = true;
			label = true;
			messageType = true;
			priority = true;
			recoverable = true;
			responseQueue = true;
			senderCertificate = true;
			senderId = true;
			senderVersion = true;
			sentTime = true;
			sourceMachine = true;
			timeToBeReceived = true;
			timeToReachQueue = true;
			transactionId = true;
			transactionStatusQueue = true;
			useAuthentication = true;
			useDeadLetterQueue = true;
			useEncryption = true;
			useJournalQueue = true;
			useTracing = true;
		}

		[MonoTODO]
		public void SetDefaults ()
		{
			acknowledgeType = false;
			acknowledgment = false;
			administrationQueue = true;			//
			appSpecific = false;
			arrivedTime = true; 				//
			attachSenderId = false;
			authenticated = false;
			authenticationProviderName = false;
			authenticationProviderType = false;
			body = true;						//
			connectorType = false;
			correlationId = true;				//
			defaultBodySize = 1024;
			defaultExtensionSize = 255;
			defaultLabelSize = 255;
			destinationQueue = false;
			destinationSymmetricKey = false;
			digitalSignature = false;
			encryptionAlgorithm = false;
			extension = false;
			hashAlgorithm = false;
			id = true;							//
			isFirstInTransaction = false;
			isLastInTransaction = false;
			label = true;						//
			messageType = false;
			priority = false;
			recoverable = false;
			responseQueue = true;				//
			senderCertificate = false;
			senderId = false;
			senderVersion = false;
			sentTime = true;					//
			sourceMachine = false;
			timeToBeReceived = false;
			timeToReachQueue = false;
			transactionId = false;
			transactionStatusQueue = false;
			useAuthentication = false;
			useDeadLetterQueue = false;
			useEncryption = false;
			useJournalQueue = false;
			useTracing = false;
		}
	}
}
