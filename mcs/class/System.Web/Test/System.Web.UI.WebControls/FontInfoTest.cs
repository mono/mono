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

		[Test]
		public void FontInfo_CopyFrom () {

			//Methods CopyFrom and MergeWith behave differently between 1.1 and 2.0
			Style s = new Style ();
			Style copy = new Style ();

			s.Font.Bold = true;
			s.Font.Underline = false;

			copy.Font.Italic = true;
			copy.Font.Underline = true;

			copy.Font.CopyFrom (s.Font);

			Assert.AreEqual (true, copy.Font.Italic, "CopyFrom#1");
			Assert.AreEqual (true, copy.Font.Bold, "CopyFrom#2");
#if NET_2_0
			Assert.AreEqual (false, copy.Font.Underline, "CopyFrom#3");
#else
			Assert.AreEqual (true, copy.Font.Underline, "CopyFrom#3");
#endif
		}
		
		[Test]
		public void FontInfo_MergeWith () {

			//Methods CopyFrom and MergeWith behave differently between 1.1 and 2.0
			Style s = new Style ();
			Style copy = new Style ();

			s.Font.Overline = false;
			s.Font.Bold = true;
			s.Font.Underline = true;

			copy.Font.Italic = true;
			copy.Font.Underline = false;

			copy.Font.MergeWith (s.Font);

			Assert.AreEqual (true, copy.Font.Italic, "MergeWith#1");
			Assert.AreEqual (true, copy.Font.Bold, "MergeWith#2");
			Assert.AreEqual (false, copy.Font.Underline, "MergeWith#3");
			Assert.AreEqual (false, copy.Font.Overline, "MergeWith#4");

			Style copy2 = new Style ();
			copy2.Font.Overline = true;
			copy2.Font.CopyFrom (copy.Font);

#if NET_2_0
			Assert.AreEqual (false, copy2.Font.Overline, "MergeWith#5");
#else
			Assert.AreEqual (true, copy2.Font.Overline, "MergeWith#5");
#endif
		}

		[Test]
		public void FontInfo_Names () {
			Style s = new Style ();
			Assert.AreEqual ("", s.Font.Name, "Names#1");
			Assert.IsNotNull (s.Font.Names, "Names#2");
			Assert.AreEqual (0, s.Font.Names.Length, "Names#3");

			s.Font.Names = new string [] { "Arial", "Veranda" };
			Assert.AreEqual ("Arial", s.Font.Name, "Names#4");

			s.Font.Names = null;
			Assert.AreEqual ("", s.Font.Name, "Names#5");
			Assert.IsNotNull (s.Font.Names, "Names#6");
			Assert.AreEqual (0, s.Font.Names.Length, "Names#7");

			s.Font.Name = "Arial";
			Assert.IsNotNull (s.Font.Names, "Names#8");
			Assert.AreEqual (1, s.Font.Names.Length, "Names#9");
			Assert.AreEqual ("Arial", s.Font.Names [0], "Names#10");

			s.Font.Name = "";
			Assert.AreEqual ("", s.Font.Name, "Names#11");
			Assert.IsNotNull (s.Font.Names, "Names#12");
			Assert.AreEqual (0, s.Font.Names.Length, "Names#13");
		}
	}
}

		
