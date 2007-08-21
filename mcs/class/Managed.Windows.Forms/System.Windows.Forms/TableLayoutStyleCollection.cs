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
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2004-2006 Novell, Inc.
//

#if NET_2_0
using System;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms {
	[Editor ("System.Windows.Forms.Design.StyleCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
	public abstract class TableLayoutStyleCollection : IList, ICollection, IEnumerable
	{
		ArrayList al = new ArrayList ();
		TableLayoutPanel table;
		
		internal TableLayoutStyleCollection (TableLayoutPanel table)
		{
			this.table = table;
		}
		
		public int Add (TableLayoutStyle style)
		{
			return al.Add (style);
		}
		
		public void Clear ()
		{
			al.Clear ();
			
			// FIXME: Need to investigate what happens when the style is gone.
			table.PerformLayout ();
		}
		
		public int Count {
			get { return al.Count; }
		}
		
		public void RemoveAt (int index)
		{
			al.RemoveAt (index);
		}
		
#region IList methods
		//
		// The IList methods will later be implemeneted, this is to get us started
		//
		int IList.Add (object style)
		{
			return al.Add ((TableLayoutStyle) style);
		}
		
		bool IList.Contains (object style)
		{
			return al.Contains ((TableLayoutStyle) style);
		}
		
		int IList.IndexOf (object style)
		{
			return al.IndexOf ((TableLayoutStyle) style);
		}
		
		void IList.Insert (int index, object style)
		{
			al.Insert (index, (TableLayoutStyle) style);
		}

		void IList.Remove (object style)
		{
			al.Remove ((TableLayoutStyle) style);
		}

		bool IList.IsFixedSize {
			get {
				return al.IsFixedSize;
			}
		}

		bool IList.IsReadOnly {
			get {
				return al.IsReadOnly;
			}
		}

		object IList.this [int index] {
			get {
				return al [index];
			}
			set {
				al [index] = value;
			}
		}
#endregion

#region ICollection methods
		void ICollection.CopyTo (Array array, int startIndex)
		{
			al.CopyTo (array, startIndex);
		}

		object ICollection.SyncRoot {
			get {
				return al.SyncRoot;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return al.IsSynchronized;
			}
		}
#endregion

#region IEnumerable methods
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return al.GetEnumerator ();
		}
#endregion
		public TableLayoutStyle this [int idx] {
			get {
				return (TableLayoutStyle) al [idx];
			}

			set {
				al [idx] = value;
			}
		}
	}
		
}	
#endif 
