//
// System.ComponentModel.PropertyDescriptorCollection.cs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Rodrigo Moya, 2002
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) 2003 Andreas Nahr
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
using System.Collections;
namespace System.ComponentModel
{
	/// <summary>
	/// Represents a collection of PropertyDescriptor objects.
	/// </summary>
	public class PropertyDescriptorCollection : IList, ICollection, IEnumerable, IDictionary
	{
		public static readonly PropertyDescriptorCollection Empty = new PropertyDescriptorCollection (null, true);
		private ArrayList properties;
		private bool readOnly;

		public PropertyDescriptorCollection (PropertyDescriptor[] properties)
		{
			this.properties = new ArrayList ();
			if (properties == null)
				return;

			this.properties.AddRange (properties);
		}

		public PropertyDescriptorCollection (PropertyDescriptor[] properties, bool readOnly) : this (properties)
		{
			this.readOnly = readOnly;
		}
		
		private PropertyDescriptorCollection ()
		{
		}

		public int Add (PropertyDescriptor value)
		{
			if (readOnly) {
				throw new NotSupportedException ();
			}
			properties.Add (value);
			return properties.Count - 1;
		}

		int IList.Add (object value)
		{
			return Add ((PropertyDescriptor) value);
		}

		void IDictionary.Add (object key, object value)
		{
			if ((value as PropertyDescriptor) == null) {
				throw new ArgumentException ("value");
			}

			Add ((PropertyDescriptor) value);
		}

		public void Clear ()
		{
			if (readOnly) {
				throw new NotSupportedException ();
			}
			properties.Clear ();
		}

#if !TARGET_JVM // DUAL_IFACE_CONFLICT
		void IList.Clear ()
		{
			Clear ();
		}

		void IDictionary.Clear ()
		{
			Clear ();
		}
#endif

		public bool Contains (PropertyDescriptor value)
		{
			return properties.Contains (value);
		}

#if TARGET_JVM // DUAL_IFACE_CONFLICT
		public bool Contains (object value)
		{
			return Contains ((PropertyDescriptor) value);
		}
#else

		bool IList.Contains (object value)
		{
			return Contains ((PropertyDescriptor) value);
		}

		bool IDictionary.Contains (object value)
		{
			return Contains ((PropertyDescriptor) value);
		}
#endif

		public void CopyTo (Array array, int index)
		{
			properties.CopyTo (array, index);
		}

		public virtual PropertyDescriptor Find (string name, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			for (int i = 0; i < properties.Count; ++i) {
				PropertyDescriptor p = (PropertyDescriptor)properties [i];
				if (ignoreCase) {
					if (0 == String.Compare (name, p.Name, StringComparison.OrdinalIgnoreCase))
						return p;
				}
				else {
					if (0 == String.Compare (name, p.Name, StringComparison.Ordinal))
						return p;
				}
			}
			return null;
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return properties.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
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
		{
			if (readOnly) {
				throw new NotSupportedException ();
			}
			properties.Insert (index, value);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, (PropertyDescriptor) value);
		}

		public void Remove (PropertyDescriptor value)
		{
			if (readOnly) {
				throw new NotSupportedException ();
			}
			properties.Remove (value);
		}

#if TARGET_JVM// DUAL_IFACE_CONFLICT
		public void Remove (object value)
		{
			Remove ((PropertyDescriptor) value);
		}
#else
		void IDictionary.Remove (object value)
		{
			Remove ((PropertyDescriptor) value);
		}

		void IList.Remove (object value)
		{
			Remove ((PropertyDescriptor) value);
		}
#endif
		public void RemoveAt (int index)
		{
			if (readOnly) {
				throw new NotSupportedException ();
			}
			properties.RemoveAt (index);
		}

		void IList.RemoveAt (int index)
		{
			RemoveAt (index);
		}

		private PropertyDescriptorCollection CloneCollection ()
		{
			PropertyDescriptorCollection col = new PropertyDescriptorCollection ();
			col.properties = (ArrayList) properties.Clone ();
			return col;
		}
		
		public virtual PropertyDescriptorCollection Sort ()
		{
			PropertyDescriptorCollection col = CloneCollection ();
			col.InternalSort ((IComparer) null);
			return col;
		}

		public virtual PropertyDescriptorCollection Sort (IComparer comparer)
		{
			PropertyDescriptorCollection col = CloneCollection ();
			col.InternalSort (comparer);
			return col;
		}

		public virtual PropertyDescriptorCollection Sort (string[] order) 
		{
			PropertyDescriptorCollection col = CloneCollection ();
			col.InternalSort (order);
			return col;
		}

		public virtual PropertyDescriptorCollection Sort (string[] order, IComparer comparer) 
		{
			PropertyDescriptorCollection col = CloneCollection ();
			if (order != null) {
				ArrayList sorted = col.ExtractItems (order);
				col.InternalSort (comparer);
				sorted.AddRange (col.properties);
				col.properties = sorted;
			} else {
				col.InternalSort (comparer);
			}
			return col;
		}

		protected void InternalSort (IComparer ic)
		{
			if (ic == null)
				ic = MemberDescriptor.DefaultComparer;
			properties.Sort (ic);
		}

		protected void InternalSort (string [] order)
		{
			if (order != null) {
				ArrayList sorted = ExtractItems (order);
				InternalSort ((IComparer) null);
				sorted.AddRange (properties);
				properties = sorted;
			} else {
				InternalSort ((IComparer) null);
			}
		}
		
		ArrayList ExtractItems (string[] names)
		{
			ArrayList sorted = new ArrayList (properties.Count);
			object[] ext = new object [names.Length];
			
			for (int n=0; n<properties.Count; n++)
			{
				PropertyDescriptor ed = (PropertyDescriptor) properties[n];
				int i = Array.IndexOf (names, ed.Name);
				if (i != -1) {
					ext[i] = ed;
					properties.RemoveAt (n);
					n--;
				}
			}
			foreach (object ob in ext)
				if (ob != null) sorted.Add (ob);
				
			return sorted;
		}
		
		internal PropertyDescriptorCollection Filter (Attribute[] attributes)
		{
			ArrayList list = new ArrayList ();
			foreach (PropertyDescriptor pd in properties) {
				if (pd.Attributes.Contains (attributes)) {
					list.Add (pd);
				}
			}
			PropertyDescriptor[] descriptors = new PropertyDescriptor[list.Count];
			list.CopyTo (descriptors);
			return new PropertyDescriptorCollection (descriptors, true);
		}

#if TARGET_JVM //DUAL_IFACE_CONFLICT
		public bool IsFixedSize
#else
		bool IDictionary.IsFixedSize
		{
			get {return ((IList)this).IsFixedSize;}
		}
		bool IList.IsFixedSize
#endif
		{
			get 
			{
				return readOnly;
			}
		}
#if TARGET_JVM //DUAL_IFACE_CONFLICT
		public bool IsReadOnly
#else
		bool IDictionary.IsReadOnly
		{
			get {return ((IList)this).IsReadOnly;}
		}
		bool IList.IsReadOnly
#endif
		{
			get 
			{
				return readOnly;
			}
		}

		bool ICollection.IsSynchronized
		{
			get {
				return false;
			}
		}

		int ICollection.Count {
			get { return Count; }
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
				if (readOnly) {
					throw new NotSupportedException ();
				}

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
				if (readOnly) {
					throw new NotSupportedException ();
				}
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

