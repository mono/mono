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
		private MessageQueuePermission owner;

		internal MessageQueuePermissionEntryCollection (MessageQueuePermission owner)
		{
			this.owner = owner;
		}

		public MessageQueuePermissionEntry this[int index] 
		{
			get
			{
				return ((MessageQueuePermissionEntry) base.List[index]);
			}
			set
			{
				base.List[index] = value;
			}
		}
		
		public int Add(MessageQueuePermissionEntry value)
		{
			return base.List.Add (value);
		}
		
		public void AddRange(MessageQueuePermissionEntry[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException ("value");

			}
			for (int counter = 0; counter < value.Length; counter++)
			{
				this.Add (value[counter]);
			}
		}
		
		public void AddRange(MessageQueuePermissionEntryCollection value)
		{
			if (value == null)
			{
				throw new ArgumentNullException ("value");

			}
			int entryCount = value.Count;
			for (int counter = 0; counter < entryCount; counter++)
			{
				this.Add (value[counter]);
			}
		}
		
		public bool Contains(MessageQueuePermissionEntry value)
		{
			return base.List.Contains (value);
		}
		
		public void CopyTo(MessageQueuePermissionEntry[] array,int index)
		{
			base.List.CopyTo (array, index);
		}
		
		[MonoTODO]
		public int IndexOf(MessageQueuePermissionEntry value)
		{
			return base.List.IndexOf (value);
		}
		
		public void Insert(int index, MessageQueuePermissionEntry value)
		{
			base.List.Insert (index, value);
		}
		
		public void Remove(MessageQueuePermissionEntry value)
		{
			base.List.Remove (value);
		}
		
		protected override void OnClear()
		{
			owner.Clear ();
		}
		
		protected override void OnInsert(int index,object value)
		{
			owner.Clear ();
		}
		
		protected override void OnRemove(int index,object value)
		{
			owner.Clear ();
		}
		
		protected override void OnSet(int index,object oldValue,object newValue)
		{
			owner.Clear ();
		}
	}
}
