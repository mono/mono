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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Rolf Bjarne Kvinge  (RKvinge@novell.com)
//


#if NET_2_0

using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DataGridViewComboBoxCellTest : TestHelper 
	{
		private DataGridView CreateAndFill ()
		{
			DataGridView dgv = DataGridViewCommon.CreateAndFill ();
			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (new DataGridViewComboBoxCell ());
			row.Cells.Add (new DataGridViewComboBoxCell ());
			dgv.Rows.Add (row);
			return dgv;
		}
		
		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("DataGridViewComboBoxCell { ColumnIndex=-1, RowIndex=-1 }", new DataGridViewComboBoxCell ().ToString (), "B");
			
			using (DataGridView dgv = CreateAndFill ())
			{
				Assert.AreEqual ("DataGridViewComboBoxCell { ColumnIndex=1, RowIndex=2 }", dgv [1, 2].ToString (), "A");
			}
			
			
		}
	}
}
#endif