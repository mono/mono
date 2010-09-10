//
// DateTimeOffsetConverterTests.cs
//
// Author:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

#if NET_4_0

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class DateTimeOffsetConverterTests
	{
		DateTimeOffsetConverter converter;
        
		[SetUp]
		public void SetUp ()
		{
			converter = new DateTimeOffsetConverter ();
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#A1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (DateTime)), "#A2");
			Assert.IsFalse (converter.CanConvertFrom (typeof (DateTimeOffset)), "#A3");
			Assert.IsFalse (converter.CanConvertFrom (typeof (object)), "#A4");
			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "#A5");
		}

		[Test]
		public void CanConvertTo ()
		{
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#A1");
			Assert.IsFalse (converter.CanConvertTo (typeof (object)), "#A2");
			Assert.IsFalse (converter.CanConvertTo (typeof (DateTime)), "#A3");
			Assert.IsFalse (converter.CanConvertTo (typeof (DateTimeOffset)), "#A4");
			Assert.IsTrue (converter.CanConvertTo (typeof (InstanceDescriptor)), "#A5");
		}

		[Test]
		public void ConvertFrom_String ()
		{
			DateTimeOffset dateOffset = DateTimeOffset.Now;
			DateTimeOffset newDateOffset = (DateTimeOffset) converter.ConvertFrom (null, CultureInfo.InvariantCulture, 
					dateOffset.ToString (CultureInfo.InvariantCulture));

			Assert.AreEqual (dateOffset.Date, newDateOffset.Date, "#A1");
			Assert.AreEqual (dateOffset.Hour, newDateOffset.Hour, "#A2");
			Assert.AreEqual (dateOffset.Minute, newDateOffset.Minute, "#A3");
			Assert.AreEqual (dateOffset.Second, newDateOffset.Second, "#A4");
			Assert.AreEqual (dateOffset.Offset, newDateOffset.Offset, "#A5");

			newDateOffset = (DateTimeOffset) converter.ConvertFrom (null, CultureInfo.InvariantCulture, String.Empty);
			Assert.AreEqual (DateTimeOffset.MinValue, newDateOffset, "#B1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Object ()
		{
			converter.ConvertFrom (new object ());
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_Int32 ()
		{
			converter.ConvertFrom (10);
		}

		[Test]
		public void ConvertTo_MinValue ()
		{
			Assert.AreEqual (String.Empty, converter.ConvertTo (null, 
				CultureInfo.InvariantCulture, DateTimeOffset.MinValue, typeof (string)), "#A1");
			Assert.AreEqual (String.Empty, converter.ConvertTo (null, 
				CultureInfo.CurrentCulture, DateTimeOffset.MinValue, typeof (string)), "#A2");
			Assert.AreEqual (String.Empty, converter.ConvertTo (DateTimeOffset.MinValue, 
				typeof (string)), "#A3");
		}

		[Test]
		public void ConvertTo_MaxValue ()
		{
			Assert.AreEqual (DateTimeOffset.MaxValue.ToString (CultureInfo.InvariantCulture), 
				converter.ConvertTo (null, CultureInfo.InvariantCulture, DateTimeOffset.MaxValue, 
				typeof (string)), "#A1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_object ()
		{
			converter.ConvertTo (DateTimeOffset.Now, typeof (object));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_int ()
		{
			converter.ConvertTo (DateTimeOffset.Now, typeof (int));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_DateTime ()
		{
			converter.ConvertTo (DateTimeOffset.Now, typeof (DateTime));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_DateTimeOffset ()
		{
			converter.ConvertTo (DateTimeOffset.Now, typeof (DateTimeOffset));
		}

		[Test]
		public void ConvertToString_MinValue ()
		{
			Assert.AreEqual (String.Empty, converter.ConvertToString (null, 
				CultureInfo.InvariantCulture, DateTimeOffset.MinValue), "#A1");

			Assert.AreEqual (String.Empty, converter.ConvertToString (null, DateTimeOffset.MinValue), "#A2");
			Assert.AreEqual (String.Empty, converter.ConvertToString (null, 
				CultureInfo.CurrentCulture, DateTimeOffset.MinValue), "#A3");
			Assert.AreEqual (String.Empty, converter.ConvertToString (DateTimeOffset.MinValue), "#A4");
		}

		[Test]
		public void ConvertToString_MaxValue ()
		{
			Assert.AreEqual (DateTimeOffset.MaxValue.ToString (CultureInfo.InvariantCulture), 
				converter.ConvertToString (null, CultureInfo.InvariantCulture, DateTimeOffset.MaxValue), "#A1");
		}

		[Test]
		public void ConvertToString ()
		{
			CultureInfo ciUS = new CultureInfo("en-US");
			CultureInfo ciGB = new CultureInfo("en-GB");
			CultureInfo ciDE = new CultureInfo("de-DE");

			DateTimeOffset dateOffset = new DateTimeOffset (2008, 12, 31, 23, 59, 58, 5, new TimeSpan (3, 6, 0));
			DoTestToString ("12/31/2008 11:59 p.m. +03:06", dateOffset, ciUS);
			DoTestToString ("31/12/2008 23:59 +03:06", dateOffset, ciGB);
			DoTestToString ("31.12.2008 23:59 +03:06", dateOffset, ciDE);
			DoTestToString ("12/31/2008 23:59:58 +03:06", dateOffset, CultureInfo.InvariantCulture);
			Assert.AreEqual ("12/31/2008 23:59:58 +03:06", converter.ConvertToInvariantString (dateOffset), "Invariant");

			dateOffset = new DateTimeOffset (new DateTime (2008, 12, 31), new TimeSpan (0, 0, 0));
			DoTestToString ("12/31/2008 +00:00", dateOffset, ciUS);
			DoTestToString ("31/12/2008 +00:00", dateOffset, ciGB);
			DoTestToString ("31.12.2008 +00:00", dateOffset, ciDE);
			DoTestToString ("2008-12-31 +00:00", dateOffset, CultureInfo.InvariantCulture);
			Assert.AreEqual ("2008-12-31 +00:00", converter.ConvertToInvariantString (dateOffset), "Invariant");
		}

		void DoTestToString (string expected, DateTimeOffset value, CultureInfo ci)
		{
			String message = ci.Name;
			if (message == null || message.Length == 0)
				message = "?Invariant";
			Assert.AreEqual (expected, converter.ConvertTo (null, ci, value, typeof (string)), message);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFrom_InvalidValue ()
		{
			converter.ConvertFrom ("*1");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFrom_InvalidValue_Invariant ()
		{
			converter.ConvertFrom (null, CultureInfo.InvariantCulture, "*1");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromString_InvalidValue ()
		{
			converter.ConvertFromString ("*1");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertFromString_InvalidValue_Invariant ()
		{
			converter.ConvertFromString (null, CultureInfo.InvariantCulture, "*1");
		}

		[Test]
		public void ConvertTo_InstanceDescriptor ()
		{
			DateTimeOffset dto = new DateTimeOffset (new DateTime (2010, 10, 11), new TimeSpan (3, 6, 0));
			InstanceDescriptor descriptor = (InstanceDescriptor) converter.ConvertTo (dto, typeof (InstanceDescriptor));

			Assert.AreEqual (".ctor", descriptor.MemberInfo.Name, "#A0");
			Assert.AreEqual (8, descriptor.Arguments.Count, "#A1");
			DateTimeOffset dto2 = (DateTimeOffset) descriptor.Invoke ();
			Assert.AreEqual (dto, dto2, "#A2");
		}
	}
}

#endif

