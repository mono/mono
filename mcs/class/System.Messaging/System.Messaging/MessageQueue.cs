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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Messaging.Design;

using Mono.Messaging;

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
		private IMessageFormatter formatter;

		private readonly IMessageQueue delegateQueue;

		#endregion //Fields


		#region Constructor

		[MonoTODO]
		public MessageQueue () : this (GetMessageQueue ())
		{
		}

		[MonoTODO]
		public MessageQueue (string path) : this (path, false) 
		{
		}

		[MonoTODO]
		public MessageQueue (string path, bool sharedModeDenyReceive) : 
			this (GetMessageQueue (path))
		{
		}
		
		internal MessageQueue (IMessageQueue delegateQueue)
		{
			this.delegateQueue = delegateQueue;
			formatter = new XmlMessageFormatter ();
		}

		#endregion //Constructor

		#region Properties

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_Authenticate")]
		public bool Authenticate {
			get {
				return delegateQueue.Authenticate;
			}
			set {
				delegateQueue.Authenticate = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_BasePriority")]
		public short BasePriority {
			get {
				return delegateQueue.BasePriority;
			}
			set {
				delegateQueue.BasePriority = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MessagingDescription ("MQ_CanRead")]
		public bool CanRead {
			get {
				return delegateQueue.CanRead;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MessagingDescription ("MQ_CanWrite")]
		public bool CanWrite {
			get {
				return delegateQueue.CanWrite;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_Category")]
		public Guid Category {
			get {
				return delegateQueue.Category;
			}
			set {
				delegateQueue.Category = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_CreateTime")]
		public DateTime CreateTime {
			get {
				return delegateQueue.CreateTime;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Browsable (false)]
		[MessagingDescription ("MQ_DefaultPropertiesToSend")]
		public DefaultPropertiesToSend DefaultPropertiesToSend {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DefaultValue (false)]
		[MessagingDescription ("MQ_DenySharedReceive")]
		public bool DenySharedReceive {
			get {
				return delegateQueue.DenySharedReceive;
			}
			set {
				delegateQueue.DenySharedReceive = value;
			}
		}

		[Browsable (false)]
		public static bool EnableConnectionCache {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_EncryptionRequired")]
		public EncryptionRequired EncryptionRequired {
			get {
				return (EncryptionRequired) delegateQueue.EncryptionRequired;
			}
			set {
				delegateQueue.EncryptionRequired = (Mono.Messaging.EncryptionRequired) value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_FormatName")]
		public string FormatName {
			get {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[TypeConverter (typeof(MessageFormatterConverter))]
		[MessagingDescription ("MQ_Formatter")]
		public IMessageFormatter Formatter {
			get {
				return formatter;
			}
			set {
				formatter = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_GuidId")]
		public Guid Id {
			get {
				return delegateQueue.Id;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_Label")]
		public string Label {
			[MonoTODO]
			get {
				//return delegateQueue.Label;
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				//delegateQueue.Label = value;
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_LastModifyTime")]
		public DateTime LastModifyTime {
			get {
				return delegateQueue.LastModifyTime;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_MachineName")]
		public string MachineName {
			get {
				return delegateQueue.QRef.Host;
			}
			set {
				delegateQueue.QRef = delegateQueue.QRef.SetHost (value);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[TypeConverter (typeof(SizeConverter))]
		[MessagingDescription ("MQ_MaximumJournalSize")]
		public long MaximumJournalSize {
			get {
				return delegateQueue.MaximumJournalSize;
			}
			set {
				delegateQueue.MaximumJournalSize = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[TypeConverter (typeof(SizeConverter))]
		[MessagingDescription ("MQ_MaximumQueueSize")]
		public long MaximumQueueSize {
			get {
				return delegateQueue.MaximumQueueSize;
			}
			set {
				delegateQueue.MaximumQueueSize = value;
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
				return delegateQueue.QRef.ToString ();
			}
			set {
				delegateQueue.QRef = QueueReference.Parse (value);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_QueueName")]
		public string QueueName {
			get {
				return delegateQueue.QRef.Queue;
			}
			set {
				delegateQueue.QRef = delegateQueue.QRef.SetQueue (value);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_ReadHandle")]
		public IntPtr ReadHandle {
			get {
				return delegateQueue.ReadHandle;
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[MessagingDescription ("MQ_SynchronizingObject")]
		public ISynchronizeInvoke SynchronizingObject {
			get {
				return delegateQueue.SynchronizingObject;
			}
			set {
				delegateQueue.SynchronizingObject = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_Transactional")]
		public bool Transactional {
			get {
				return delegateQueue.Transactional;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_WriteHandle")]
		public bool UseJournalQueue {
			get {
				return delegateQueue.UseJournalQueue;
			}
			set {
				delegateQueue.UseJournalQueue = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[MessagingDescription ("MQ_WriteHandle")]
		public IntPtr WriteHandle {
			get {
				return delegateQueue.WriteHandle;
			}
		}
		
		internal IMessageQueue DelegateQueue {
			get {
				return delegateQueue;
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

		public static MessageQueue Create (string path)
		{
			QueueReference qRef = QueueReference.Parse (path);
			IMessageQueue iMessageQueue = CreateMessageQueue (qRef);
			return new MessageQueue (iMessageQueue);
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

		public static bool Exists (string path)
		{
			return Exists (QueueReference.Parse (path));
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

		public MessageEnumerator GetMessageEnumerator ()
		{
			return new MessageEnumerator (delegateQueue.GetMessageEnumerator (), Formatter);
		}

		private static ArrayList currentQueueList;

		static MessageQueue ()
		{
			currentQueueList = new ArrayList ();
			// for testing purposes
			//currentQueueList.Add (new MessageQueue (@"localhost:\public\TestQueue"));
			//currentQueueList.Add (new MessageQueue (@"\private\AnotherTestQueue", "localhost", true));
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
			throw new NotImplementedException ();
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

		public Message Receive ()
		{
			try {
				IMessage iMsg = delegateQueue.Receive ();
				return new Message (iMsg, null, Formatter);
				
			} catch (ConnectionException e) {
				throw new MessageQueueException (MessageQueueErrorCode.QueueNotAvailable, e.Message);
			} catch (MonoMessagingException e) {
				throw new MessageQueueException (MessageQueueErrorCode.Generic, e.Message);
			}
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

		public void Send (object obj)
		{
			if (typeof (Message) == obj.GetType ()) {
				Message m = (Message) obj;
				Console.WriteLine ("BodyStream {0}", m.BodyStream == null);
				if (m.BodyStream == null) {
					IMessageFormatter f = (m.Formatter == null) ? Formatter : m.Formatter;
					f.Write (m, m.Body);
				}

				try {
					delegateQueue.Send (m.DelegateMessage);
				} catch (ConnectionException e) {
					throw new MessageQueueException (MessageQueueErrorCode.QueueNotAvailable, e.Message);
				} catch (MonoMessagingException e) {
					throw new MessageQueueException (MessageQueueErrorCode.Generic, e.Message);
				}
			} else {
				Message m = new Message (obj);
				Send (m);
			}
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

		protected override void Dispose (bool disposing)
		{
			//delegateQueue.Dispose ();
		}

		#endregion //Methods

		//TODO: Use these events.

		[MessagingDescription ("MQ_PeekCompleted")]
		public event PeekCompletedEventHandler PeekCompleted;

		[MessagingDescription ("MQ_ReceiveCompleted")]
		public event ReceiveCompletedEventHandler ReceiveCompleted;
		
		private static IMessageQueue GetMessageQueue (string path)
		{
			IMessageQueue q = MessagingProviderLocator
				.GetProvider ()
				.GetMessageQueue ();
			q.QRef = QueueReference.Parse (path);
			return q;
		}
		
		private static IMessageQueue GetMessageQueue ()
		{
			return MessagingProviderLocator.GetProvider ()
				.GetMessageQueue ();
		}
		
		private static IMessageQueue CreateMessageQueue (QueueReference qRef)
		{
			return MessagingProviderLocator.GetProvider ()
				.CreateMessageQueue (qRef);
		}
		
		private static bool Exists (QueueReference qRef)
		{
			return MessagingProviderLocator.GetProvider ().Exists (qRef);
		}
		
	}
}
