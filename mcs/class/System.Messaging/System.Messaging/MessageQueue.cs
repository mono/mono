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
using System.Collections;
using System.ComponentModel;

namespace System.Messaging 
{
	public class MessageQueue: Component, IEnumerable 
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
		public MessageQueue() 
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public MessageQueue(string path)
		{
			this.Path = path;
			this.isPrivate= false;
		}
		
		[MonoTODO]
		private MessageQueue(string queueName, string machineName, bool isPrivate)
		{
			this.queueName = queueName;
			this.machineName = machineName;
			this.isPrivate= isPrivate;
		}
		
		[MonoTODO]
		public MessageQueue(string path, bool sharedModeDenyReceive)
		{
			throw new NotImplementedException();
		}
		
		#endregion //Constructor
		
		#region Properties
		
		public bool Authenticate {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public short BasePriority {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool CanRead {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public bool CanWrite {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public Guid Category {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public DateTime CreateTime {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public DefaultPropertiesToSend DefaultPropertiesToSend {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool DenySharedReceive {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public static bool EnableConnectionCache {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public EncryptionRequired EncryptionRequired {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public string FormatName {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public IMessageFormatter Formatter {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public Guid Id {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public string Label {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public DateTime LastModifyTime {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public string MachineName {
			get { return machineName; }
			set { machineName = value; }
		}
		public long MaximumJournalSize {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public long MaximumQueueSize {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public MessagePropertyFilter MessageReadPropertyFilter {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public string Path {
			get { return machineName + ":" + queueName; }
			[MonoTODO("split input")]
			set {throw new NotImplementedException();}
		}
		public string QueueName {
			get { return queueName; }
			set { queueName = value; }
		}
		public IntPtr ReadHandle {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public ISynchronizeInvoke SynchronizingObject {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		public bool Transactional {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		public bool UseJournalQueue {
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		#endregion //Properties
		
		#region Methods
		
		public IntPtr WriteHandle {
			[MonoTODO]
			get {throw new NotImplementedException();}
		}
		[MonoTODO]
		public IAsyncResult BeginPeek()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public IAsyncResult BeginPeek(TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public IAsyncResult BeginPeek(TimeSpan timeout, object stateObject)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public IAsyncResult BeginPeek(TimeSpan timeout,
									  object stateObject,
									  AsyncCallback callback)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public IAsyncResult BeginReceive()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public IAsyncResult BeginReceive(TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public IAsyncResult BeginReceive(TimeSpan timeout, object stateObject)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public IAsyncResult BeginReceive(TimeSpan timeout, object stateObject, AsyncCallback callback)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static void ClearConnectionCache()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Close()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static MessageQueue Create(string path)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static MessageQueue Create(string path, bool transactional)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static void Delete(string path)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message EndPeek(IAsyncResult asyncResult)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message EndReceive(IAsyncResult asyncResult)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static bool Exists(string path)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message[] GetAllMessages()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static Guid GetMachiIdneId(string machineName)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public MessageEnumerator GetMessageEnumerator()
		{
			throw new NotImplementedException();
		}
		
		private static ArrayList currentQueueList;
		
		static MessageQueue()
		{
			currentQueueList = new ArrayList();
			// for testing purposes
			currentQueueList.Add(new MessageQueue(@"localhost:\public\TestQueue"));
			currentQueueList.Add(new MessageQueue(@"\private\AnotherTestQueue", "localhost", true));
		}
		
		public static MessageQueueEnumerator GetMessageQueueEnumerator()
		{
			return new MessageQueueEnumerator(currentQueueList);
		}

		private static ArrayList filteredQueueList(MessageQueueCriteria criteria)
		{
			ArrayList list = new ArrayList();
			foreach(MessageQueue queue in currentQueueList)
				if (criteria.Match(queue.Id, queue.CreateTime, queue.Label, queue.MachineName, queue.LastModifyTime))
					list.Add(queue);
			return list;
		}
		
		public static MessageQueueEnumerator GetMessageQueueEnumerator(MessageQueueCriteria criteria)
		{
			return new MessageQueueEnumerator(filteredQueueList(criteria));
		}
		
		public static MessageQueue[] GetPrivateQueuesByMachine(string machineName)
		{
			if (machineName == null || machineName.Length == 0)
				throw new ArgumentException();
			ArrayList list = new ArrayList();
			foreach(MessageQueue queue in currentQueueList)
				if (queue.machineName == machineName && queue.isPrivate)
					list.Add(queue);
			return (MessageQueue[])list.ToArray(typeof(MessageQueue));
		}
		
		[MonoTODO]
		public static MessageQueue[] GetPublicQueues()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static MessageQueue[] GetPublicQueues(MessageQueueCriteria criteria)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static MessageQueue[] GetPublicQueuesByCategory(Guid category)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static MessageQueue[] GetPublicQueuesByLabel(string label)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public static MessageQueue[] GetPublicQueuesByMachine(string machineName)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message Peek()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message Peek(TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message PeekByCorrelationId(string correlationId)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message PeekByCorrelationId(string correlationId, TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message PeekById(string id)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message PeekById(string id, TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Purge()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message Receive()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message Receive(MessageQueueTransaction transaction)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message Receive(MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message Receive(TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message Receive(TimeSpan timeout, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message Receive(TimeSpan timeout, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId(string correlationId)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId(string correlationId, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId(string correlationId, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId(string correlationId, TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId(string correlationId, TimeSpan timeout, MessageQueueTransaction transaction)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveByCorrelationId(string correlationId, TimeSpan timeout, MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveById(string id)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveById(string id,MessageQueueTransaction transaction)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveById(string id,MessageQueueTransactionType transactionType)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public Message ReceiveById(string id,TimeSpan timeout)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Refresh()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void ResetPermissions()
		{
			throw new NotImplementedException();
		}
		
		#endregion //Methods
		
		//TODO: Use these events.
		
		public event PeekCompletedEventHandler PeekCompleted;
		
		public event ReceiveCompletedEventHandler ReceiveCompleted;
		
		[MonoTODO]
		protected override void Dispose(bool disposing)
		{
		}
		~MessageQueue()
		{
		}
	}
}
