//
// System.Web.UI.HtmlControls.HtmlTableCellCollection.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using System.Collections;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HtmlTableCellCollection : ICollection, IEnumerable {

		ControlCollection cc;


		internal HtmlTableCellCollection (HtmlTableRow tr)
		{
			cc = tr.Controls;
		}


		public int Count {
			get { return cc.Count; }
		}

		public bool IsReadOnly {
			get { return false; }	// documented as always false
		}

		public bool IsSynchronized {
			get { return false; }	// documented as always false
		}

		public HtmlTableCell this [int index] {
			get { return (HtmlTableCell) cc [index]; }
		}

		public object SyncRoot {
			get { return this; }	// as documented
		}


		public void Add (HtmlTableCell cell)
		{
			cc.Add (cell);
		}

		public void Clear ()
		{
			cc.Clear ();
		}

		public void CopyTo (Array array, int index)
		{
			cc.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return cc.GetEnumerator ();
		}

		public void Insert (int index, HtmlTableCell cell)
		{
			cc.AddAt (index, cell);
		}

		public void Remove (HtmlTableCell cell)
		{
			cc.Remove (cell);
		}

		public void RemoveAt (int index)
		{
			cc.RemoveAt (index);
		}
	}
}
