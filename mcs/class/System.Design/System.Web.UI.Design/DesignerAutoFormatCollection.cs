//
// System.ComponentModel.Design.Data.DesignerAutoFormatCollection
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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

#if NET_2_0

using System;
using System.Drawing;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Collections;

namespace System.Web.UI.Design
{
	public sealed class DesignerAutoFormatCollection : IList, ICollection, IEnumerable
	{
		public DesignerAutoFormatCollection ()
		{
		}

		[MonoTODO]
		public int Count {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DesignerAutoFormat this [int index] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Size PreviewSize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object SyncRoot {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int Add (DesignerAutoFormat format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains (DesignerAutoFormat format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf (DesignerAutoFormat format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Insert (int index, DesignerAutoFormat format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (DesignerAutoFormat format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int ICollection.Count {
			get { return Count; }
		}

		[MonoTODO]
		bool ICollection.IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IList.Add (object item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IList.Contains (object item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IList.IndexOf (object item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Insert (int index, object item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Remove (object item)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IList.IsFixedSize {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		bool IList.IsReadOnly {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		object IList.this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}

#endif
