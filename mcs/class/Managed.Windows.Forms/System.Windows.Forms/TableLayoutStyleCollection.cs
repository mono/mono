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
			return ((IList)this).Add (style);
		}
		
		public void Clear ()
		{
			foreach (TableLayoutStyle style in al)
				style.Owner = null;
			al.Clear ();
			table.PerformLayout ();
		}
		
		public int Count {
			get { return al.Count; }
		}
		
		public void RemoveAt (int index)
		{
			((TableLayoutStyle)al[index]).Owner = null;
			al.RemoveAt (index);
			table.PerformLayout ();
		}
		
#region IList methods
		//
		// The IList methods will later be implemeneted, this is to get us started
		//
		int IList.Add (object style)
		{
			TableLayoutStyle layoutStyle = (TableLayoutStyle) style;
			if (layoutStyle.Owner != null)
				throw new ArgumentException ("Style is already owned");

			layoutStyle.Owner = table;
			int result = al.Add (layoutStyle);

			if (table != null)
				table.PerformLayout ();

			return result;
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
			if (((TableLayoutStyle)style).Owner != null)
				throw new ArgumentException ("Style is already owned");
			((TableLayoutStyle)style).Owner = table;
			al.Insert (index, (TableLayoutStyle) style);
			table.PerformLayout ();
		}

		void IList.Remove (object style)
		{
			((TableLayoutStyle)style).Owner = null;
			al.Remove ((TableLayoutStyle) style);
			table.PerformLayout ();
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
				if (((TableLayoutStyle)value).Owner != null)
					throw new ArgumentException ("Style is already owned");
				((TableLayoutStyle)value).Owner = table;
				al [index] = value;
				table.PerformLayout ();
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
		public TableLayoutStyle this [int index] {
			get {
				return (TableLayoutStyle) ((IList)this)[index];
			}

			set {
				((IList)this)[index] = value;
			}
		}
	}
		
}
