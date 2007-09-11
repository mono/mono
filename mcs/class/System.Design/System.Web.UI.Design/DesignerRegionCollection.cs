//
// DesignerRegionCollection.cs
//
// Author:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2007 Novell, Inc.
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
using System.Collections;

namespace System.Web.UI.Design
{
	public class DesignerRegionCollection : IList, ICollection, IEnumerable
	{
		[MonoNotSupported ("")]
		public DesignerRegionCollection ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public DesignerRegionCollection (ControlDesigner owner)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public int Add (DesignerRegion region)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public bool Contains (DesignerRegion region)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public int IndexOf (DesignerRegion region)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public void Insert (int index, DesignerRegion region)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public void Remove (DesignerRegion region)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public void RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public int Count {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public bool IsFixedSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public bool IsReadOnly {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public bool IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public DesignerRegion this[int index] {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public ControlDesigner Owner {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public object SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}

		// Interface implementations
		// Interface methods

		[MonoNotSupported ("")]
		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		int IList.Add (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		void IList.Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		bool IList.Contains (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		int IList.IndexOf (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		void IList.Insert (int index, object o)
		{
			throw new NotImplementedException ();
		}
		
		[MonoNotSupported ("")]
		void IList.Remove (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		void IList.RemoveAt(int index)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		int ICollection.Count {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		bool ICollection.IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		object ICollection.SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		bool IList.IsFixedSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		bool IList.IsReadOnly {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		object IList.this[int index] {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}
	}
}
#endif