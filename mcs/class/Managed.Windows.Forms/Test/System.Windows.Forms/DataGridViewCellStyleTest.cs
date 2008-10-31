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
using System.Threading;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DataGridViewCellStyleTest  : TestHelper {

		DataGridViewCellStyle style;
		
		[SetUp]
		protected override void SetUp () {
			style = new DataGridViewCellStyle();
			base.SetUp ();
		}

		[Test]
		public void TestDefaultValues () {
			Assert.AreEqual (DataGridViewContentAlignment.NotSet, style.Alignment, "#A1");
			Assert.AreEqual (Color.Empty, style.BackColor, "#A2");
			Assert.AreEqual (null, style.Font, "#A3");
			Assert.AreEqual (Color.Empty, style.ForeColor, "#A4");
			Assert.AreEqual (String.Empty, style.Format, "#A5");
			Assert.AreEqual (true, style.IsNullValueDefault, "#A8");
			Assert.AreEqual (string.Empty, style.NullValue, "#A9");
			Assert.AreEqual (Color.Empty, style.SelectionBackColor, "#A10");
			Assert.AreEqual (Color.Empty, style.SelectionForeColor, "#A11");
			Assert.AreEqual (null, style.Tag, "#A12");
			Assert.AreEqual (DataGridViewTriState.NotSet, style.WrapMode, "#A13");
		}

		[Test]
		public void TestApplyStyle () {
			DataGridViewCellStyle style_aux = new DataGridViewCellStyle();
			style.ApplyStyle(style_aux);
			Assert.AreEqual (style_aux, style, "#B1");
		}

		[Test]
		public void TestClone () {
			DataGridViewCellStyle style_aux = (DataGridViewCellStyle) style.Clone();
			Assert.AreEqual (style_aux, style, "#C1");
		}

		[Test]
		public void TestEquals () {
			DataGridViewCellStyle style_aux = (DataGridViewCellStyle) style.Clone();
			Assert.AreEqual (true, (style_aux.Equals(style)), "#D1");
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestAlignmentInvalidEnumArgumentException () {
			style.Alignment = DataGridViewContentAlignment.BottomCenter | DataGridViewContentAlignment.BottomRight;
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestWrapModeInvalidEnumArgumentException () {
			style.WrapMode = (DataGridViewTriState) 3;
		}

		[Test]
		public void FormatProvider ()
		{
			CultureInfo orignalCulture = CultureInfo.CurrentCulture;
			CultureInfo orignalUICulture = CultureInfo.CurrentUICulture;

			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("nl-BE");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo ("ja-JP");
				Assert.AreSame (CultureInfo.CurrentCulture, style.FormatProvider, "#1");
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("fr-FR");
				Assert.AreSame (CultureInfo.CurrentCulture, style.FormatProvider, "#2");
				style.FormatProvider = CultureInfo.CurrentCulture;
				Assert.AreSame (CultureInfo.CurrentCulture, style.FormatProvider, "#3");
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
				Assert.AreEqual (new CultureInfo ("fr-FR"), style.FormatProvider, "#4");
				style.FormatProvider = null;
				Assert.AreSame (CultureInfo.CurrentCulture, style.FormatProvider, "#5");
			} finally {
				Thread.CurrentThread.CurrentCulture = orignalCulture;
				Thread.CurrentThread.CurrentUICulture = orignalUICulture;
			}
		}

		[Test]
		public void IsFormatProviderDefault ()
		{
			CultureInfo orignalCulture = CultureInfo.CurrentCulture;

			try {
				Assert.IsTrue (style.IsFormatProviderDefault, "#1");
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("nl-BE");
				Assert.IsTrue (style.IsFormatProviderDefault, "#2");
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("fr-FR");
				Assert.IsTrue (style.IsFormatProviderDefault, "#3");
				style.FormatProvider = CultureInfo.CurrentCulture;
				Assert.IsFalse (style.IsFormatProviderDefault, "#4");
				style.FormatProvider = new CultureInfo ("en-US");
				Assert.IsFalse (style.IsFormatProviderDefault, "#5");
				style.FormatProvider = null;
				Assert.IsTrue (style.IsFormatProviderDefault, "#6");
			} finally {
				Thread.CurrentThread.CurrentCulture = orignalCulture;
			}
		}
	}
}

#endif
