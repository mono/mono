//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
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

namespace System.Messaging 
{
	[DesignerAttribute ("System.Messaging.Design.MessageDesigner, " + Consts.AssemblySystem_Design)]
	public class Message: Component 
	{
		#region Constructor
		
		[MonoTODO]
		public Message()
		{
		}

		[MonoTODO]
		public Message (object body)
		{
		}

		[MonoTODO]
		public Message (object body, IMessageFormatter formatter)
		{
		}
		
		#endregion //Constructor
		
		[MonoTODO]
		public static readonly TimeSpan InfiniteTimeout;
		
		#region Properties

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAcknowledgeType")]
		public AcknowledgeTypes AcknowledgeType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAcknowledgement")]
		public Acknowledgment Acknowledgment {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAdministrationQueue")]
		public MessageQueue AdministrationQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAppSpecific")]
		public int AppSpecific {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgArrivedTime")]
		public DateTime ArrivedTime {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAttachSenderId")]
		public bool AttachSenderId {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgAuthenticated")]
		public bool Authenticated {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAuthenticationProviderName")]
		public string AuthenticationProviderName {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgAuthenticationProviderType")]
		public CryptographicProviderType AuthenticationProviderType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public object Body {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[Editor ("System.ComponentModel.Design.BinaryEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MessagingDescription ("MsgBodyStream")]
		public Stream BodyStream {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgBodyType")]
		[ReadOnly (true)]
		public int BodyType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgConnectorType")]
		public Guid ConnectorType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgCorrelationId")]
		public string CorrelationId {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MessagingDescription("MsgDestinationQueue")]
		public MessageQueue DestinationQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgDestinationSymmetricKey")]
		public byte[] DestinationSymmetricKey {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgDigitalSignature")]
		public byte[] DigitalSignature {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgEncryptionAlgorithm")]
		public EncryptionAlgorithm EncryptionAlgorithm {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgExtension")]
		public byte[] Extension {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public IMessageFormatter Formatter {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgHashAlgorithm")]
		public HashAlgorithm HashAlgorithm {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgId")]
		public string Id {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MessagingDescription("MsgIsFirstInTransaction")]
		public bool IsFirstInTransaction {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MessagingDescription("MsgIsLastInTransaction")]
		public bool IsLastInTransaction {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgLabel")]
		public string Label {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MessagingDescription("MsgMessageType")]
		public MessageType MessageType {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgPriority")]
		public MessagePriority Priority {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgRecoverable")]
		public bool Recoverable {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgResponseQueue")]
		public MessageQueue ResponseQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[MessagingDescription("MsgSenderCertificate")]
		public byte[] SenderCertificate {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgSenderId")]
		public byte[] SenderId {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgSenderVersion")]
		public long SenderVersion {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgSentTime")]
		public DateTime SentTime {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgSourceMachine")]
		public string SourceMachine {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgTimeToBeReceived")]
		[TypeConverter (typeof(TimeoutConverter))]
		public TimeSpan TimeToBeReceived {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgTimeToReachQueue")]
		[TypeConverter (typeof(TimeoutConverter))]
		public TimeSpan TimeToReachQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MsgTransactionId")]
		public string TransactionId {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgTransactionStatusQueue")]
		public MessageQueue TransactionStatusQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseAuthentication")]
		public bool UseAuthentication {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseDeadLetterQueue")]
		public bool UseDeadLetterQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseEncryption")]
		public bool UseEncryption {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseJournalQueue")]
		public bool UseJournalQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[ReadOnly (true)]
		[MessagingDescription ("MsgUseTracing")]
		public bool UseTracing {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		#endregion //Properties
	}
}
