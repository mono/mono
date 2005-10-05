//
// System.Configuration.TimeSpanMinutesConverterTest.cs - Unit tests
// for System.Configuration.TimeSpanMinutesConverter.
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
	public class TimeSpanMinutesConverterTest
	{
		[Test]
		public void CanConvertFrom ()
		{
			TimeSpanMinutesConverter cv = new TimeSpanMinutesConverter ();

			Assert.IsTrue (cv.CanConvertFrom (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (object)), "A4");
		}

		[Test]
		public void CanConvertTo ()
		{
			TimeSpanMinutesConverter cv = new TimeSpanMinutesConverter ();

			Assert.IsTrue (cv.CanConvertTo (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (object)), "A4");
		}

		[Test]
		public void ConvertFrom ()
		{
			TimeSpanMinutesConverter cv = new TimeSpanMinutesConverter ();
			object o;

			o = cv.ConvertFrom (null, null, "59");
			Assert.AreEqual (typeof (TimeSpan), o.GetType(), "A1");
			Assert.AreEqual ("00:59:00", o.ToString(), "A2");
			o = cv.ConvertFrom (null, null, "104");
			Assert.AreEqual ("01:44:00", o.ToString(), "A3");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFrom_FormatError ()
		{
			TimeSpanMinutesConverter cv = new TimeSpanMinutesConverter ();
			object o;

			o = cv.ConvertFrom (null, null, "100.5");
			Assert.IsNull (o, "A1");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertFrom_TypeError ()
		{
			TimeSpanMinutesConverter cv = new TimeSpanMinutesConverter ();
			object o;

			o = cv.ConvertFrom (null, null, 59);
			Assert.IsNull (o, "A1");
		}

		[Test]
		public void ConvertTo ()
		{
			TimeSpanMinutesConverter cv = new TimeSpanMinutesConverter ();
			TimeSpan ts;

			ts = TimeSpan.FromMinutes (59);
			Assert.AreEqual ("59", cv.ConvertTo (null, null, ts, typeof (string)), "A1");

			ts = TimeSpan.FromMinutes (144);
			Assert.AreEqual ("144", cv.ConvertTo (null, null, ts, typeof (string)), "A2");

			ts = TimeSpan.FromSeconds (390);
			Assert.AreEqual ("6", cv.ConvertTo (null, null, ts, typeof (string)), "A2");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConvertTo_NullError ()
		{
			TimeSpanMinutesConverter cv = new TimeSpanMinutesConverter ();

			Assert.AreEqual ("", cv.ConvertTo (null, null, null, typeof (string)), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertTo_TypeError1 ()
		{
			TimeSpanMinutesConverter cv = new TimeSpanMinutesConverter ();

			Assert.AreEqual ("59", cv.ConvertTo (null, null, 59, typeof (string)), "A1");
		}

		[Test]
		public void ConvertTo_TypeError2 ()
		{
			TimeSpanMinutesConverter cv = new TimeSpanMinutesConverter ();
			TimeSpan ts;

			ts = TimeSpan.FromMinutes (59);

			Assert.AreEqual ("59", cv.ConvertTo (null, null, ts, typeof (int)), "A1");
			Assert.AreEqual ("59", cv.ConvertTo (null, null, ts, null), "A2");
		}

	}
}

#endif
