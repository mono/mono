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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Messaging.Design;

namespace System.Messaging
{
	[TypeConverter (typeof(MessageQueueConverter))]
	[Editor ("System.Messaging.Design.QueuePathEditor", "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	[Designer ("Microsoft.VisualStudio.Install.MessageQueueInstallableComponentDesigner, " + Consts.AssemblyMicrosoft_VisualStudio)]
	[InstallerType (typeof(MessageQueueInstaller))]
	[DefaultEvent ("ReceiveCompleted")]
	public class MessageQueue : Component, IEnumerable
	{
		#region Fields

		public static readonly long InfiniteQueueSize;
		public static readonly TimeSpan InfiniteTimeout;

		private bool isPrivate = false;
		private string queueName;
		private string machineName;


		#endregion //Fields


		#region Constructor

		[MonoTODO]
		public MessageQueue ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MessageQueue (string path) {
			this.Path = path;
			this.isPrivate = false;
		}

		[MonoTODO]
		private MessageQueue (string queueName, string machineName, bool isPrivate) 
		{
			this.queueName = queueName;
			this.machineName = machineName;
			this.isPrivate = isPrivate;
		}

		[MonoTODO]
		public MessageQueue (string path, bool sharedModeDenyReceive) 
		{
			throw new NotImplementedException ();
		}

		#endregion //Constructor

		#region Properties

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_Authenticate")]
		public bool Authenticate {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_BasePriority")]
		public short BasePriority {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MessagingDescription ("MQ_CanRead")]
		public bool CanRead {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MessagingDescription ("MQ_CanWrite")]
		public bool CanWrite {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_Category")]
		public Guid Category {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_CreateTime")]
		public DateTime CreateTime {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Browsable (false)]
		[MessagingDescription ("MQ_DefaultPropertiesToSend")]
		public DefaultPropertiesToSend DefaultPropertiesToSend {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DefaultValue (false)]
		[MessagingDescription ("MQ_DenySharedReceive")]
		public bool DenySharedReceive {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		public static bool EnableConnectionCache {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_EncryptionRequired")]
		public EncryptionRequired EncryptionRequired {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_FormatName")]
		public string FormatName {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[TypeConverter (typeof(MessageFormatterConverter))]
		[MessagingDescription ("MQ_Formatter")]
		public IMessageFormatter Formatter {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_GuidId")]
		public Guid Id {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_Label")]
		public string Label {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_LastModifyTime")]
		public DateTime LastModifyTime {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_MachineName")]
		public string MachineName {
			get {
				return machineName;
			}
			set {
				machineName = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[TypeConverter (typeof(SizeConverter))]
		[MessagingDescription ("MQ_MaximumJournalSize")]
		public long MaximumJournalSize {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[TypeConverter (typeof(SizeConverter))]
		[MessagingDescription ("MQ_MaximumQueueSize")]
		public long MaximumQueueSize {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[MessagingDescription ("MQ_MessageReadPropertyFilter")]
		public MessagePropertyFilter MessageReadPropertyFilter {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[RecommendedAsConfigurable (true)]
		[Editor ("System.Messaging.Design.QueuePathEditor", "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[Browsable (false)]
		[DefaultValue ("")]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[MessagingDescription ("MQ_Path")]
		public string Path {
			get {
				return machineName + ":" + queueName;
			}
			[MonoTODO ("split input")]
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_QueueName")]
		public string QueueName {
			get {
				return queueName;
			}
			set {
				queueName = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_ReadHandle")]
		public IntPtr ReadHandle {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[MessagingDescription ("MQ_SynchronizingObject")]
		public ISynchronizeInvoke SynchronizingObject {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_Transactional")]
		public bool Transactional {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_WriteHandle")]
		public bool UseJournalQueue {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_WriteHandle")]
		public IntPtr WriteHandle {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion //Properties

		#region Methods

		[MonoTODO]
		public IAsyncResult BeginPeek ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IAsyncResult BeginPeek (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IAsyncResult BeginPeek (TimeSpan timeout, object stateObject)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IAsyncResult BeginPeek (TimeSpan timeout,
									  object stateObject,
									  AsyncCallback callback)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IAsyncResult BeginReceive ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IAsyncResult BeginReceive (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IAsyncResult BeginReceive (TimeSpan timeout, object stateObject)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IAsyncResult BeginReceive (TimeSpan timeout, object stateObject, AsyncCallback callback)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static void ClearConnectionCache ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Close ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static MessageQueue Create (string path)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static MessageQueue Create (string path, bool transactional)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static void Delete (string path)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message EndPeek (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message EndReceive (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static bool Exists (string path)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message[] GetAllMessages ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static Guid GetMachineId (string machineName)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public MessageEnumerator GetMessageEnumerator ()
		{
			throw new NotImplementedException ();
		}

		private static ArrayList currentQueueList;

		static MessageQueue ()
		{
			currentQueueList = new ArrayList ();
			// for testing purposes
			currentQueueList.Add (new MessageQueue (@"localhost:\public\TestQueue"));
			currentQueueList.Add (new MessageQueue (@"\private\AnotherTestQueue", "localhost", true));
		}

		public static MessageQueueEnumerator GetMessageQueueEnumerator ()
		{
			return new MessageQueueEnumerator (currentQueueList);
		}

		private static ArrayList filteredQueueList (MessageQueueCriteria criteria)
		{
			ArrayList list = new ArrayList ();
			foreach (MessageQueue queue in currentQueueList)
				if (criteria.Match (queue.Id, queue.CreateTime, queue.Label, queue.MachineName, queue.LastModifyTime))
					list.Add (queue);
			return list;
		}

		public static MessageQueueEnumerator GetMessageQueueEnumerator (MessageQueueCriteria criteria)
		{
			return new MessageQueueEnumerator (filteredQueueList (criteria));
		}

		public static MessageQueue[] GetPrivateQueuesByMachine (string machineName)
		{
			if (machineName == null || machineName.Length == 0)
				throw new ArgumentException ();
			ArrayList list = new ArrayList ();
			foreach (MessageQueue queue in currentQueueList)
				if (queue.machineName == machineName && queue.isPrivate)
					list.Add (queue);
			return (MessageQueue[]) list.ToArray (typeof(MessageQueue));
		}

		[MonoTODO]
		public static MessageQueue[] GetPublicQueues ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static MessageQueue[] GetPublicQueues (MessageQueueCriteria criteria)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static MessageQueue[] GetPublicQueuesByCategory (Guid category)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static MessageQueue[] GetPublicQueuesByLabel (string label)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static MessageQueue[] GetPublicQueuesByMachine (string machineName)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message Peek ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message Peek (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message PeekByCorrelationId (string correlationId)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message PeekByCorrelationId (string correlationId, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message PeekById (string id)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message PeekById (string id, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Purge ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message Receive ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message Receive (MessageQueueTransaction transaction)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message Receive (MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message Receive (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message Receive (TimeSpan timeout, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message Receive (TimeSpan timeout, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId (string correlationId)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId (string correlationId, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId (string correlationId, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId (string correlationId, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId (string correlationId, TimeSpan timeout, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId (string correlationId, TimeSpan timeout, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveById (string id)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveById (string id, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveById (string id, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveById (string id, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveById (string id, TimeSpan timeout, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Message ReceiveById (string id, TimeSpan timeout, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Refresh ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ResetPermissions ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Send (object obj)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Send (object obj, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Send (object obj, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Send (object obj, string label)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Send (object obj, string label, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Send (object obj, string label, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetPermissions (AccessControlList dacl)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetPermissions (MessageQueueAccessControlEntry ace)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetPermissions (string user, MessageQueueAccessRights rights)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetPermissions (string user, MessageQueueAccessRights rights, AccessControlEntryType entryType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		#endregion //Methods

		//TODO: Use these events.

		[MessagingDescription ("MQ_PeekCompleted")]
		public event PeekCompletedEventHandler PeekCompleted;

		[MessagingDescription ("MQ_ReceiveCompleted")]
		public event ReceiveCompletedEventHandler ReceiveCompleted;
	}
}
