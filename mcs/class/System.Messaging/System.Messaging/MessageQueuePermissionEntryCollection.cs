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
		
		[MonoTODO]
		protected override void OnClear()
		{
		}
		
		[MonoTODO]
		protected override void OnInsert(int index,object value)
		{
		}
		
		[MonoTODO]
		protected override void OnRemove(int index,object value)
		{
		}
		
		[MonoTODO]
		protected override void OnSet(int index,object oldValue,object newValue)
		{
		}
	}
}
