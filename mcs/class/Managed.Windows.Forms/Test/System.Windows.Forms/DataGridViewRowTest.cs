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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//


#if NET_2_0

using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DataGridViewRowTest {
		
		[SetUp]
		public void GetReady() {}

		[TearDown]
		public void Clean() {}

		[Test]
		public void TestDefaultValues () {
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestVisibleInvalidOperationException () {
			DataGridView grid = new DataGridView();
			DataGridViewRow row = new DataGridViewRow();
			grid.Rows.Add(row);
			row.Visible = false;
		}

		[Test]
		public void Height ()
		{
			DataGridViewRow row = new DataGridViewRow();
			Assert.IsTrue (row.Height > 5, "#1");
			row.Height = 70;
			Assert.AreEqual (70, row.Height, "#2");
			row.Height = 40;
			Assert.AreEqual (40, row.Height, "#3");
		}

		[Test]
		[Category ("NotWorking")]
		public void MinimumHeight ()
		{
			DataGridViewRow row = new DataGridViewRow();
			Assert.IsTrue (row.MinimumHeight > 0, "#A1");
			Assert.IsFalse (row.Height > row.MinimumHeight, "#A2");
			row.MinimumHeight = 40;
			row.Height = 50;
			Assert.AreEqual (40, row.MinimumHeight, "#B1");
			Assert.AreEqual (50, row.Height, "#B2");
			row.MinimumHeight = 20;
			Assert.AreEqual (20, row.MinimumHeight, "#C1");
			Assert.AreEqual (20, row.Height, "#C2");
			row.MinimumHeight = 40;
			Assert.AreEqual (40, row.MinimumHeight, "#D1");
			Assert.AreEqual (40, row.Height, "#D2");
		}
	}
}
#endif
