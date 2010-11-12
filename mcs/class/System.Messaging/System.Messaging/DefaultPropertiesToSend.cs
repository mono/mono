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
