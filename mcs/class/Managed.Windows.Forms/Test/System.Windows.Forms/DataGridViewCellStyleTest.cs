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
using System.Globalization;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DataGridViewCellStyleTest : Assertion {

		DataGridViewCellStyle style;
		
		[SetUp]
		public void GetReady() {
			style = new DataGridViewCellStyle();
		}

		[TearDown]
		public void Clean() {
			style = new DataGridViewCellStyle();
		}

		[Test]
		public void TestDefaultValues () {
			AssertEquals("Alignment property", DataGridViewContentAlignment.NotSet, style.Alignment);
			AssertEquals("BackColor property", Color.Empty, style.BackColor);
			AssertEquals("Font property", null, style.Font);
			AssertEquals("ForeColor property", Color.Empty, style.ForeColor);
			AssertEquals("Format property", String.Empty, style.Format);
			AssertEquals("FormatProvider property", CultureInfo.CurrentUICulture, style.FormatProvider);
			AssertEquals("IsFormatProviderDefault property", true, style.IsFormatProviderDefault);
			AssertEquals("IsNullValueDefault property", true, style.IsNullValueDefault);
			AssertEquals("NullValue property", "(null)", style.NullValue);
			AssertEquals("SelectionBackColor property", Color.Empty, style.SelectionBackColor);
			AssertEquals("SelectionForeColor property", Color.Empty, style.SelectionForeColor);
			AssertEquals("Tag property", null, style.Tag);
			AssertEquals("WrapMode property", DataGridViewTriState.NotSet, style.WrapMode);
		}

		[Test]
		public void TestApplyStyle () {
			DataGridViewCellStyle style_aux = new DataGridViewCellStyle();
			style.ApplyStyle(style_aux);
			AssertEquals("ApplyStyle method", style_aux, style);
		}

		[Test]
		public void TestClone () {
			DataGridViewCellStyle style_aux = (DataGridViewCellStyle) style.Clone();
			AssertEquals("Clone method", style_aux, style);
		}

		[Test]
		public void TestEquals () {
			DataGridViewCellStyle style_aux = (DataGridViewCellStyle) style.Clone();
			AssertEquals("Equals method", true, (style_aux.Equals(style)));
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestAlignmentInvalidEnumArgumentException () {
			style.Alignment = DataGridViewContentAlignment.BottomCenter | DataGridViewContentAlignment.BottomRight;
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestSelectionBackColorArgumentException () {
			style.SelectionBackColor = Color.FromArgb(100, Color.Red);
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestWrapModeInvalidEnumArgumentException () {
			style.WrapMode = (DataGridViewTriState) 3;
		}

		/*
		[Test]
		[ExpectedException(typeof(Exception))]
		public void TestException () {
			ConcreteCollection myCollection;
			myCollection = new ConcreteCollection();
			....
			AssertEquals ("#UniqueID", expected, actual);
			....
			Fail ("Message");
		}
		*/

	}

}

#endif
