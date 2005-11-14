//
// System.Configuration.CommaDelimitedStringCollectionConverterTest.cs - Unit tests
// for System.Configuration.CommaDelimitedStringCollectionConverter.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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

#if NET_2_0

using System;
using System.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class CommaDelimitedStringCollectionConverterTest
	{
		[Test]
		public void CanConvertFrom ()
		{
			CommaDelimitedStringCollectionConverter cv = new CommaDelimitedStringCollectionConverter ();

			Assert.IsTrue (cv.CanConvertFrom (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (object)), "A4");
		}

		[Test]
		public void CanConvertTo ()
		{
			CommaDelimitedStringCollectionConverter cv = new CommaDelimitedStringCollectionConverter ();

			Assert.IsTrue (cv.CanConvertTo (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (object)), "A4");
		}

		[Test]
		public void ConvertFrom ()
		{
			CommaDelimitedStringCollectionConverter cv = new CommaDelimitedStringCollectionConverter ();
			object o;
			CommaDelimitedStringCollection col;

			o = cv.ConvertFrom (null, null, "hi,bye");
			Assert.AreEqual (typeof (CommaDelimitedStringCollection), o.GetType(), "A1");

			col = (CommaDelimitedStringCollection)o;
			Assert.AreEqual (2, col.Count, "A2");
			Assert.AreEqual ("hi", col[0], "A3");
			Assert.AreEqual ("bye", col[1], "A4");

			col = (CommaDelimitedStringCollection)cv.ConvertFrom (null, null, "hi, bye");
			Assert.AreEqual (2, col.Count, "A5");
			Assert.AreEqual ("hi", col[0], "A6");
			Assert.AreEqual ("bye", col[1], "A7");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertFrom_TypeError ()
		{
			CommaDelimitedStringCollectionConverter cv = new CommaDelimitedStringCollectionConverter ();
			object o;

			o = cv.ConvertFrom (null, null, 59);
			Assert.IsNull (o, "A1");
		}

		[Test]
		public void ConvertTo ()
		{
			CommaDelimitedStringCollectionConverter cv = new CommaDelimitedStringCollectionConverter ();
			CommaDelimitedStringCollection col = new CommaDelimitedStringCollection();
			col.Add ("hi");
			col.Add ("bye");

			Assert.AreEqual ("hi,bye", cv.ConvertTo (null, null, col, typeof (string)), "A1");
		}

		[Test]
		public void ConvertTo_NullError ()
		{
			CommaDelimitedStringCollectionConverter cv = new CommaDelimitedStringCollectionConverter ();

			Assert.AreEqual (null, cv.ConvertTo (null, null, null, typeof (string)), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertTo_TypeError ()
		{
			CommaDelimitedStringCollectionConverter cv = new CommaDelimitedStringCollectionConverter ();

			Assert.AreEqual ("59", cv.ConvertTo (null, null, 59, typeof (string)), "A1");
		}
	}
}

#endif
