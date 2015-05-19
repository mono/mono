//
// System.Configuration.InfiniteTimeSpanConverterTest.cs - Unit tests
// for System.Configuration.InfiniteTimeSpanConverter.
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


using System;
using System.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class InfiniteTimeSpanConverterTest
	{
		[Test]
		public void CanConvertFrom ()
		{
			InfiniteTimeSpanConverter cv = new InfiniteTimeSpanConverter ();

			Assert.IsTrue (cv.CanConvertFrom (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (object)), "A4");
		}

		[Test]
		public void CanConvertTo ()
		{
			InfiniteTimeSpanConverter cv = new InfiniteTimeSpanConverter ();

			Assert.IsTrue (cv.CanConvertTo (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (object)), "A4");
		}

		[Test]
		public void ConvertFrom ()
		{
			InfiniteTimeSpanConverter cv = new InfiniteTimeSpanConverter ();
			object o;

			o = cv.ConvertFrom (null, null, "00:00:59");
			Assert.AreEqual (typeof (TimeSpan), o.GetType(), "A1");
			Assert.AreEqual (TimeSpan.FromSeconds (59), o, "A2");

			/* and now test infinity */
			o = cv.ConvertFrom (null, null, "Infinite");
			Assert.AreEqual (TimeSpan.MaxValue, o, "A3");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertFrom_TypeError ()
		{
			InfiniteTimeSpanConverter cv = new InfiniteTimeSpanConverter ();
			object o;

			o = cv.ConvertFrom (null, null, 59);
			Assert.IsNull (o, "A1");
		}

		[Test]
		public void ConvertTo ()
		{
			InfiniteTimeSpanConverter cv = new InfiniteTimeSpanConverter ();

			Assert.AreEqual ("00:00:59", cv.ConvertTo (null, null, TimeSpan.FromSeconds (59), typeof (string)), "A1");

			Assert.AreEqual ("00:02:24", cv.ConvertTo (null, null, TimeSpan.FromSeconds (144), typeof (string)), "A2");
			
			/* infinity tests */
			Assert.AreEqual ("Infinite", cv.ConvertTo (null, null, TimeSpan.MaxValue, typeof (string)), "A3");
			Assert.AreEqual ("10675199.02:48:04.4775807", cv.ConvertTo (null, null, TimeSpan.MaxValue - TimeSpan.FromSeconds (1), typeof (string)), "A4");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConvertTo_NullError ()
		{
			InfiniteTimeSpanConverter cv = new InfiniteTimeSpanConverter ();

			Assert.AreEqual ("", cv.ConvertTo (null, null, null, typeof (string)), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertTo_TypeError1 ()
		{
			InfiniteTimeSpanConverter cv = new InfiniteTimeSpanConverter ();

			Assert.AreEqual ("59", cv.ConvertTo (null, null, "hi", typeof (string)), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertTo_TypeError2 ()
		{
			InfiniteTimeSpanConverter cv = new InfiniteTimeSpanConverter ();

			Assert.AreEqual ("00:00:59", cv.ConvertTo (null, null, 59, typeof (int)), "A1");
		}

	}
}

