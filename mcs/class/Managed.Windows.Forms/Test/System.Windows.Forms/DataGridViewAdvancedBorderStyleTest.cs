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
	public class DataGridViewAdvancedBorderStyleTest : Assertion {

		private DataGridViewAdvancedBorderStyle style;
		
		[SetUp]
		public void GetReady() {
			style = new DataGridViewAdvancedBorderStyle();
		}

		[TearDown]
		public void Clean() {
		}

		[Test]
		public void TestDefaultValues () {
			AssertEquals("All property before any change", DataGridViewAdvancedCellBorderStyle.None, style.All);
			style.Left = DataGridViewAdvancedCellBorderStyle.Single;
			AssertEquals("All property after changes", DataGridViewAdvancedCellBorderStyle.NotSet, style.All);
			style.All = DataGridViewAdvancedCellBorderStyle.Single;
			AssertEquals("All property after changes", DataGridViewAdvancedCellBorderStyle.Single, style.All);
			AssertEquals("Left property after changes", DataGridViewAdvancedCellBorderStyle.Single, style.Left);
			AssertEquals("Right property after changes", DataGridViewAdvancedCellBorderStyle.Single, style.Right);
			AssertEquals("Top property after changes", DataGridViewAdvancedCellBorderStyle.Single, style.Top);
			AssertEquals("Bottom property after changes", DataGridViewAdvancedCellBorderStyle.Single, style.Bottom);
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
