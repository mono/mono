//
// EncryptionProperties.cs - EncryptionProperties implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptionProperties
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Collections;
using System.Xml;

namespace System.Security.Cryptography.Xml {
	public sealed class EncryptionProperties : IList, ICollection, IEnumerable {

		#region Fields
		
		ArrayList list;

		#endregion // Fields
	
		#region Constructors

		public EncryptionProperties ()
		{
			list = new ArrayList ();
		}
	
		#endregion // Constructors
	
		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public bool IsFixedSize {
			get { return list.IsFixedSize; }
		}

		public bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object IList.this [int index] {
			get { return this [index]; }
			set { this [index] = (EncryptionProperty) value; }
		}

		public EncryptionProperty this [int index] {
			get { return (EncryptionProperty) list [index]; }
			set { list [index] = value; }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		#endregion // Properties

		#region Methods

		public int Add (EncryptionProperty value)
		{
			return list.Add (value);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (EncryptionProperty value)
		{
			return list.Contains (value);
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public void CopyTo (EncryptionProperty[] array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		bool IList.Contains (object value)
		{
			return Contains ((EncryptionProperty) value);
		}

		int IList.Add (object value)
		{
			return Add ((EncryptionProperty) value);
		}

		int IList.IndexOf (object value)
		{
			return IndexOf ((EncryptionProperty) value);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, (EncryptionProperty) value);
		}

		void IList.Remove (object value)
		{
			Remove ((EncryptionProperty) value);
		}

		public int IndexOf (EncryptionProperty value)
		{
			return list.IndexOf (value);
		}

		public void Insert (int index, EncryptionProperty value)
		{
			list.Insert (index, value);
		}

		public EncryptionProperty Item (int index)
		{
			return (EncryptionProperty) list [index];
		}

		public void Remove (EncryptionProperty value)
		{
			list.Remove (value);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
	}
}

#endif
