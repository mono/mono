//
// System.Diagnostics.TraceListenerCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original
// implementation.
//
// (C) 2002 Jonathan Pryor
//


using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace System.Diagnostics {

	/// <summary>
	/// Provides a list of TraceListener objects.
	/// </summary>
	public class TraceListenerCollection : IList, ICollection, IEnumerable {

		private ArrayList listeners = new ArrayList ();

		internal TraceListenerCollection ()
		{
			Add (new DefaultTraceListener ());
		}

		/// <summary>
		/// Returns the number of items in the list
		/// </summary>
		/// <value>
		/// The number of items
		/// </value>
		public int Count{
			get {return listeners.Count;}
		}

		/// <summary>
		/// Gets the first TraceListener in the list with the
		/// specified name.
		/// </summary>
		public TraceListener this [string name] {
			get {
				foreach (TraceListener listener in listeners) {
					if (listener.Name == name)
						return listener;
				}
				return null;
			}
		}

		public TraceListener this [int index] {
			get {return (TraceListener) listeners[index];}
			set {listeners[index] = value;}
		}

		object IList.this [int index] {
			get {return listeners[index];}
			set {((IList)this).Insert (index, value);}
		}

		bool ICollection.IsSynchronized {
			get {return listeners.IsSynchronized;}
		}

		object ICollection.SyncRoot {
			get {return listeners.SyncRoot;}
		}

		bool IList.IsFixedSize {
			get {return listeners.IsFixedSize;}
		}

		bool IList.IsReadOnly {
			get {return listeners.IsReadOnly;}
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
		public int Add (TraceListener listener)
		{
			return listeners.Add (listener);
		}

		/// <summary>
		/// Adds an array of TraceListeners to the list.
		/// </summary>
		/// <param name="value">
		/// Array of TraceListeners to add
		/// </param>
		public void AddRange (TraceListener[] value)
		{
			listeners.AddRange (value);
		}

		/// <summary>
		/// Adds the contents of another TraceListenerCollection to this one.
		/// </summary>
		/// <param name="value">
		/// The TraceListenerCollection to copy values from.
		/// </param>
		public void AddRange (TraceListenerCollection value)
		{
			listeners.AddRange (value.listeners);
		}

		/// <summary>
		/// Clears all listeners from the list.
		/// </summary>
		public void Clear ()
		{
			listeners.Clear ();
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
		public bool Contains (TraceListener listener)
		{
			return listeners.Contains (listener);
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
		public void CopyTo (TraceListener[] listeners, int index)
		{
			listeners.CopyTo (listeners, index);
		}

		/// <summary>
		/// Returns an enumerator for the list of listeners.
		/// </summary>
		/// <return>
		/// List Enumerator of type IEnumerator.
		/// </return>
		public IEnumerator GetEnumerator ()
		{
			return listeners.GetEnumerator ();
		}

		void ICollection.CopyTo (Array array, int index)
		{
			listeners.CopyTo (array, index);
		}

		int IList.Add (object value)
		{
			if (value is TraceListener)
				return listeners.Add (value);
			throw new NotSupportedException (Locale.GetText (
				"You can only add TraceListener objects to the collection"));
		}

		bool IList.Contains (object value)
		{
			if (value is TraceListener)
				return listeners.Contains (value);
			return false;
		}

		int IList.IndexOf (object value)
		{
			if (value is TraceListener)
				return listeners.IndexOf (value);
			return -1;
		}

		void IList.Insert (int index, object value)
		{
			if (value is TraceListener) {
				listeners.Insert (index, value);
				return;
			}
			throw new NotSupportedException (Locale.GetText (
				"You can only insert TraceListener objects into the collection"));
		}

		void IList.Remove (object value)
		{
			if (value is TraceListener)
				listeners.Remove (value);
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
		public int IndexOf (TraceListener listener)
		{
			return listeners.IndexOf (listener);
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
		public void Insert (int index, TraceListener listener)
		{
			listeners.Insert (index, listener);
		}

		/// <summary>
		/// Removes the listener with the specified name from the list, if it 
		/// exists.
		/// </summary>
		/// <param name="name">
		/// Name of listener to remove
		/// </param>
		public void Remove (string name)
		{
			TraceListener found = null;

			foreach (TraceListener listener in listeners) {
				if (listener.Name == name) {
					found = listener;
					break;
				}
			}

			if (found != null)
				listeners.Remove (found);
			else
				throw new ArgumentException (Locale.GetText (
					"TraceListener " + name + " was not in the collection"));
		}

		/// <summary>
		/// Removes the specified listener from the list
		/// </summary>
		/// <param name="listener">
		/// The listener to remove.
		/// </param>
		public void Remove (TraceListener listener)
		{
			listeners.Remove (listener);
		}

		/// <summary>
		/// Removes the listener at the specified index.
		/// </summary>
		/// <param name="index">
		/// Location of the listener to remove.
		/// </param>
		public void RemoveAt (int index)
		{
			listeners.RemoveAt (index);
		}
	}
}

