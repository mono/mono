//
// ReferenceList.cs - ReferenceList implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-ReferenceList
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

		bool IList.IsFixedSize {
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		[IndexerName ("ItemOf")]
		public EncryptedReference this [int index] {
			get { return (EncryptedReference) list [index]; }
			set { list [index] = value; }
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

		public EncryptedReference Item (int index)
		{
			return (EncryptedReference) list [index];
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
