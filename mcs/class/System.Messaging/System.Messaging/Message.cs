//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
//
using System;
using System.IO;
using System.ComponentModel;

namespace System.Messaging 
{
	public class Message: Component 
	{
		
		#region Constructor
		
		[MonoTODO]
		public Message()
		{
		}
		
		//public Message(object body);
		//public Message(object body, IMessageFormatter formatter);

		
		#endregion //Constructor
		
		[MonoTODO]
		public static readonly TimeSpan InfiniteTimeout;
		
		
		#region Properties
		
		public AcknowledgeTypes AcknowledgeType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public Acknowledgment Acknowledgment {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public MessageQueue AdministrationQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public int AppSpecific {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public DateTime ArrivedTime {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public bool AttachSenderId {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool Authenticated {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public string AuthenticationProviderName {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public CryptographicProviderType AuthenticationProviderType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public object Body {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public Stream BodyStream {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public int BodyType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public Guid ConnectorType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public string CorrelationId {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public MessageQueue DestinationQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public byte[] DestinationSymmetricKey {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public byte[] DigitalSignature {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public EncryptionAlgorithm EncryptionAlgorithm {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public byte[] Extension {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public IMessageFormatter Formatter {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public HashAlgorithm HashAlgorithm {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public string Id {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public bool IsFirstInTransaction {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public bool IsLastInTransaction {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public string Label {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public MessageType MessageType {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public MessagePriority Priority {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool Recoverable {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public MessageQueue ResponseQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public byte[] SenderCertificate {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public byte[] SenderId {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public long SenderVersion {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public DateTime SentTime {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public string SourceMachine {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public TimeSpan TimeToBeReceived {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public TimeSpan TimeToReachQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public string TransactionId {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public MessageQueue TransactionStatusQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool UseAuthentication {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool UseDeadLetterQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool UseEncryption {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool UseJournalQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool UseTracing {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		#endregion //Properties
		
		
		[MonoTODO]
		~Message()
		{
		}
	}
}
