//
// ReferenceList.cs - ReferenceList implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-ReferenceList
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_1_2

using System.Collections;
using System.Xml;

namespace System.Security.Cryptography.Xml {
	public sealed class ReferenceList : IList, ICollection, IEnumerable {

		#region Fields

		ArrayList list;

		#endregion // Fields

		#region Constructors

		public ReferenceList ()
		{
			list = new ArrayList ();
		}
	
		#endregion // Constructors

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		object IList.this [int index] {
			get { return this [index]; }
			set { this [index] = (EncryptedReference) value; }
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

		public EncryptedReference this [int oid] {
			get { return (EncryptedReference) list [oid]; }
			set { this [oid] = value; }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		#endregion // Properties

		#region Methods

		public int Add (object value)
		{
			if (!(value is EncryptedReference))
				throw new ArgumentException ("value");
			return list.Add (value);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (object value)
		{
			return list.Contains (value);
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		public void Insert (int index, object value)
		{
			if (!(value is EncryptedReference))
				throw new ArgumentException ("value");
			list.Insert (index, value);
		}

		public EncryptedReference Item (int index) 
		{
			return (EncryptedReference) list [index];
		}

		public void Remove (object value)
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
