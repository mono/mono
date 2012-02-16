//
// Mono.Messaging
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
using System.IO;

namespace Mono.Messaging {

	public class MessageBase : IMessage {
	
		AcknowledgeTypes acknowledgeType;
		Acknowledgment acknowledgment;
		IMessageQueue administrationQueue = null;
		int appSpecific;
		DateTime arrivedTime;
		bool attachSenderId = true;
		const bool authenticated = false;
		string authenticationProviderName = "Microsoft Base Cryptographic Provider, Ver. 1.0";
		CryptographicProviderType authenticationProviderType 
			= CryptographicProviderType.RsaFull;
		Stream bodyStream;
		int bodyType;
		Guid connectorType = Guid.Empty;
		string correlationId;
		IMessageQueue destinationQueue;
		byte[] destinationSymmetricKey = new byte[0];
		byte[] digitalSignature = new byte[0];
		EncryptionAlgorithm encryptionAlgorithm 
			= EncryptionAlgorithm.Rc2;
		byte[] extension = new byte[0];
		HashAlgorithm hashAlgorithm = HashAlgorithm.Sha;
		string id = Guid.Empty.ToString () + "\\0";
		bool isFirstInTransaction = false;
		bool isLastInTransaction = false;
		string label = "";
		MessageType messageType;
		MessagePriority priority = MessagePriority.Normal;
		bool recoverable = false;
		IMessageQueue responseQueue;
		byte[] senderCertificate = new byte[0];
		byte[] senderId = new byte[0];
		long senderVersion;
		DateTime sentTime;
		string sourceMachine;
		TimeSpan timeToBeReceived;
		TimeSpan timeToReachQueue;
		string transactionId = "";
		IMessageQueue transactionStatusQueue;
		bool useAuthentication;
		bool useDeadLetterQueue;
		bool useEncryption;
		bool useJournalQueue;
		bool useTracing;
		bool isDelivered;
		
		public AcknowledgeTypes AcknowledgeType { 
			get { return acknowledgeType; }
			set { acknowledgeType = value; }
		}

		public Acknowledgment Acknowledgment { 
			get { 
				CheckDelivered ();
				return acknowledgment;
			}
		}
		
		public IMessageQueue AdministrationQueue { 
			get { return administrationQueue; }
			set { administrationQueue = value; }
		}
		
		public int AppSpecific { 
			get { return appSpecific; }
			set { appSpecific = value; }
		}
		
		public DateTime ArrivedTime { 
			get { 
				CheckDelivered ();
				return arrivedTime;
			}
		}
		
		public bool AttachSenderId { 
			get { return attachSenderId; }
			set { attachSenderId = value; }
		}
		
		public bool Authenticated {
			get { 
				CheckDelivered ();
				return authenticated;
			}
		}
		
		public string AuthenticationProviderName {
			get { return authenticationProviderName; }
			set { authenticationProviderName = value; }
		}
		
		public CryptographicProviderType AuthenticationProviderType {
			get { return authenticationProviderType; }
			set { authenticationProviderType = value; }
		}

		public Stream BodyStream {
			get { return bodyStream; }
			set { bodyStream = value; }
		}

		public int BodyType {
			get { return bodyType; }
			set { bodyType = value; }
		}

		public Guid ConnectorType {
			get { return connectorType; }
			set { connectorType = value; }
		}

		public string CorrelationId {
			get { return correlationId; }
			set { correlationId = value; }
		}

		public IMessageQueue DestinationQueue {
			get { 
				CheckDelivered ();
				return destinationQueue;
			}
		}

		public byte[] DestinationSymmetricKey {
			get { return destinationSymmetricKey; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("DestinationSymmetricKey can not be null");
				destinationSymmetricKey = value;
			}
		}

		public byte[] DigitalSignature {
			get { return digitalSignature; }
			set {
				if (value == null)
					throw new ArgumentNullException ("DigitalSignature can not be null");
				digitalSignature = value;
			}
		}

		public EncryptionAlgorithm EncryptionAlgorithm {
			get { return encryptionAlgorithm; }
			set { encryptionAlgorithm = value; }
		}

		public byte[] Extension {
			get { return extension; }
			set {
				if (value == null)
					throw new ArgumentNullException ("Extension can not be null");
				extension = value;
			}
		}

		public HashAlgorithm HashAlgorithm {
			get { return hashAlgorithm; }
			set { hashAlgorithm = value; }
		}

		public string Id {
			get { 
				//CheckDelivered ();
				return id;
			}
		}

		public bool IsFirstInTransaction {
			get { 
				//CheckDelivered ();
				return isFirstInTransaction;
			}
		}

		public bool IsLastInTransaction {
			get { 
				//CheckDelivered ();
				return isLastInTransaction;
			}
		}

		public string Label {
			get { return label; }
			set { label =  value; }
		}

		public MessageType MessageType {
			get { 
				CheckDelivered ();
				return messageType;
			}
		}

		public MessagePriority Priority {
			get { return priority; }
			set { priority = value; }
		}

		public bool Recoverable {
			get { return recoverable; }
			set { recoverable = value; }
		}

		public IMessageQueue ResponseQueue {
			get { return responseQueue; }
			set { responseQueue = value; }
		}

		public byte[] SenderCertificate {
			get { return senderCertificate; }
			set { senderCertificate = value; }
		}

		public byte[] SenderId {
			get { 
				CheckDelivered ();
				return senderId;
			}
		}

		public long SenderVersion {
			get { 
				CheckDelivered ();
				return senderVersion;
			}
		}

		public DateTime SentTime {
			get {  
				CheckDelivered ();
				return sentTime;
			}
		}

		public string SourceMachine {
			get {  
				CheckDelivered ();
				return sourceMachine;
			}
		}

		public TimeSpan TimeToBeReceived {
			get { return timeToBeReceived; }
			set { timeToBeReceived = value; }
		}

		public TimeSpan TimeToReachQueue {
			get { return timeToReachQueue; }
			set { timeToReachQueue = value; }
		}

		public string TransactionId {
			get {  
				//CheckDelivered ();
				return transactionId;
			}
		}

		public IMessageQueue TransactionStatusQueue {
			get { return transactionStatusQueue; }
			set { transactionStatusQueue = value; }
		}

		public bool UseAuthentication {
			get { return useAuthentication; }
			set { useAuthentication = value; }
		}

		public bool UseDeadLetterQueue {
			get { return useDeadLetterQueue; }
			set { useDeadLetterQueue = value; }
		}

		public bool UseEncryption {
			get { return useEncryption; }
			set { useEncryption = value; }
		}

		public bool UseJournalQueue {
			get { return useJournalQueue; }
			set { useJournalQueue = value; }
		}

		public bool UseTracing {
			get { return useTracing; }
			set { useTracing = value; }
		}
		
		private void CheckDelivered ()
		{
			if (!isDelivered)
				throw new InvalidOperationException ("Message has not been delivered");
		}
		
		public void SetDeliveryInfo (Acknowledgment acknowledgment,
		                             DateTime arrivedTime,
		                             IMessageQueue destinationQueue,
									 string id,
		                             MessageType messageType,
		                             byte[] senderId,
									 long senderVersion,
		                             DateTime sentTime,
									 string sourceMachine,
		                             string transactionId)
		{
			this.acknowledgment = acknowledgment;
			this.arrivedTime = arrivedTime;
			this.destinationQueue = destinationQueue;
			this.id = id;
			this.messageType = messageType;
			this.senderId = senderId;
			this.senderVersion = senderVersion;
			this.sentTime = sentTime;
			this.sourceMachine = sourceMachine;
			this.transactionId = transactionId;
			this.isDelivered = true;
		}
	}
}
