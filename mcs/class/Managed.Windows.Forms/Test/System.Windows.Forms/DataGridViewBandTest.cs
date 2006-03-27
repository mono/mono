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
using System.ComponentModel;
using System.Collections;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DataGridViewBandTest : Assertion {
		
		[SetUp]
		public void GetReady() {}

		[TearDown]
		public void Clean() {}

		[Test]
		public void TestDefaultValues () {
			DataGridViewBand band = new DataGridViewBand();
			// AssertEquals("ContextMenuStrip property", null, band.ContextMenuStrip);
			AssertEquals("Frozen", false, band.Frozen);
			AssertEquals("HasDefaultCellStyle property", false, band.HasDefaultCellStyle);
			AssertEquals("Index property", -1, band.Index);
			AssertEquals("InheritedStyle property", null, band.InheritedStyle);
			AssertEquals("ReadOnly property", false, band.ReadOnly);
			AssertEquals("Resizable property", DataGridViewTriState.True, band.Resizable);
			AssertEquals("Selected property", false, band.Selected);
			AssertEquals("Tag property", null, band.Tag);
			AssertEquals("Visible property", true, band.Visible);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void TestSelectedInvalidOperationException () {
			DataGridViewBand band = new DataGridViewBand();
			band.Selected = true;
		}

	}

}

#endif
