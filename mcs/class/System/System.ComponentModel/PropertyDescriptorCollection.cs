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
		public int Add (PropertyDescriptor value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (PropertyDescriptor value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (Array array, int index) {
			throw new NotImplementedException ();
		}

		public virtual PropertyDescriptor Find (string name, bool ignoreCase) {
		}

		public virtual IEnumerator GetEnumerator () {
		}

		[MonoTODO]
		public int IndexOf (PropertyDescriptor value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, PropertyDescriptor value) {
			throw new NotImplementedException ();
		}

		public void Remove (PropertyDescriptor value) {
			Remove (IndexOf (value));
		}

		[MonoTODO]
		public void RemoveAt (int index) {
			throw new NotImplementedException ();
		}

		public virtual PropertyDescriptorCollection Sort () {
		}

		public virtual PropertyDescriptorCollection Sort (IComparer ic) {
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

		bool IsFixedSize {
			[MonoTODO]
			get {
				throw new NotImplementedException ();	
			}
		}

		bool IsReadOnly {
			[MonoTODO]
			get {
				throw new NotImplementedException ();	
			}
		}

		ICollection Keys {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		ICollection Values {
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

		object this[object key] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}
		
		public virtual PropertyDescriptor this[string s] {
			get {
			}
		}
		
		public virtual PropertyDescriptor this[int i] {
			get {
			}
		}
	}
}
