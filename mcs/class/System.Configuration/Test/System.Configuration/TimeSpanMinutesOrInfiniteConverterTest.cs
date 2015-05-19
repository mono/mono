//
// System.Configuration.TimeSpanMinutesOrInfiniteConverterTest.cs - Unit tests
// for System.Configuration.TimeSpanMinutesOrInfiniteConverter.
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
	public class TimeSpanMinutesOrInfiniteConverterTest
	{
		[Test]
		public void CanConvertFrom ()
		{
			TimeSpanMinutesOrInfiniteConverter cv = new TimeSpanMinutesOrInfiniteConverter ();

			Assert.IsTrue (cv.CanConvertFrom (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (object)), "A4");
		}

		[Test]
		public void CanConvertTo ()
		{
			TimeSpanMinutesOrInfiniteConverter cv = new TimeSpanMinutesOrInfiniteConverter ();

			Assert.IsTrue (cv.CanConvertTo (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (object)), "A4");
		}

		[Test]
		public void ConvertFrom ()
		{
			TimeSpanMinutesOrInfiniteConverter cv = new TimeSpanMinutesOrInfiniteConverter ();
			object o;

			/* make sure the TimeSpanMinutesConverter tests work here too */
			o = cv.ConvertFrom (null, null, "59");
			Assert.AreEqual (typeof (TimeSpan), o.GetType(), "A1");
			Assert.AreEqual ("00:59:00", o.ToString(), "A2");
			o = cv.ConvertFrom (null, null, "104");
			Assert.AreEqual ("01:44:00", o.ToString(), "A3");

			/* and now test infinity */
			o = cv.ConvertFrom (null, null, "Infinite");
			Assert.AreEqual (TimeSpan.MaxValue.ToString(), o.ToString(), "A3");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFrom_FormatError ()
		{
			TimeSpanMinutesOrInfiniteConverter cv = new TimeSpanMinutesOrInfiniteConverter ();
			object o;

			o = cv.ConvertFrom (null, null, "100.5");
			Assert.IsNull (o, "A1");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertFrom_TypeError ()
		{
			TimeSpanMinutesOrInfiniteConverter cv = new TimeSpanMinutesOrInfiniteConverter ();
			object o;

			o = cv.ConvertFrom (null, null, 59);
			Assert.IsNull (o, "A1");
		}

		[Test]
		public void ConvertTo ()
		{
			TimeSpanMinutesOrInfiniteConverter cv = new TimeSpanMinutesOrInfiniteConverter ();
			TimeSpan ts;

			ts = TimeSpan.FromMinutes (59);
			Assert.AreEqual ("59", cv.ConvertTo (null, null, ts, typeof (string)), "A1");

			ts = TimeSpan.FromMinutes (144);
			Assert.AreEqual ("144", cv.ConvertTo (null, null, ts, typeof (string)), "A2");

			ts = TimeSpan.FromSeconds (390);
			Assert.AreEqual ("6", cv.ConvertTo (null, null, ts, typeof (string)), "A3");
			
			/* infinity tests */
			Assert.AreEqual ("Infinite", cv.ConvertTo (null, null, TimeSpan.MaxValue, typeof (string)), "A4");
			Assert.AreEqual ("Infinite", cv.ConvertTo (null, null, new TimeSpan (Int64.MaxValue), typeof (string)), "A5");
			Assert.AreEqual ("15372286728", cv.ConvertTo (null, null, new TimeSpan (Int64.MaxValue - 1), typeof (string)), "A6");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConvertTo_NullError ()
		{
			TimeSpanMinutesOrInfiniteConverter cv = new TimeSpanMinutesOrInfiniteConverter ();

			Assert.AreEqual ("", cv.ConvertTo (null, null, null, typeof (string)), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertTo_TypeError1 ()
		{
			TimeSpanMinutesOrInfiniteConverter cv = new TimeSpanMinutesOrInfiniteConverter ();

			Assert.AreEqual ("59", cv.ConvertTo (null, null, 59, typeof (string)), "A1");
		}

		[Test]
		public void ConvertTo_TypeError2 ()
		{
			TimeSpanMinutesOrInfiniteConverter cv = new TimeSpanMinutesOrInfiniteConverter ();
			TimeSpan ts;

			ts = TimeSpan.FromMinutes (59);

			Assert.AreEqual ("59", cv.ConvertTo (null, null, ts, typeof (int)), "A1");
			Assert.AreEqual ("59", cv.ConvertTo (null, null, ts, null), "A2");
		}

	}
}

