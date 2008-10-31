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
// Copyright (c) 2007 Novell, Inc.
//

#if NET_2_0

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TreeViewImageIndexConverterTest : TestHelper
	{
		[Test]
		public void PropertyIncludeNoneAsStandardValue ()
		{
			PublicImageIndexConverter c = new PublicImageIndexConverter ();
			Assert.AreEqual (false, c.PublicIncludeNoneAsStandardValue, "A1");
		}

		private class PublicImageIndexConverter : TreeViewImageIndexConverter
		{
			public bool PublicIncludeNoneAsStandardValue { get { return base.IncludeNoneAsStandardValue; } }
		}
		
		[Test]
		public void GetStandardValues ()
		{
			TreeViewImageIndexConverter c = new TreeViewImageIndexConverter ();
			Assert.AreEqual (2, c.GetStandardValues (null).Count, "B1");
			Assert.AreEqual ("-1", c.GetStandardValues (null)[0].ToString (), "B2");
			Assert.AreEqual ("-2", c.GetStandardValues (null)[1].ToString (), "B3");
		}
		
		[Test]
		public void ConvertFrom ()
		{
			TreeViewImageIndexConverter iic = new TreeViewImageIndexConverter ();
			Assert.AreEqual (-1, iic.ConvertFrom (null, CultureInfo.CurrentCulture, "(default)"), "N1");
			Assert.AreEqual (-1, iic.ConvertFrom (null, CultureInfo.CurrentCulture, "(DEFAULT)"), "N1-1");
			Assert.AreEqual (-2, iic.ConvertFrom (null, CultureInfo.CurrentCulture, "(none)"), "N2");
			Assert.AreEqual (-2, iic.ConvertFrom (null, CultureInfo.CurrentCulture, "(nONE)"), "N2-1");
			Assert.AreEqual (0, iic.ConvertFrom (null, CultureInfo.CurrentCulture, "0"), "N3");
			Assert.AreEqual (6, iic.ConvertFrom (null, CultureInfo.CurrentCulture, "6"), "N4");
			Assert.AreEqual (-1, iic.ConvertFrom (null, CultureInfo.CurrentCulture, "-1"), "N5");
			Assert.AreEqual (-2, iic.ConvertFrom (null, CultureInfo.CurrentCulture, "-2"), "N6");
		}
		
		[Test]
		public void ConvertTo ()
		{
			TreeViewImageIndexConverter iic = new TreeViewImageIndexConverter ();
			Assert.AreEqual (string.Empty, iic.ConvertTo (null, null, string.Empty, typeof (string)), "N1");
			Assert.AreEqual (string.Empty, iic.ConvertTo (null, null, null, typeof (string)), "N2");
			Assert.AreEqual ("6", iic.ConvertTo (null, null, 6, typeof (string)), "N3");
			Assert.AreEqual ("(default)", iic.ConvertTo (null, null, -1, typeof (string)), "N4");
			Assert.AreEqual ("(none)", iic.ConvertTo (null, null, -2, typeof (string)), "N5");
		}
	}
}
#endif
