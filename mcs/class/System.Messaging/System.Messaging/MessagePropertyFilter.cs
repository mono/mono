//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//
using System;

namespace System.Messaging 
{
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
		public MessagePropertyFilter()
		{
			
		}
		
		public bool AcknowledgeType	
		{
			get {return acknowledgeType;}
			set {acknowledgeType = value;}
		}
		
		public bool Acknowledgment	
		{
			get {return acknowledgment;}
			set {acknowledgment = value;}
		}
		
		public bool AdministrationQueue	
		{
			get {return administrationQueue;}
			set {administrationQueue = value;}
		}
		
		public bool AppSpecific	
		{
			get{return appSpecific;}
			set{appSpecific = value;}
		}
		
		public bool ArrivedTime
		{
			get {return arrivedTime;}
			set {arrivedTime = value;}
		}
		
		public bool AttachSenderId
		{
			get {return attachSenderId;}
			set {attachSenderId = value;}
		}
		
		public bool Authenticated
		{
			get {return authenticated;}
			set {authenticated = value;}
		}
		
		public bool AuthenticationProviderName
		{
			get {return authenticationProviderName;}
			set {authenticationProviderName = value;}
		}
		
		public bool AuthenticationProviderType
		{
			get {return authenticationProviderType;}
			set {authenticationProviderType = value;}
		}
		
		public bool Body
		{
			get {return body;}
			set {body = value;}
		}
		
		public bool ConnectorType
		{
			get {return connectorType;}
			set {connectorType = value;}
		}
		
		public bool CorrelationId
		{
			get {return correlationId;}
			set {correlationId = value;}
		}
		
		public int DefaultBodySize 
		{
			get {return defaultBodySize;} 
			set {
				if (value < 0) throw new ArgumentException("DefaultBodySize");
				defaultBodySize = value;
			}
		}
		
		public int DefaultExtensionSize 
		{
			get {return defaultExtensionSize;}
			set {
				if (value < 0) throw new ArgumentException("DefaultExtensionSize");
				defaultExtensionSize = value;
			}
		}
		
		public int DefaultLabelSize 
		{
			get {return defaultLabelSize;}
			set {
				if (value < 0) throw new ArgumentException("DefaultLabelSize");
				defaultLabelSize = value;
			}
		}
		
		public bool DestinationQueue 
		{
			get {return destinationQueue;}
			set {destinationQueue = value;}
		}
		
		public bool DestinationSymmetricKey 
		{
			get {return destinationSymmetricKey;}
			set {destinationSymmetricKey = value;}
		}
		
		public bool DigitalSignature 
		{
			get {return digitalSignature;}
			set {digitalSignature = value;}
		}
		
		public bool EncryptionAlgorithm 
		{
			get {return encryptionAlgorithm;}
			set {encryptionAlgorithm = value;}
		}
		
		public bool Extension 
		{
			get {return extension;}
			set {extension = value; }
		}
		
		public bool HashAlgorithm 
		{
			get {return hashAlgorithm;}
			set {hashAlgorithm = value;}
		}
		
		public bool Id 
		{
			get {return id;}
			set {id = value;}
		}
		
		public bool IsFirstInTransaction 
		{
			get {return isFirstInTransaction;}
			set {isFirstInTransaction = value;}
		}
		
		public bool IsLastInTransaction 
		{
			get {return isLastInTransaction;}
			set {isLastInTransaction = value; }
		}
		
		public bool Label 
		{
			get {return label; }
			set {label = value;}
		}
		
		public bool MessageType 
		{
			get {return messageType;}
			set {messageType = value;}
		}
		
		public bool Priority 
		{
			get {return priority;}
			set {priority = value;}
		}
		
		public bool Recoverable 
		{
			get {return recoverable;}
			set {recoverable = value;}
		}
		
		public bool ResponseQueue 
		{
			get {return responseQueue;}
			set {responseQueue = value;}
		}
		
		public bool SenderCertificate 
		{
			get {return senderCertificate;}
			set {senderCertificate = value; }
		}
		
		public bool SenderId 
		{
			get {return senderId;}
			set {senderId = value;}
		}
		
		public bool SenderVersion 
		{
			get {return senderVersion;}
			set {senderVersion = value;}
		}
		
		public bool SentTime 
		{
			get {return sentTime;}
			set {sentTime = value;}
		}
		
		public bool SourceMachine 
		{
			get {return sourceMachine;}
			set {sourceMachine = value;}
		}
		
		public bool TimeToBeReceived 
		{
			get {return timeToBeReceived;}
			set {timeToBeReceived = value;}
		}
		
		public bool TimeToReachQueue 
		{
			get {return timeToReachQueue;}
			set {timeToReachQueue = value;}
		}
		
		public bool TransactionId 
		{
			get {return transactionId;} 
			set {transactionId = value;}
		}
		
		public bool TransactionStatusQueue 
		{
			get {return transactionStatusQueue;}
			set {transactionStatusQueue = value;}
		}
		
		public bool UseAuthentication 
		{
			get {return useAuthentication;}
			set {useAuthentication = value;}
		}
		
		public bool UseDeadLetterQueue 
		{
			get {return useDeadLetterQueue;}
			set {useDeadLetterQueue = value;}
		}
		
		public bool UseEncryption 
		{
			get {return useEncryption;}
			set {useEncryption = value; }
		}
		
		public bool UseJournalQueue 
		{
			get {return useJournalQueue;}
			set {useJournalQueue = value;}
		}
		
		public bool UseTracing 
		{
			get {return useTracing;}
			set {useTracing = value;}
		}
		
		public void ClearAll() {
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
		
		public void SetAll() 
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
		public void SetDefaults() 
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
		
		[MonoTODO]
		~MessagePropertyFilter()
		{
		}
	}
}
