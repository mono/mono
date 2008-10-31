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
	public class LinkConverterTest : TestHelper
	{
		[Test]
		public void CanConvertFrom ()
		{
			LinkConverter c = new LinkConverter ();

			Assert.AreEqual (true, c.CanConvertFrom (null, typeof (string)), "1");
			Assert.AreEqual (false, c.CanConvertFrom (null, typeof (int)), "2");
			Assert.AreEqual (false, c.CanConvertFrom (null, typeof (float)), "3");
			Assert.AreEqual (false, c.CanConvertFrom (null, typeof (object)), "4");
			Assert.AreEqual (false, c.CanConvertFrom (null, typeof (LinkLabel)), "5");
			Assert.AreEqual (false, c.CanConvertFrom (null, typeof (LinkLabel.Link)), "6");
		}

		[Test]
		public void CanConvertTo ()
		{
			LinkConverter c = new LinkConverter ();

			Assert.AreEqual (true, c.CanConvertTo (null, typeof (string)), "A1");
			Assert.AreEqual (false, c.CanConvertTo (null, typeof (int)), "A2");
			Assert.AreEqual (false, c.CanConvertTo (null, typeof (float)), "A3");
			Assert.AreEqual (false, c.CanConvertTo (null, typeof (object)), "A4");
			Assert.AreEqual (false, c.CanConvertTo (null, typeof (LinkLabel)), "A5");
			Assert.AreEqual (false, c.CanConvertTo (null, typeof (LinkLabel.Link)), "A6");
		}

		[Test]
		public void ConvertFrom ()
		{
			LinkConverter lc = new LinkConverter ();
			Assert.AreEqual (null, lc.ConvertFrom (null, null, string.Empty), "N1");
			Assert.AreEqual (new LinkLabel.Link ().Start, ((LinkLabel.Link)lc.ConvertFrom (null, null, string.Format ("0{0} 0", CultureInfo.CurrentCulture.TextInfo.ListSeparator))).Start, "N2");
			Assert.AreEqual (new LinkLabel.Link (-1, 8).Start, ((LinkLabel.Link)lc.ConvertFrom (null, null, string.Format ("-1{0} 8", CultureInfo.CurrentCulture.TextInfo.ListSeparator))).Start, "N3");
			Assert.AreEqual (new LinkLabel.Link (12, 24).Start, ((LinkLabel.Link)lc.ConvertFrom (null, null, string.Format ("12{0} 24", CultureInfo.CurrentCulture.TextInfo.ListSeparator))).Start, "N4");
			Assert.AreEqual (new LinkLabel.Link ().Length, ((LinkLabel.Link)lc.ConvertFrom (null, null, string.Format ("0{0} 0", CultureInfo.CurrentCulture.TextInfo.ListSeparator))).Length, "N5");
			Assert.AreEqual (new LinkLabel.Link (-1, 8).Length, ((LinkLabel.Link)lc.ConvertFrom (null, null, string.Format ("-1{0} 8", CultureInfo.CurrentCulture.TextInfo.ListSeparator))).Length, "N6");
			Assert.AreEqual (new LinkLabel.Link (12, 24).Length, ((LinkLabel.Link)lc.ConvertFrom (null, null, string.Format ("12{0} 24", CultureInfo.CurrentCulture.TextInfo.ListSeparator))).Length, "N7");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromNSE ()
		{
			LinkConverter lc = new LinkConverter ();
			Assert.AreEqual (new LinkLabel.Link (), lc.ConvertFrom (null, null, null), "N2");
		}
		
		[Test]
		public void ConvertTo ()
		{
			LinkConverter lc = new LinkConverter ();
			Assert.AreEqual (string.Empty, lc.ConvertTo (null, null, null, typeof (string)), "N1");
			Assert.AreEqual (string.Format ("0{0} 0", CultureInfo.CurrentCulture.TextInfo.ListSeparator), lc.ConvertTo (null, null, new LinkLabel.Link (), typeof (string)), "N2");
			Assert.AreEqual (string.Format ("0{0} 7", CultureInfo.CurrentCulture.TextInfo.ListSeparator), lc.ConvertTo (null, null, new LinkLabel.Link (0, 7), typeof (string)), "N3");
			Assert.AreEqual (string.Format ("5{0} 12", CultureInfo.CurrentCulture.TextInfo.ListSeparator), lc.ConvertTo (null, null, new LinkLabel.Link (5, 12, "mydata"), typeof (string)), "N4");
		}
	}
}
#endif
