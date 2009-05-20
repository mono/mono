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

	public interface IMessage {
	
		AcknowledgeTypes AcknowledgeType { 
			get; set; 
		}

		Acknowledgment Acknowledgment { 
			get;
		}
		
		IMessageQueue AdministrationQueue { 
			get; set;
		}
		
		int AppSpecific { 
			get; set;
		}
		
		DateTime ArrivedTime { 
			get;
		}
		
		bool AttachSenderId { 
			get; set;
		}
		
		bool Authenticated {
			get;
		}
		
		string AuthenticationProviderName {
			get; set;
		}
		
		CryptographicProviderType AuthenticationProviderType {
			get; set;
		}

		Stream BodyStream {
			get; set;
		}

		int BodyType {
			get; set;
		}

		Guid ConnectorType {
			get; set;
		}

		string CorrelationId {
			get; set;
		}

		IMessageQueue DestinationQueue {
			get;
		}

		byte[] DestinationSymmetricKey {
			get; set;
		}

		byte[] DigitalSignature {
			get; set;
		}

		EncryptionAlgorithm EncryptionAlgorithm {
			get; set;
		}

		byte[] Extension {
			get; set;
		}

		HashAlgorithm HashAlgorithm {
			get; set;
		}

		string Id {
			get;
		}

		bool IsFirstInTransaction {
			get;
		}

		bool IsLastInTransaction {
			get;
		}

		string Label {
			get;
			set;
		}

		MessageType MessageType {
			get;
		}

		MessagePriority Priority {
			get; set;
		}

		bool Recoverable {
			get; set;
		}

		IMessageQueue ResponseQueue {
			get; set;
		}

		byte[] SenderCertificate {
			get; set;
		}

		byte[] SenderId {
			get;
		}

		long SenderVersion {
			get;
		}

		DateTime SentTime {
			get;
		}

		string SourceMachine {
			get;
		}

		TimeSpan TimeToBeReceived {
			get; set;
		}

		TimeSpan TimeToReachQueue {
			get; set;
		}

		string TransactionId {
			get;
		}

		IMessageQueue TransactionStatusQueue {
			get; set;
		}

		bool UseAuthentication {
			get; set;
		}

		bool UseDeadLetterQueue {
			get; set;
		}

		bool UseEncryption {
			get; set;
		}

		bool UseJournalQueue {
			get; set;
		}

		bool UseTracing {
			get; set;
		}
		
		/// <summary>
		/// Sets all of the information about a message after is has been
		/// delivered.  Implementing classes should set the values of the
		/// appropriate properties in this method call.
		/// </summary>
		void SetDeliveryInfo (Acknowledgment acknowledgment,
                              DateTime arrivedTime,
                              IMessageQueue destinationQueue,
							  string id,
                              MessageType messageType,
                              byte[] senderId,
							  long senderVersion,
		                      DateTime sentTime,
							  string sourceMachine,
                              string transactionId);
	}
}
