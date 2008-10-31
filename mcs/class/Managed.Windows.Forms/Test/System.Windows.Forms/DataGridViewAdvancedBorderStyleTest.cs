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
// Copyright (c) 2005,2006 Novell, Inc. (http://www.novell.com)
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
	public class DataGridViewAdvancedBorderStyleTest : TestHelper {

		private DataGridViewAdvancedBorderStyle style;

		
		[SetUp]
		protected override void SetUp ()	{
			style = new DataGridViewAdvancedBorderStyle();
			base.SetUp ();
		}

		[Test]
		public void TestDefaultValues () {
			Assert.AreEqual (DataGridViewAdvancedCellBorderStyle.None, style.All, "#A1");
			style.Left = DataGridViewAdvancedCellBorderStyle.Single;
			Assert.AreEqual (DataGridViewAdvancedCellBorderStyle.NotSet, style.All, "#A2");
			style.All = DataGridViewAdvancedCellBorderStyle.Single;
			Assert.AreEqual (DataGridViewAdvancedCellBorderStyle.Single, style.All, "#A3");
			Assert.AreEqual (DataGridViewAdvancedCellBorderStyle.Single, style.Left, "#A4");
			Assert.AreEqual (DataGridViewAdvancedCellBorderStyle.Single, style.Right, "#A5");
			Assert.AreEqual (DataGridViewAdvancedCellBorderStyle.Single, style.Top, "#A6");
			Assert.AreEqual (DataGridViewAdvancedCellBorderStyle.Single, style.Bottom, "#A7");
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestLeftInvalidEnumArgumentException () {
			style.Left = (DataGridViewAdvancedCellBorderStyle) 8;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestLeftArgumentException1 () {
			style.Left = DataGridViewAdvancedCellBorderStyle.NotSet;
		}

		/*
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestLeftArgumentException2 () {
			Control.RightToLeft = true;
			style.Left = DataGridViewAdvancedCellBorderStyle.InsetDouble;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestLeftArgumentException3 () {
			Control.RightToLeft = true;
			style.Left = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
		}
		*/

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestRightInvalidEnumArgumentException () {
			style.Right = (DataGridViewAdvancedCellBorderStyle) 8;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestRightArgumentException1 () {
			style.Right = DataGridViewAdvancedCellBorderStyle.NotSet;
		}

		/*
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestRightArgumentException2 () {
			Control.RightToLeft = false;
			style.Right = DataGridViewAdvancedCellBorderStyle.InsetDouble;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestRightArgumentException3 () {
			Control.RightToLeft = false;
			style.Right = DataGridViewAdvancedCellBorderStyle.OutsetDouble;
		}
		*/

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestTopInvalidEnumArgumentException () {
			style.Top = (DataGridViewAdvancedCellBorderStyle) 8;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestTopArgumentException () {
			style.Top = DataGridViewAdvancedCellBorderStyle.NotSet;
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestBottomInvalidEnumArgumentException () {
			style.Bottom = (DataGridViewAdvancedCellBorderStyle) 8;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestBottomArgumentException () {
			style.Bottom = DataGridViewAdvancedCellBorderStyle.NotSet;
		}
	}
}

#endif
