//
// System.Web.UI.WebControls.GridViewRowCollection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004-2010 Novell, Inc (http://www.novell.com)
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
//

using System;
using System.Web.UI;
using System.Collections;

namespace System.Web.UI.WebControls
{
	public class GridViewRowCollection: ICollection, IEnumerable
	{
		ArrayList rows = new ArrayList ();
		
		public GridViewRowCollection (ArrayList rows)
		{
			this.rows = rows;
		}
		
		public GridViewRow this [int index] {
			get { return (GridViewRow) rows [index]; }
		}
		
		public void CopyTo (GridViewRow[] array, int index)
		{
			rows.CopyTo (array, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return rows.GetEnumerator ();
		}
		
		public int Count {
			get { return rows.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public object SyncRoot {
			get { return this; }
		}
		
		void System.Collections.ICollection.CopyTo (Array array, int index)
		{
			rows.CopyTo (array, index);
		}
	}
}

