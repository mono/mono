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
	public class ImageKeyConverterTest : TestHelper
	{
		[Test]
		public void PropertyIncludeNoneAsStandardValue ()
		{
			PublicImageKeyConverter c = new PublicImageKeyConverter ();
			Assert.AreEqual (true, c.PublicIncludeNoneAsStandardValue, "A1");
		}
		
		private class PublicImageKeyConverter : ImageKeyConverter
		{
			public bool PublicIncludeNoneAsStandardValue { get { return base.IncludeNoneAsStandardValue; } }
		}
		
		[Test]
		public void GetStandardValues ()
		{
			ImageKeyConverter c = new ImageKeyConverter ();
			Assert.AreEqual (1, c.GetStandardValues (null).Count, "B1");
			Assert.AreEqual (string.Empty, c.GetStandardValues (null)[0].ToString (), "B2");
		}

		[Test]
		public void GetStandardValuesExclusive ()
		{
			ImageKeyConverter c = new ImageKeyConverter ();
			Assert.AreEqual (true, c.GetStandardValuesExclusive (null), "C1");
		}

		[Test]
		public void GetStandardValuesSupported ()
		{
			ImageKeyConverter c = new ImageKeyConverter ();
			Assert.AreEqual (true, c.GetStandardValuesSupported (null), "D1");
		}
		
		[Test]
		public void CanConvertFrom ()
		{
			ImageKeyConverter c = new ImageKeyConverter ();

			Assert.IsTrue (c.CanConvertFrom (null, typeof (string)), "1");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (int)), "2");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (float)), "3");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (object)), "4");
		}
		
		[Test]
		public void ConvertFrom ()
		{
			ImageKeyConverter ikc = new ImageKeyConverter ();
			Assert.AreEqual (string.Empty, ikc.ConvertFrom (null, null, string.Empty), "N1");
			Assert.AreEqual (string.Empty, ikc.ConvertFrom (null, null, null), "N2");
			Assert.AreEqual ("(none)", ikc.ConvertFrom (null, null, "(none)"), "N2-1");
			Assert.AreEqual ("bob", ikc.ConvertFrom (null, null, "bob"), "N3");
			Assert.AreEqual ("oSCar", ikc.ConvertFrom (null, null, "oSCar"), "N4");
		}

		[Test]
		public void ConvertTo ()
		{
			ImageKeyConverter ikc = new ImageKeyConverter ();
			Assert.AreEqual ("(none)", ikc.ConvertTo (null, null, string.Empty, typeof (string)), "N1");
			Assert.AreEqual ("(none)", ikc.ConvertTo (null, null, null, typeof (string)), "N2");
			Assert.AreEqual ("(none)", ikc.ConvertTo (null, null, "(none)", typeof (string)), "N2-1");
			Assert.AreEqual ("bob", ikc.ConvertTo (null, null, "bob", typeof (string)), "N3");
			Assert.AreEqual ("oSCar", ikc.ConvertTo (null, null, "oSCar", typeof (string)), "N4");
		}
	}
}
#endif
