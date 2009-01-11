//
// DsmlRequestDocument.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.
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

using System;
using System.Collections;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	[MonoTODO]
	public class DsmlRequestDocument : DsmlDocument, IList, ICollection, IEnumerable
	{
		IList list = new ArrayList ();

		internal DsmlRequestDocument ()
		{
		}

		public DsmlDocumentProcessing DocumentProcessing { get; set; }
		public DsmlErrorProcessing ErrorProcessing { get; set; }

		public override XmlDocument ToXml ()
		{
			throw new NotImplementedException ();
		}

		public int Count {
			get { return list.Count; }
		}

		protected bool IsFixedSize {
			get { return list.IsFixedSize; }
		}

		protected bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		protected bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public DirectoryRequest this [int index] {
			get { return (DirectoryRequest) list [index]; }
			set { list [index] = value; }
		}

		public string RequestId { get; set; }

		public DsmlResponseOrder ResponseOrder { get; set; }

		protected object SyncRoot {
			get { return list.SyncRoot; }
		}

		int ICollection.Count {
			get { return Count; }
		}

		bool ICollection.IsSynchronized {
			get { return IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return SyncRoot; }
		}

		bool IList.IsFixedSize {
			get { return IsFixedSize; }
		}

		bool IList.IsReadOnly {
			get { return IsReadOnly; }
		}

		object IList.this [int index] {
			get { return this [index]; }
			set { this [index] = (DirectoryRequest) value; }
		}

		public int Add (DirectoryRequest request)
		{
			throw new NotImplementedException ();
		}

		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		public bool Contains (DirectoryRequest value)
		{
			throw new NotImplementedException ();
		}

		public void CopyTo (DirectoryRequest [] value, int i)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public int IndexOf (DirectoryRequest value)
		{
			throw new NotImplementedException ();
		}

		public void Insert (int index, DirectoryRequest value)
		{
			throw new NotImplementedException ();
		}

		public void Remove (DirectoryRequest value)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		void ICollection.CopyTo (Array value, int i)
		{
			CopyTo ((DirectoryRequest []) value, i);
		}

		int IList.Add (object request)
		{
			return Add ((DirectoryRequest) request);
		}

		void IList.Clear ()
		{
			Clear ();
		}

		bool IList.Contains (object value)
		{
			return Contains ((DirectoryRequest) value);
		}

		int IList.IndexOf (object value)
		{
			return IndexOf ((DirectoryRequest) value);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, (DirectoryRequest) value);
		}

		void IList.Remove (object value)
		{
			Remove ((DirectoryRequest) value);
		}

		void IList.RemoveAt (int index)
		{
			RemoveAt (index);
		}
	}
}
