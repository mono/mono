//
// System.Drawing.FontFamily unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	public class FontFamilyTest {

		private Bitmap bitmap;
		private Graphics graphic;
		private string name;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			bitmap = new Bitmap (10, 10);
			graphic = Graphics.FromImage (bitmap);
			try {
				using (FontFamily ff = new FontFamily (GenericFontFamilies.Monospace)) {
					name = ff.Name;
				}
			}
			catch (ArgumentException) {
				Assert.Ignore ("No font family could be found.");
			}
		}

		[Test]
		public void FontFamily_String_Null ()
		{
			Assert.Throws<ArgumentException> (() => new FontFamily (null));
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus/fontconfig always return something
		public void FontFamily_String_Empty ()
		{
			Assert.Throws<ArgumentException> (() => new FontFamily (String.Empty));
		}

		private void CheckMono (FontFamily ff)
		{
			Assert.IsTrue (ff.Equals (FontFamily.GenericMonospace), "GenericMonospace");
			// note: Mono has this behaviour on both 1.1 and 2.0 profiles
			Assert.AreEqual (ff.Name.GetHashCode (), ff.GetHashCode (), "GetHashCode");
		}

		[Test]
		public void FontFamily_String ()
		{
			HostIgnoreList.CheckTest ("MonoTests.System.Drawing.FontFamilyTest.FontFamily_String");
			FontFamily ff = new FontFamily (name);
			CheckMono (ff);
			FontStyle style = FontStyle.Bold;
			Assert.AreEqual (ff.Name, ff.GetName (0), "GetName");
			Assert.IsTrue ((ff.GetCellAscent (style) > 0), "GetCellAscent");
			Assert.IsTrue ((ff.GetCellDescent (style) > 0), "GetCellDescent");
			Assert.IsTrue ((ff.GetEmHeight (style) > 0), "GetEmHeight");
			Assert.IsTrue ((ff.GetLineSpacing (style) > 0), "GetLineSpacing");
			Assert.IsTrue (ff.IsStyleAvailable (style), "IsStyleAvailable");
		}

		[Test]
		public void FontFamily_String_FontCollection_Null ()
		{
			FontFamily ff = new FontFamily (name, null);
			CheckMono (ff);
		}

		[Test]
		public void FontFamily_String_InstalledFontCollection ()
		{
			FontFamily ff = new FontFamily (name, new InstalledFontCollection ());
			CheckMono (ff);
		}

		[Test]
		public void FontFamily_String_PrivateFontCollection ()
		{
			Assert.Throws<ArgumentException> (() => new FontFamily (name, new PrivateFontCollection ()));
		}

		[Test]
		public void FontFamily_Monospace ()
		{
			FontFamily ff = new FontFamily (GenericFontFamilies.Monospace);
			CheckMono (ff);
		}

		[Test]
		public void FontFamily_SansSerif ()
		{
			FontFamily ff = new FontFamily (GenericFontFamilies.SansSerif);
			Assert.IsTrue (ff.Equals (FontFamily.GenericSansSerif), "GenericSansSerif");
			// note: Mono has this behaviour on both 1.1 and 2.0 profiles
			Assert.AreEqual (ff.Name.GetHashCode (), ff.GetHashCode (), "GetHashCode");
		}

		[Test]
		public void FontFamily_Serif ()
		{
			FontFamily ff = new FontFamily (GenericFontFamilies.Serif);
			Assert.IsTrue (ff.Equals (FontFamily.GenericSerif), "GenericSerif");
			// note: Mono has this behaviour on both 1.1 and 2.0 profiles
			Assert.AreEqual (ff.Name.GetHashCode (), ff.GetHashCode (), "GetHashCode");
		}

		[Test]
		public void FontFamily_Invalid ()
		{
			FontFamily ff = new FontFamily ((GenericFontFamilies)Int32.MinValue);
			// default to Monospace
			Assert.IsTrue (ff.Equals (FontFamily.GenericMonospace), "GenericMonospace");
			CheckMono (ff);
		}

		[Test]
		public void GenericMonospace ()
		{
			FontFamily ff = FontFamily.GenericMonospace;
			string ts = ff.ToString ();
			Assert.AreEqual ('[', ts[0], "[");
			Assert.IsTrue ((ts.IndexOf (name) >= 0), "ToString");
			Assert.AreEqual (']', ts[ts.Length - 1], "]");
		}

		[Test]
		public void GenericSansSerif ()
		{
			FontFamily ff = FontFamily.GenericSansSerif;
			string name = ff.Name;
			ff.Dispose ();
			Assert.AreEqual (name, FontFamily.GenericSansSerif.Name, "Name");
		}

		[Test]
		public void GenericSerif ()
		{
			FontFamily ff = FontFamily.GenericSerif;
			string name = ff.Name;
			ff.Dispose ();
			Assert.AreEqual (name, FontFamily.GenericSerif.Name, "Name");
		}

		[Test]
		public void GetFamilies_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => FontFamily.GetFamilies (null));
		}

		[Test]
		public void GetFamilies ()
		{
			FontFamily[] ffc = FontFamily.GetFamilies (graphic);
			Assert.AreEqual (ffc.Length, FontFamily.Families.Length, "Length");
		}

		[Test]
		public void Dispose_Double ()
		{
			FontFamily ff = FontFamily.GenericSerif;
			ff.Dispose ();
			ff.Dispose ();
		}

		[Test]
		public void Dispose_UseAfter ()
		{
			FontFamily ff = FontFamily.GenericMonospace;
			ff.Dispose ();
			Assert.Throws<ArgumentException> (() => { var x = ff.Name; });
		}
	}
}
