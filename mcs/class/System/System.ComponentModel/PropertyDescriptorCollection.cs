//
// System.ComponentModel.PropertyDescriptorCollection.cs
//
// Authors:
// 	Rodrigo Moya (rodrigo@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Rodrigo Moya, 2002
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//
using System.Collections;
namespace System.ComponentModel
{
	/// <summary>
	/// Represents a collection of PropertyDescriptor objects.
	/// </summary>
	//[DefaultMember ("Item")]
	public class PropertyDescriptorCollection : IList, ICollection, IEnumerable, IDictionary
	{
		public static readonly PropertyDescriptorCollection Empty = new PropertyDescriptorCollection (null);
		ArrayList properties;
		bool readOnly;

		public PropertyDescriptorCollection (PropertyDescriptor[] properties)
		{
			this.properties = new ArrayList ();
			if (properties == null)
				return;

			foreach (PropertyDescriptor p in properties)
				this.properties.Add (p);
		}

		public int Add (PropertyDescriptor value)
		{
			properties.Add (value);
			return properties.Count - 1;
		}

		int IList.Add (object value)
		{
			return Add ((PropertyDescriptor) value);
		}

		void IDictionary.Add (object key, object value)
		{
			Add ((PropertyDescriptor) value);
		}

		public void Clear ()
		{
			properties.Clear ();
		}

		void IList.Clear ()
		{
			Clear ();
		}

		void IDictionary.Clear ()
		{
			Clear ();
		}

		public bool Contains (PropertyDescriptor value)
		{
			return properties.Contains (value);
		}

		bool IList.Contains (object value)
		{
			return Contains ((PropertyDescriptor) value);
		}

		bool IDictionary.Contains (object value)
		{
			return Contains ((PropertyDescriptor) value);
		}

		public void CopyTo (Array array, int index)
		{
			properties.CopyTo (array, index);
		}

		public virtual PropertyDescriptor Find (string name, bool ignoreCase)
		{
			foreach (PropertyDescriptor p in properties) {
				if (0 == String.Compare (name, p.Name, ignoreCase))
					return p;
			}
			return null;
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return properties.GetEnumerator ();
		}

		[MonoTODO]
		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public int IndexOf (PropertyDescriptor value)
		{
			return properties.IndexOf (value);
		}

		int IList.IndexOf (object value)
		{
			return IndexOf ((PropertyDescriptor) value);
		}

		public void Insert (int index, PropertyDescriptor value)
		{            Insert (index, value);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, (PropertyDescriptor) value);
		}

		public void Remove (PropertyDescriptor value)
		{
			properties.Remove (value);
		}

		void IDictionary.Remove (object value)
		{
			Remove ((PropertyDescriptor) value);
		}

		void IList.Remove (object value)
		{
			Remove ((PropertyDescriptor) value);
		}

		public void RemoveAt (int index)
		{
			properties.RemoveAt (index);
		}

		void IList.RemoveAt (int index)
		{
			RemoveAt (index);
		}

		public virtual PropertyDescriptorCollection Sort ()
		{
			properties.Sort ();
			return this;
		}

		public virtual PropertyDescriptorCollection Sort (IComparer comparer)
		{
			properties.Sort (comparer);
			return this;
		}

		[MonoTODO]
		public virtual PropertyDescriptorCollection Sort (string[] order) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual PropertyDescriptorCollection Sort (string[] order, IComparer comparer) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void InternalSort (IComparer ic)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void InternalSort (string [] order)
		{
			throw new NotImplementedException ();
		}

		bool IDictionary.IsFixedSize
		{
			get {
				return !readOnly;
			}
		}

		bool IList.IsFixedSize
		{
			get {
				return !readOnly;
			}
		}

		public bool IsReadOnly
		{
			get {
				return readOnly;
			}
		}

		public bool IsSynchronized
		{
			get {
				return false;
			}
		}

		public int Count
		{
			get {
				return properties.Count;
			}
		}

		object ICollection.SyncRoot
		{
			get {
				return null;
			}
		}

		ICollection IDictionary.Keys
		{
			get {
				string [] keys = new string [properties.Count];
				int i = 0;
				foreach (PropertyDescriptor p in properties)
					keys [i++] = p.Name;
				return keys;
			}
		}

		ICollection IDictionary.Values
		{
			get {
				return (ICollection) properties.Clone ();
			}
		}

		object IDictionary.this [object key]
		{
			get {
				if (!(key is string))
					return null;
				return this [(string) key];
			}
			set {
				if (!(key is string) || (value as PropertyDescriptor) == null)
					throw new ArgumentException ();
				int idx = properties.IndexOf (value);
				if (idx == -1)
					Add ((PropertyDescriptor) value);
				else
					properties [idx] = value;
			}
		}

		public virtual PropertyDescriptor this [string s]
		{
			get {
				return Find (s, false);
			}
		}

		object IList.this [int index]
		{
			get {
				return properties [index];
			}
			set {
				properties [index] = value;
			}
		}

		public virtual PropertyDescriptor this [int index]
		{
			get {
				return (PropertyDescriptor) properties [index];
			}
		}
	}
}

