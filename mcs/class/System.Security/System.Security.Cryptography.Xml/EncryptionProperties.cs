//
// EncryptionProperties.cs - EncryptionProperties implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptionProperties
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Collections;
using System.Runtime.CompilerServices;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public sealed class EncryptionPropertyCollection : IList, ICollection, IEnumerable {

		#region Fields
		
		ArrayList list;

		#endregion // Fields
	
		#region Constructors

		public EncryptionPropertyCollection ()
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

		[IndexerName ("ItemOf")]
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
