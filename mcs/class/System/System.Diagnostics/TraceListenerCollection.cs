//
// System.Diagnostics.TraceListenerCollection.cs
//
// Author: John R. Hicks <angryjohn69@nc.rr.com>
//
// (C) 2001
//
using System;
using System.Collections;

namespace System.Diagnostics
{
	
	/// <summary>
	/// Provides a list of TraceListener objects.
	/// </summary>
	public class TraceListenerCollection : IList, ICollection,
		IEnumerable
	{
		private int count;
		private bool isReadOnly;
		private bool isFixedSize;
		private bool isSynchronized;
		private ArrayList listeners;
		
		/// <summary>
		/// Gets the first TraceListener in the list with the
		/// specified name.
		/// </summary>
		public TraceListener this[string name]
		{
			get
			{
				int index = listeners.IndexOf(name);
				return (TraceListener)listeners[index];
			}
		}
		
		public object this[int index]
		{
			get
			{
				return listeners[index];
			}
			set
			{
				listeners[index] = value;
			}
		}
		
		internal TraceListenerCollection()
		{
			count = 0;
			isReadOnly = false;
			isFixedSize = false;
			isSynchronized = false;
			listeners = new ArrayList();
			listeners.Add(new DefaultTraceListener());
		}
		
		/// <summary>
		/// Returns the number of items in the list
		/// </summary>
		/// <value>
		/// The number of items
		/// </value>
		public int Count
		{
			get
			{
				return count;
			}
			set
			{
				count = value;
			}
		}
		
		/// <summary>
		/// Adds a TraceListener to the list.
		/// </summary>
		/// <param name="listener">
		/// The TraceListener being added to the list.
		/// </param>
		/// <return>
		/// The position in the list where the listener was inserted.
		/// </return>
		public int Add(object listener)
		{
			return listeners.Add(listener);
		}
		
		/// <summary>
		/// Adds an array of TraceListeners to the list.
		/// </summary>
		/// <param name="value">
		/// Array of TraceListeners to add
		/// </param>
		public void AddRange(TraceListener[] value)
		{
			listeners.AddRange(value);
		}
		
		/// <summary>
		/// Adds the contents of another TraceListenerCollection to this one.
		/// </summary>
		/// <param name="value">
		/// The TraceListenerCollection to copy values from.
		/// </param>
		[MonoTODO]
		public void AddRange(TraceListenerCollection value)
		{
			// TODO: use an iterator to copy the objects.
			for(int i = 0; i < value.count; i++)
			{
				listeners.Add(value[i]);
			}
		}
		
		/// <summary>
		/// Clears all listeners from the list.
		/// </summary>
		public void Clear()
		{
			listeners.Clear();
		}
		
		/// <summary>
		/// Checks to see if the list contains the specified listener
		/// </summary>
		/// <param name="listener">
		/// The listener to search for.
		/// </param>
		/// <return>
		/// true if list contains listener; false otherwise.
		/// </return>
		public bool Contains(object listener)
		{
			return listeners.Contains(listener);
		}
		
		/// <summary>
		/// Copies a section of the current TraceListenerCollection to
		/// the specified array at the specified index.
		/// </summary>
		/// <param name="listeners">
		/// Array to copy listeners to.
		/// </param>
		/// <param name="index">
		/// Starting index of copy
		/// </param>
		[MonoTODO]
		public void CopyTo(Array listeners, int index)
		{
			try {
				this.listeners.CopyTo(listeners, index);
			} catch {
				
			}
		}
		
		/// <summary>
		/// Returns an enumerator for the list of listeners.
		/// </summary>
		/// <return>
		/// List Enumerator of type IEnumerator.
		/// </return>
		public IEnumerator GetEnumerator()
		{
			return listeners.GetEnumerator();
		}
		
		/// <summary>
		/// Gets the index of the specified listener.
		/// </summary>
		/// <param name="listener">
		/// The listener to search for
		/// </param>
		/// <return>
		/// The index of the listener in the list, if it exists.
		/// </return>
		[MonoTODO]
		public int IndexOf(object listener)
		{
			// TODO: we may have to add in some type-checking here.
			return listeners.IndexOf(listener);
		}
		
		/// <summary>
		/// Inserts the specified listener into the list at the specified index.
		/// </summary>
		/// <param name="index">
		/// Location in the list to insert the listener.
		/// </param>
		/// <param name="listener">
		/// The TraceListener to insert into the list.
		/// </param>
		public void Insert(int index, object listener)
		{
			listeners.Insert(index, listener);
		}
		
		/// <summary>
		/// Removes the listener with the specified name from the list, if it exists.
		/// </summary>
		/// <param name="name">
		/// Name of listener to remove
		/// </param>
		[MonoTODO]
		public void Remove(object name)
		{
			try {
				// TODO: may use an enumerator here.
				for(int i = 0; i < listeners.Count; i++)
				{
					TraceListener listener = (TraceListener) listeners[i];
					if(listener == null)
						continue;
					if(listener.Name.Equals(name))
						listeners.Remove(listener);
				}
			} catch {
				throw new ArgumentException("Listener is not in list.");
			}
		}
		
		/// <summary>
		/// Removes the specified listener from the list
		/// </summary>
		/// <param name="listener">
		/// The listener to remove.
		/// </param>
		public void Remove(TraceListener listener)
		{
			listeners.Remove(listener);
		}
		
		/// <summary>
		/// Removes the listener at the specified index.
		/// </summary>
		/// <param name="index">
		/// Location of the listener to remove.
		/// </param>
		public void RemoveAt(int index)
		{
			try {
				listeners.RemoveAt(index);
			} catch(Exception e) {
				throw new ArgumentOutOfRangeException(e.ToString());
			}
		}
		
		~TraceListenerCollection()
		{
			listeners = null;
		}
		
		public bool IsReadOnly
		{
			get
			{
				return isReadOnly;
			}
		}
		
		public bool IsFixedSize
		{
			get
			{
				return isFixedSize;
			}
		}
		
		public object SyncRoot
		{
			get
			{
				return this;
			}
		}
		
		public bool IsSynchronized
		{
			get
			{
				return isSynchronized;
			}
		}
	}
}
