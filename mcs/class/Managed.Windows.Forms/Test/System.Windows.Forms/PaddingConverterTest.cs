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
// Copyright (c) 2006 Novell, Inc.
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
	public class PaddingConverterTest
	{
		[Test]
		public void CanConvertFrom ()
		{
			PaddingConverter c = new PaddingConverter ();

			Assert.IsTrue (c.CanConvertFrom (null, typeof (string)), "1");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (int)), "2");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (float)), "3");
			Assert.IsFalse (c.CanConvertFrom (null, typeof (object)), "4");
		}

		[Test]
		public void CanConvertTo ()
		{
			PaddingConverter c = new PaddingConverter ();

			Assert.IsTrue (c.CanConvertTo (null, typeof (string)), "1");
			Assert.IsFalse (c.CanConvertTo (null, typeof (int)), "2");
			Assert.IsFalse (c.CanConvertTo (null, typeof (float)), "3");
			Assert.IsFalse (c.CanConvertTo (null, typeof (object)), "4");
		}
		
		[Test]
		public void RoundTrip ()
		{
			Padding p1 = new Padding (1, 2, 3, 4);
			Padding p2 = new Padding (1);
			Padding p3 = new Padding ();

			Assert.AreEqual (p1, RoundTripPadding (p1), "B1");
			Assert.AreEqual (p2, RoundTripPadding (p2), "B2");
			Assert.AreEqual (p3, RoundTripPadding (p3), "B3");
			
		}
		
		private Padding RoundTripPadding (Padding p)
		{
			PaddingConverter pc = new PaddingConverter ();
			
			string s = (string)pc.ConvertTo (p, typeof (string));
			return (Padding)pc.ConvertFrom (s);
		}
		
		[Test]
		public void CreateInstanceSupported ()
		{
			PaddingConverter pc = new PaddingConverter ();
			
			Assert.AreEqual (true, pc.GetCreateInstanceSupported (null), "A1");
			Assert.AreEqual (true, pc.GetPropertiesSupported (null), "A2");
		}
	}
}
#endif