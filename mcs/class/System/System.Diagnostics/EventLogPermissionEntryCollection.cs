//
// System.Diagnostics.EventLogPermissionEntryCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Jonathan Pryor
// (C) 2003 Andreas Nahr
//


using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Permissions;

namespace System.Diagnostics 
{

	[Serializable]
	public class EventLogPermissionEntryCollection : CollectionBase 
	{

		private EventLogPermissionEntryCollection()
		{
		}

		internal EventLogPermissionEntryCollection (ResourcePermissionBaseEntry[] entries)
		{
			foreach (ResourcePermissionBaseEntry entry in entries) {
				List.Add (new EventLogPermissionEntry ((EventLogPermissionAccess) entry.PermissionAccess, entry.PermissionAccessPath[0]));
			}	
		}

		public EventLogPermissionEntry this [int index] {
			get { return ((EventLogPermissionEntry) List[index]); }
			set { List[index] = value; }
		}

		public int Add(EventLogPermissionEntry value)
		{
			return List.Add (value);
		}

		public void AddRange(EventLogPermissionEntry[] value)
		{
			foreach (EventLogPermissionEntry entry in value)
				List.Add (entry);
		}

		public void AddRange(EventLogPermissionEntryCollection value)
		{
			foreach (EventLogPermissionEntry entry in value)
				List.Add (entry);
		}

		public bool Contains(EventLogPermissionEntry value)
		{
			return List.Contains (value);
		}

		public void CopyTo(EventLogPermissionEntry[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf(EventLogPermissionEntry value)
		{
			return List.IndexOf (value);
		}

		public void Insert(int index, EventLogPermissionEntry value)
		{
			List.Insert (index, value);
		}

		[MonoTODO]
		protected override void OnClear()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnInsert(int index, object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnRemove(int index, object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnSet(int index, object oldValue, 
			object newValue)
		{
			throw new NotImplementedException();
		}

		public void Remove(EventLogPermissionEntry value)
		{
			List.Remove (value);
		}
	}
}

