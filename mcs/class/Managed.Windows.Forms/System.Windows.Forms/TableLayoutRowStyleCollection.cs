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

	public class TableLayoutRowStyleCollection : TableLayoutStyleCollection {
		
		internal TableLayoutRowStyleCollection (TableLayoutPanel panel) : base (panel)
		{
		}
		
		public int Add (RowStyle rowStyle)
		{
			return base.Add (rowStyle);
		}

		public bool Contains (RowStyle rowStyle)
		{
			return ((IList)this).Contains (rowStyle);
		}

		public int IndexOf (RowStyle rowStyle)
		{
			return ((IList)this).IndexOf (rowStyle);
		}

		public void Insert (int index, RowStyle rowStyle)
		{
			((IList)this).Insert (index, rowStyle);
		}

		public void Remove (RowStyle rowStyle)
		{
			((IList)this).Remove (rowStyle);
		}
		
		public new RowStyle this [int index] {
			get {
				return (RowStyle) base [index];
			}
			
			set {
				base [index] = value;
			}
		}
	}
}
