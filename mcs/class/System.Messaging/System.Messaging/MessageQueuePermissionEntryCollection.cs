//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//
// (C) 2003 Peter Van Isacker
//
using System;
using System.Collections;

namespace System.Messaging 
{
	[Serializable]
	public class MessageQueuePermissionEntryCollection: CollectionBase 
	{
		//[Serializable]
		public MessageQueuePermissionEntry this[int index] 
		{
			[MonoTODO]
			get {throw new NotImplementedException();}
			[MonoTODO]
			set {throw new NotImplementedException();}
		}
		
		[MonoTODO]
		public int Add(MessageQueuePermissionEntry value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void AddRange(MessageQueuePermissionEntry[] value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void AddRange(MessageQueuePermissionEntryCollection value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public bool Contains(MessageQueuePermissionEntry value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void CopyTo(MessageQueuePermissionEntry[] array,int index)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public int IndexOf(MessageQueuePermissionEntry value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void Insert(int index, MessageQueuePermissionEntry value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void Remove(MessageQueuePermissionEntry value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		protected override void OnClear()
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		protected override void OnInsert(int index,object value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		protected override void OnRemove(int index,object value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		protected override void OnSet(int index,object oldValue,object newValue)
		{
			throw new NotImplementedException();
		}
	}
}
