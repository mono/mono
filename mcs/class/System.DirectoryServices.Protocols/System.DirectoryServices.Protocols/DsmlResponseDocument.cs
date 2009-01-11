//
// DsmlResponseDocument.cs
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
	public class DsmlResponseDocument : DsmlDocument, ICollection, IEnumerable
	{
		ArrayList list = new ArrayList ();

		public DsmlDocumentProcessing DocumentProcessing { get; set; }
		public DsmlErrorProcessing ErrorProcessing { get; set; }

		[MonoTODO]
		public override XmlDocument ToXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsErrorResponse { get; private set; }
		[MonoTODO]
		public bool IsOperationError { get; private set; }
		[MonoTODO]
		public string RequestId { get; private set; }

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

		public DirectoryResponse this [int index] {
			get { return (DirectoryResponse) list [index]; }
			set { list [index] = value; }
		}

		public string ResponseId { get; set; }

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

		public int Add (DirectoryResponse request)
		{
			return list.Add (request);
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (DirectoryResponse value)
		{
			return list.Contains (value);
		}

		public void CopyTo (DirectoryResponse [] value, int i)
		{
			list.CopyTo (value, i);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (DirectoryResponse value)
		{
			return list.IndexOf (value);
		}

		public void Insert (int index, DirectoryResponse value)
		{
			list.Insert (index, value);
		}

		public void Remove (DirectoryResponse value)
		{
			list.Remove (value);
		}

		public void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		void ICollection.CopyTo (Array value, int i)
		{
			CopyTo ((DirectoryResponse []) value, i);
		}
	}
}
