//
// System.ComponentModel.PropertyDescriptorCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Rodrigo Moya, 2002
//

using System.Collections;

namespace System.ComponentModel
{
	/// <summary>
	/// Represents a collection of PropertyDescriptor objects.
	/// </summary>
	public class PropertyDescriptorCollection : IList, ICollection,
		IEnumerable, IDictionary
	{
		[MonoTODO]
		public PropertyDescriptorCollection (PropertyDescriptor[] properties) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Add (PropertyDescriptor value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IList.Add (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDictionary.Add (object key, object value)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Clear () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Clear () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDictionary.Clear () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (PropertyDescriptor value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IList.Contains (object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IDictionary.Contains (object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (Array array, int index) {
			throw new NotImplementedException ();
		}

		public virtual PropertyDescriptor Find (string name, bool ignoreCase) {
			throw new NotImplementedException ();
		}

		public virtual IEnumerator GetEnumerator () {
			throw new NotImplementedException ();
		}

		IDictionaryEnumerator IDictionary.GetEnumerator () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf (PropertyDescriptor value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IList.IndexOf (object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, PropertyDescriptor value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Insert (int index, object value) {
			throw new NotImplementedException ();
		}

		public void Remove (PropertyDescriptor value) {
			throw new NotImplementedException ();
		}

		void IDictionary.Remove (object value) {
			throw new NotImplementedException ();
		}

		void IList.Remove (object value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (int index) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.RemoveAt (int index) {
			throw new NotImplementedException ();
		}

		public virtual PropertyDescriptorCollection Sort () {
			throw new NotImplementedException ();
		}

		public virtual PropertyDescriptorCollection Sort (IComparer ic) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void InternalSort (IComparer ic) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void InternalSort (string[] order) {
			throw new NotImplementedException ();
		}
		
		public static readonly PropertyDescriptorCollection Empty;

		public bool IsFixedSize {
			[MonoTODO]
			get {
				throw new NotImplementedException ();	
			}
		}

		public bool IsReadOnly {
			[MonoTODO]
			get {
				throw new NotImplementedException ();	
			}
		}

		public bool IsSynchronized {
			[MonoTODO]
			get {
				throw new NotImplementedException ();	
			}
		}

		public ICollection Keys {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public ICollection Values {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
		
		public int Count {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		object ICollection.SyncRoot {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
		
		public object this[object key] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public virtual PropertyDescriptor this[string s] {
			get {
				throw new NotImplementedException ();
			}
		}

		object IList.this [int index] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual PropertyDescriptor this[int index] {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}
