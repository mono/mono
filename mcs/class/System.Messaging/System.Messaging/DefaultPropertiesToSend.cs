//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Messaging 
{
	[TypeConverter (typeof(ExpandableObjectConverter))]
	public class DefaultPropertiesToSend 
	{
		[MonoTODO]
		public DefaultPropertiesToSend()
		{
		}

		[DefaultValue (AcknowledgeTypes.None)]
		[MessagingDescription ("MsgAcknowledgeType")]
		public AcknowledgeTypes AcknowledgeType {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (null)]
		[MessagingDescription ("MsgAdministrationQueue")]
		public MessageQueue AdministrationQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (0)]
		[MessagingDescription ("MsgAppSpecific")]
		public int AppSpecific {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (true)]
		[MessagingDescription ("MsgAttachSenderId")]
		public bool AttachSenderId {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (EncryptionAlgorithm.Rc2)]
		[MessagingDescription ("MsgEncryptionAlgorithm")]
		public EncryptionAlgorithm EncryptionAlgorithm {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[Editor ("System.ComponentModel.Design.ArrayEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MessagingDescription ("MsgExtension")]
		public byte[] Extension {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (HashAlgorithm.Md5)]
		[MessagingDescription ("MsgHashAlgorithm")]
		public HashAlgorithm HashAlgorithm {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue ("")]
		[MessagingDescription ("MsgLabel")]
		public string Label {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (MessagePriority.Normal)]
		[MessagingDescription ("MsgPriority")]
		public MessagePriority Priority {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgRecoverable")]
		public bool Recoverable {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (null)]
		[MessagingDescription ("MsgResponseQueue")]
		public MessageQueue ResponseQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[TypeConverter (typeof(TimeoutConverter))]
		[MessagingDescription ("MsgTimeToBeReceived")]
		public TimeSpan TimeToBeReceived {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[TypeConverter (typeof(TimeoutConverter))]
		[MessagingDescription ("MsgTimeToReachQueue")]
		public TimeSpan TimeToReachQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (null)]
		[MessagingDescription ("MsgTransactionStatusQueue")]
		public MessageQueue TransactionStatusQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgUseAuthentication")]
		public bool UseAuthentication {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgUseDeadLetterQueue")]
		public bool UseDeadLetterQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgUseEncryption")]
		public bool UseEncryption {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgUseJournalQueue")]
		public bool UseJournalQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}

		[DefaultValue (false)]
		[MessagingDescription ("MsgUseTracing")]
		public bool UseTracing {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
	}
}
