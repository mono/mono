//
// System.Collections.DictionaryBase.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Collections {

	/// <summary>
	///   An abstract class that provides a simple way to monitor changes to a
	///   Hashtable.  Derived classes overwrite one or more of the `On' methods
	///   to track the changes to the Hashtable.
	/// </summary>
	///
	/// <remarks>
	///   This class is a base class that can simplify the development of
	///   strongly typed collections.  The idea being that the insertion of elements
	///   into the Hashtable can be forced to be of a given type.
	///
	///   The `On' members are protected and designed to be used only by derived
	///   classes.
	/// </remarks>
	[Serializable]
	public abstract class DictionaryBase : IDictionary, ICollection, IEnumerable {

		Hashtable dictionary;
		
		protected DictionaryBase ()
		{
			dictionary = new Hashtable ();
		}

		/// <summary>
		///   Clears the contents of the dictionary
		/// </summary>
		public void Clear ()
		{
			OnClear ();
			dictionary.Clear ();
			OnClearComplete ();
		}

		/// <summary>
		///   Returns the number of items in the dictionary
		/// </summary>
		public int Count {
			get {
				return dictionary.Count;
			}
		}

		/// <summary>
		///   The collection contained as an IDictionary
		/// </summary>
		protected IDictionary Dictionary {
			get {
				return this;
			}
		}

		/// <summary>
		///   The internal Hashtable representation for this dictionary
		/// </summary>
		protected Hashtable InnerHashtable {
			get {
				return dictionary;
			}
		}

		/// <summary>
		///   Copies the contents of the Dictionary into the target array
		/// </summary>
		/// <param name="array">
		///   The array to copy the contents of the dictionary to.  The
		///   array must have a zero-based indexing
		/// </param>
		/// <param name="index">
		///   Starting index within the array where to copy the objects
		///   to.
		/// </param>
		public void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index must be possitive");
			if (array.Rank > 1)
				throw new ArgumentException ("array is multidimensional");
			int size = array.Length;
			if (index > size)
				throw new ArgumentException ("index is larger than array size");
			if (index + Count > size)
				throw new ArgumentException ("Copy will overlflow array");

			DoCopy (array, index);
		}

		/// <summary>
		///   Internal routine called by CopyTo to perform the actual
		///   copying of the data
		/// </summary>
		private void DoCopy (Array array, int index)
		{
			foreach (DictionaryEntry de in dictionary)
				array.SetValue (de, index++);
		}

		/// <summary>
		///   Returns an enumerator for the dictionary
		/// </summary>
		public IDictionaryEnumerator GetEnumerator ()
		{
			return dictionary.GetEnumerator ();
		}

		/// <summary>
		///   Hook invoked before the clear operation
		///   is performed on the DictionaryBase
		/// </summary>
		protected virtual void OnClear ()
		{
		}

		/// <summary>
		///   Hook invoked after the clear operation
		///   is performed on the DictionaryBase
		/// </summary>
		///
		/// <remarks>
		///   The default implementation does nothing, derived classes
		///   can override this method to be notified of changes
		/// </remarks>
		protected virtual void OnClearComplete ()
		{
		}

		/// <summary>
		///   Hook invoked while fetching data from the DictionaryBase.
		/// </summary>
		///
		/// <remarks>
		///   This method is provided as a simple way to override the values
		///   returned by the DictionaryBase. 
		/// </remarks>
		///
		/// <param name="key">Key of the object to retrieve</param>
		/// <param name="current_value">Current value of the object associated with
		/// <paramref name="key"/></param>
		protected virtual object OnGet (object key, object current_value)
		{
			return current_value;
		}

		/// <summary>
		///   Hook invoked before inserting data into the DictionaryBase.
		/// </summary>
		///
		/// <remarks>
		///   Derived classes can override this method and perform some
		///   action before the <paramref name="current_value"/> is inserted
		///   into the dictionary.
		///
		///   The default implementation does nothing, derived classes
		///   can override this method to be notified of changes
		/// </remarks>
		///
		/// <param name="key">Key of the object to insert</param>
		/// <param name="current_value">Current value of the object associated with
		/// <paramref name="key"/></param>
		protected virtual void OnInsert (object key, object current_value)
		{
		}
		
		/// <summary>
		///   Hook invoked after inserting the data into the DictionaryBase
		/// </summary>
		///
		/// <remarks>
		///   The default implementation does nothing, derived classes
		///   can override this method to be notified of changes
		/// </remarks>
		///
		/// <param name="key">Key of the object to insert</param>
		/// <param name="current_value">Current value of the object associated with
		/// <paramref name="key"/></param>
		protected virtual void OnInsertComplete (object key, object current_value)
		{
		}

		/// <summary>
		///   Hook invoked before changing a value for a key in the DictionaryBase.
		/// </summary>
		///
		/// <remarks>
		///   Derived classes can override this method and perform some
		///   action before the <paramref name="current_value"/> is changed
		///   in the dictionary.  
		/// </remarks>
		///
		/// <param name="key">Key of the object to change</param>
		/// <param name="current_value">Current value of the object associated with
		/// <paramref name="key"/></param>
		protected virtual void OnSet (object key, object current_value, object new_value)
		{
		}
		
		/// <summary>
		///   Hook invoked after changing a value for a key in the DictionaryBase.
		/// </summary>
		///
		/// <remarks>
		///   The default implementation does nothing, derived classes
		///   can override this method to be notified of changes
		/// </remarks>
		///
		/// <param name="key">Key of the object to change</param>
		/// <param name="current_value">Current value of the object associated with
		/// <paramref name="key"/></param>
		protected virtual void OnSetComplete (object key, object current_value, object new_value)
		{
		}

		/// <summary>
		///   Hook invoked before removing a key/value from the DictionaryBase.
		/// </summary>
		///
		/// <remarks>
		///   Derived classes can override this method and perform some
		///   action before the <paramref name="current_value"/> is removed
		///   from the dictionary.  
		/// </remarks>
		///
		/// <param name="key">Key of the object to remove</param>
		/// <param name="current_value">Current value of the object associated with
		/// <paramref name="key"/></param>
		protected virtual void OnRemove (object key, object current_value)
		{
		}
		
		/// <summary>
		///   Hook invoked after removing a key/value from the DictionaryBase.
		/// </summary>
		///
		/// <remarks>
		///   The default implementation does nothing, derived classes
		///   can override this method to be notified of changes.
		/// </remarks>
		///
		/// <param name="key">Key of the object to remove</param>
		/// <param name="current_value">Current value of the object associated with
		/// <paramref name="key"/></param>
		protected virtual void OnRemoveComplete (object key, object current_value)
		{
		}
		
		/// <summary>
		///   Hook invoked after the value has been validated
		/// </summary>
		///
		/// <remarks>
		///   The default implementation does nothing, derived classes
		///   can override this method to monitor the DictionaryBase.
		/// </remarks>
		///
		/// <param name="key">Key of the object to retrieve</param>
		/// <param name="current_value">Current value of the object associated with
		/// <paramref name="key"/></param>
		protected virtual void OnValidate (object key, object current_value)
		{
		}

		bool IDictionary.IsFixedSize {
			get {
				return false;
			}
		}

		bool IDictionary.IsReadOnly {
			get {
				return false;
			}
		}

		object IDictionary.this [object key] {
			get {
				OnGet (key, dictionary[key]);
				object value = dictionary [key];
				return value;
			}

			set {
				OnValidate (key, value);
				object current_value = dictionary [key];
				OnSet (key, current_value, value);
				dictionary [key] = value;
				try {
					OnSetComplete (key, current_value, value);
				} catch {
					dictionary [key] = current_value;
					throw;
				}
			}
		}

		ICollection IDictionary.Keys {
			get {
				return dictionary.Keys;
			}
		}

		ICollection IDictionary.Values {
			get {
				return dictionary.Values;
			}
		}

		/// <summary>
		///   Adds a key/value pair to the dictionary.
		/// </summary>
		void IDictionary.Add (object key, object value)
		{
			OnValidate (key, value);
			OnInsert (key, value);
			dictionary.Add (key, value);
			try {
				OnInsertComplete (key, value);
			} catch {
				dictionary.Remove(key);
				throw;
			}
		}

		/// <summary>
		///   Removes a Dictionary Entry based on its key
		/// </summary>
		void IDictionary.Remove (object key)
		{
			object value = dictionary [key];
			OnValidate (key, value);
			OnRemove (key, value);
			dictionary.Remove (key);
			OnRemoveComplete (key, value);
		}

		/// <summary>
		///   Tests whether the dictionary contains an entry
		/// </summary>
		bool IDictionary.Contains (object key)
		{
			return dictionary.Contains (key);
		}

		bool ICollection.IsSynchronized {
			get {
				return dictionary.IsSynchronized;
			}
		}

		object ICollection.SyncRoot {
			get {
				return dictionary.SyncRoot;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return dictionary.GetEnumerator ();
		}
	}
}
