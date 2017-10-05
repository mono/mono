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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class Int32RectConverterTest
	{
		[Test]
		public void CanConvertFrom ()
		{
			Int32RectConverter r = new Int32RectConverter ();

			Assert.IsTrue (r.CanConvertFrom (typeof (string)));
			Assert.IsFalse (r.CanConvertFrom (typeof (Int32Rect)));
			Assert.IsFalse (r.CanConvertFrom (typeof (Size)));
		}

		[Test]
		public void CanConvertTo ()
		{
			Int32RectConverter r = new Int32RectConverter ();

			Assert.IsTrue (r.CanConvertTo (typeof (string)));
			Assert.IsFalse (r.CanConvertTo (typeof (Int32Rect)));
			Assert.IsFalse (r.CanConvertTo (typeof (Size)));
		}

		[Test]
		public void ConvertFrom ()
		{
			Int32RectConverter r = new Int32RectConverter ();

			object or = r.ConvertFrom ("1, 2, 3, 4");
			
			Assert.AreEqual (typeof (Int32Rect), or.GetType());
			Assert.AreEqual (new Int32Rect (1, 2, 3, 4), or);

			or = r.ConvertFrom ("Empty");
			Assert.AreEqual (typeof (Int32Rect), or.GetType());
			Assert.IsTrue (((Int32Rect)or).IsEmpty);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_size ()
		{
			Int32RectConverter r = new Int32RectConverter ();

			r.ConvertFrom (new Size (10, 20));
		}

		[Test]
		public void ConvertFrom_negative ()
		{
			Int32RectConverter r = new Int32RectConverter ();
			object or = r.ConvertFrom ("1, 2, -4, -5");

			Assert.AreEqual (typeof (Int32Rect), or.GetType());
			Assert.AreEqual (new Int32Rect (1, 2, -4, -5), or);
		}

		[Test]
		public void ConvertTo ()
		{
			Int32RectConverter r = new Int32RectConverter ();

			Int32Rect rect = new Int32Rect (0, 0, 1, 2);

			object o = r.ConvertTo (null, CultureInfo.InvariantCulture, rect, typeof (string));
			
			Assert.AreEqual (typeof (string), o.GetType());
			Assert.AreEqual ("0,0,1,2", (string)o);
		}
	}

}
