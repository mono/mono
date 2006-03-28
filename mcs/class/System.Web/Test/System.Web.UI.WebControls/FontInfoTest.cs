//
// Tests for System.Web.UI.WebControls.FontInfo.cs 
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
//

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

using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class FontInfoTest {
		private void SetSomeValues(Style s) {
			s.BackColor = Color.Red;
			s.Width = new Unit(10);
			s.Font.Bold = true;
		}

		[Test]
		public void Style_Defaults ()
		{
			Style s = new Style ();

			Assert.AreEqual (s.BackColor, Color.Empty, "Default1");
			Assert.AreEqual (s.BorderColor, Color.Empty, "Default22");
			Assert.AreEqual (s.BorderStyle, BorderStyle.NotSet, "Default3");
			Assert.AreEqual (s.BorderWidth, Unit.Empty, "Default4");
			Assert.AreEqual (s.CssClass, string.Empty, "Default5");
			Assert.AreEqual (s.ForeColor, Color.Empty, "Default6");
			Assert.AreEqual (s.Height, Unit.Empty, "Default7");
			Assert.AreEqual (s.Width, Unit.Empty, "Default8");
		}

		[Test]
		public void ShouldSerializeNames ()
		{
			Style style = new Style ();
			FontInfo fontInfo = style.Font;
			Assert.IsFalse (fontInfo.ShouldSerializeNames ());
			fontInfo.Name = "Verdana";
			Assert.IsTrue (fontInfo.ShouldSerializeNames ());
			fontInfo.Name = String.Empty;
			Assert.IsFalse (fontInfo.ShouldSerializeNames ());
		}

		[Test]
		public void Style_Merge ()
		{
		}

		[Test]
		public void Style_Copy ()
		{
			Style s = new Style ();
			Style copy = new Style ();

			SetSomeValues(s);

			copy.CopyFrom (s);
			Assert.AreEqual (s.BackColor, Color.Red, "Copy1");
		}
	}
}

		
