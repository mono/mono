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
	public class SizeConverterTest
	{
		[Test]
		public void CanConvertFrom ()
		{
			SizeConverter r = new SizeConverter ();

			Assert.IsTrue (r.CanConvertFrom (typeof (string)));
			Assert.IsFalse (r.CanConvertFrom (typeof (Size)));
		}

		[Test]
		public void CanConvertTo ()
		{
			SizeConverter r = new SizeConverter ();

			Assert.IsTrue (r.CanConvertTo (typeof (string)));
			Assert.IsFalse (r.CanConvertTo (typeof (Size)));
		}

		[Test]
		public void ConvertFrom ()
		{
			SizeConverter r = new SizeConverter ();

			object or = r.ConvertFrom ("3, 4");
			
			Assert.AreEqual (typeof (Size), or.GetType());
			Assert.AreEqual (new Size (3, 4), or);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_size ()
		{
			SizeConverter r = new SizeConverter ();

			r.ConvertFrom (new Size (10, 20));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFrom_negative ()
		{
			SizeConverter r = new SizeConverter ();
			r.ConvertFrom ("-1, -4");
		}

		[Test]
		public void ConvertTo ()
		{
			SizeConverter r = new SizeConverter ();

			Size rect = new Size (1, 2);

			object o = r.ConvertTo (null, CultureInfo.InvariantCulture, rect, typeof (string));
			
			Assert.AreEqual (typeof (string), o.GetType());
			Assert.AreEqual ("1,2", (string)o);
		}
	}

}
