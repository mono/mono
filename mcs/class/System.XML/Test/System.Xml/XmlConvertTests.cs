//
// System.Xml.XmlConvertTests.cs
//
// Authors: Atsushi Enomoto (ginga@kit.hi-ho.ne.jp), Jon Kessler (jwkpiano1@comcast.net)
//
// (C) 2003 Atsushi Enomoto, Jon Kessler
//

using System;
using System.Globalization;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlConvertTests
	{
		private void AssertName (string result, string source)
		{
			Assert.AreEqual (result,
				XmlConvert.EncodeName (source));
		}
		
		private void AssertNmToken (string result, string source)
		{
			Assert.AreEqual (result,
				XmlConvert.EncodeNmToken (source));
		}
		
		[Test]
		public void DecodeName ()
		{
			Assert.AreEqual (null, XmlConvert.DecodeName (null));
			Assert.AreEqual ("", XmlConvert.DecodeName (""));
			Assert.AreEqual ("Test", XmlConvert.DecodeName ("Test"));
			Assert.AreEqual ("_Test", XmlConvert.DecodeName ("_Test"));
			Assert.AreEqual ("_hello_friends", XmlConvert.DecodeName ("_hello_friends"));
			Assert.AreEqual ("_hello friends", XmlConvert.DecodeName ("_hello friends"));
			Assert.AreEqual (" ", XmlConvert.DecodeName ("_x0020_"));
		}
		
		[Test]
		public void EncodeLocalName ()
		{
			Assert.IsNull (XmlConvert.EncodeLocalName (null));
			Assert.AreEqual (String.Empty, XmlConvert.EncodeLocalName (String.Empty));
			Assert.AreEqual ("Hello_x003A__x0020_", XmlConvert.EncodeLocalName ("Hello: "));
			Assert.AreEqual ("Hello", XmlConvert.EncodeLocalName ("Hello"));
		}
		
		[Test]
		public void EncodeName ()
		{
			Assert.IsNull (XmlConvert.EncodeName (null));
			Assert.AreEqual (String.Empty, XmlConvert.EncodeName (String.Empty));
			AssertName ("Test", "Test");
			AssertName ("Hello_x0020_my_x0020_friends.", "Hello my friends.");
			AssertName ("_x0031_23", "123");
			AssertName ("_x005F_x0031_23", "_x0031_23");
		}
		
		[Test]
		public void EncodeNmToken ()
		{
			Assert.IsNull (XmlConvert.EncodeNmToken (null));
			AssertNmToken ("Test", "Test");
			AssertNmToken ("Hello_x0020_my_x0020_friends.", "Hello my friends.");
			AssertNmToken ("123", "123");
			AssertNmToken ("_x005F_x0031_23", "_x0031_23");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void EncodeNmTokenError ()
		{
			XmlConvert.EncodeNmToken (String.Empty);
		}

		[Test]
		public void ToBoolean ()
		{
			Assert.AreEqual (true, XmlConvert.ToBoolean ("  1 "));
			Assert.AreEqual (true, XmlConvert.ToBoolean (" true "));
			Assert.AreEqual (false, XmlConvert.ToBoolean (" 0 "));
			Assert.AreEqual (false, XmlConvert.ToBoolean (" false "));
			try
			{
				Assert.AreEqual (false, XmlConvert.ToBoolean (" invalid "));
			}
			catch (FormatException)
			{
			}
		}
		
		[Test]
		public void ToByte ()
		{
			Assert.AreEqual (255, XmlConvert.ToByte ("255"));
		}
		
		[Test]
		public void ToChar ()
		{
			Assert.AreEqual ('x', XmlConvert.ToChar ("x"));
		}
		
		[Test]
		public void ToDateTime ()
		{
			//dateTime
			Assert.AreEqual (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00").Ticks);
			Assert.AreEqual (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0").Ticks);
			Assert.AreEqual (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00").Ticks);
			Assert.AreEqual (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000").Ticks);
			Assert.AreEqual (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000").Ticks);
			Assert.AreEqual (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00000").Ticks);
			Assert.AreEqual (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000000").Ticks);
			Assert.AreEqual (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000000").Ticks);
			/*
			// These tests also failed on MS.NET
			Assert.AreEqual (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00+13:00").Ticks);
			Assert.AreEqual (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0+13:00").Ticks);
			Assert.AreEqual (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00+13:00").Ticks);
			Assert.AreEqual (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000+13:00").Ticks);
			Assert.AreEqual (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000+13:00").Ticks);
			Assert.AreEqual (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00000+13:00").Ticks);
			Assert.AreEqual (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000000+13:00").Ticks);
			Assert.AreEqual (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000000+13:00").Ticks);
			Assert.AreEqual (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00Z").Ticks);
			Assert.AreEqual (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0Z").Ticks);
			Assert.AreEqual (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00Z").Ticks);
			Assert.AreEqual (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000Z").Ticks);
			Assert.AreEqual (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000Z").Ticks);
			Assert.AreEqual (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00000Z").Ticks);
			Assert.AreEqual (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000000Z").Ticks);
			Assert.AreEqual (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000000Z").Ticks);
			*/
			//time
			DateTime t1 = new DateTime (DateTime.Today.Year, 1, 1);
			t1 = DateTime.Today + new TimeSpan (12,0,0);
			Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00").Ticks);
			Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.0").Ticks);
			Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.00").Ticks);
			Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.000").Ticks);
			Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.0000").Ticks);
			Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.00000").Ticks);
			Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.000000").Ticks);
			Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.0000000").Ticks);
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00+13:00").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.f+13:00").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ff+13:00").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fff+13:00").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffff+13:00").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffff+13:00").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffffff+13:00").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffffff+13:00").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00Z").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fZ").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffZ").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffZ").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffffZ").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffffZ").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffffffZ").Ticks);//doesn't work on .NET
			//Assert.AreEqual (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffffffZ").Ticks);//doesn't work on .NET
			//date
			Assert.AreEqual (632001312000000000L, XmlConvert.ToDateTime ("2003-09-26").Ticks);
//			Assert.AreEqual (632000664000000000L, XmlConvert.ToDateTime ("2003-09-26+13:00").Ticks);
//			Assert.AreEqual (632001132000000000L, XmlConvert.ToDateTime ("2003-09-26Z").Ticks);
			//gYearMonth
			Assert.AreEqual (631979712000000000L, XmlConvert.ToDateTime ("2003-09").Ticks);
//			Assert.AreEqual (631979064000000000L, XmlConvert.ToDateTime ("2003-09+13:00").Ticks);
//			Assert.AreEqual (631979532000000000L, XmlConvert.ToDateTime ("2003-09Z").Ticks);
			//gYear
			Assert.AreEqual (631769760000000000L, XmlConvert.ToDateTime ("2003").Ticks);
//			Assert.AreEqual (631769076000000000L, XmlConvert.ToDateTime ("2003+13:00").Ticks);
//			Assert.AreEqual (631769544000000000L, XmlConvert.ToDateTime ("2003Z").Ticks);
			//gMonthDay
// Don't try locale-dependent test
//			Assert.AreEqual (632001312000000000L, XmlConvert.ToDateTime ("--09-26").Ticks);//shouldn't have a hardcoded value
//			Assert.AreEqual (632000664000000000L, XmlConvert.ToDateTime ("--09-26+13:00").Ticks);//shouldn't have a hardcoded value
//			Assert.AreEqual (632001132000000000L, XmlConvert.ToDateTime ("--09-26Z").Ticks);//shouldn't have a hardcoded value
			//gDay
// Don't try locale-dependent test
//			Assert.AreEqual (631791360000000000L, XmlConvert.ToDateTime ("---26").Ticks);//shouldn't have a hardcoded value
//			Assert.AreEqual (631790676000000000L, XmlConvert.ToDateTime ("---26+13:00").Ticks);//shouldn't have a hardcoded value
//			Assert.AreEqual (631791144000000000L, XmlConvert.ToDateTime ("---26Z").Ticks);//shouldn't have a hardcoded value
			try
			{
				Assert.AreEqual (45L, XmlConvert.ToDateTime (";ljdfas;kl").Ticks);
			}
			catch (Exception)
			{
			}
		}
		
		[Test]
		public void ToDecimal ()
		{
			Assert.AreEqual (1.987, XmlConvert.ToDecimal ("1.987"));
		}
		
		[Test]
		public void ToDouble ()
		{
			Assert.AreEqual (1.0d/0.0d, XmlConvert.ToDouble ("INF"));
			Assert.AreEqual (-1.0d/0.0d, XmlConvert.ToDouble ("-INF"));
			Assert.AreEqual (0.0d/0.0d, XmlConvert.ToDouble ("NaN"));
			Assert.AreEqual (789324, XmlConvert.ToDouble ("789324"));
			Assert.AreEqual (42, XmlConvert.ToDouble ("  42  "));
			Assert.AreEqual (double.NaN, XmlConvert.ToDouble ("  NaN  "));
			Assert.AreEqual (double.PositiveInfinity, XmlConvert.ToDouble ("  Infinity  "));
			Assert.AreEqual (double.NegativeInfinity, XmlConvert.ToDouble ("  -Infinity "));
			Assert.AreEqual (double.PositiveInfinity, XmlConvert.ToDouble ("  INF"));
			Assert.AreEqual (double.NegativeInfinity, XmlConvert.ToDouble ("  -INF "));
		}
		
		[Test]
		public void ToDoubleRoundtrip ()
		{
			// bug #320424
			string s = XmlConvert.ToString (double.MaxValue);
			Assert.AreEqual (double.MaxValue, XmlConvert.ToDouble (s));
		}
		
		[Test]
		public void ToGuid ()
		{
			Assert.AreEqual (new Guid ("ca761232-ed42-11ce-bacd-00aa0057b223"), XmlConvert.ToGuid ("ca761232-ed42-11ce-bacd-00aa0057b223"));
		}
	
		[Test]
		public void ToInt16 ()
		{
			Assert.AreEqual (0, XmlConvert.ToInt16 ("0"), "0");
			Assert.AreEqual (-1, XmlConvert.ToInt16 ("-1"), "-1");
			Assert.AreEqual (1, XmlConvert.ToInt16 ("1"), "1");
			Assert.AreEqual (32767, XmlConvert.ToInt16 ("32767"), "32767");
			Assert.AreEqual (-32768, XmlConvert.ToInt16 ("-32768"), "-32768");
			try {
				XmlConvert.ToInt16 ("32768");
				Assert.Fail ("32768");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt16 ("-32769");
				Assert.Fail ("-32769");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt16 ("0x100");
				Assert.Fail ("0x100");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToInt32 ()
		{
			Assert.AreEqual (0, XmlConvert.ToInt32 ("0"), "0");
			Assert.AreEqual (-1, XmlConvert.ToInt32 ("-1"), "-1");
			Assert.AreEqual (1, XmlConvert.ToInt32 ("1"), "1");
			Assert.AreEqual (int.MaxValue, XmlConvert.ToInt32 ("2147483647"), "2147483647");
			Assert.AreEqual (int.MinValue, XmlConvert.ToInt32 ("-2147483648"), "-2147483648");
			try {
				int.Parse ("2147483648", CultureInfo.CurrentCulture);
				Assert.Fail ("int.Parse(current culture)");
			} catch (OverflowException) {
			}
			try {
				int.Parse ("2147483648", CultureInfo.InvariantCulture);
				Assert.Fail ("int.Parse(invariant culture)");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt32 ("2147483648");
				Assert.Fail ("2147483648");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt32 ("-2147483649");
				Assert.Fail ("-2147483649");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt32 ("0x10000");
				Assert.Fail ("0x10000");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToInt64 ()
		{
			Assert.AreEqual (0, XmlConvert.ToInt64 ("0"), "0");
			Assert.AreEqual (-1, XmlConvert.ToInt64 ("-1"), "-1");
			Assert.AreEqual (1, XmlConvert.ToInt64 ("1"), "1");
			Assert.AreEqual (long.MaxValue, XmlConvert.ToInt64 ("9223372036854775807"), "9223372036854775807");
			Assert.AreEqual (long.MinValue, XmlConvert.ToInt64 ("-9223372036854775808"), "-9223372036854775808");
			try {
				XmlConvert.ToInt64 ("9223372036854775808");
				Assert.Fail ("9223372036854775808");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt64 ("-9223372036854775809");
				Assert.Fail ("-9223372036854775809");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt64 ("0x10000");
				Assert.Fail ("0x10000");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToSByte ()
		{
			Assert.AreEqual (0, XmlConvert.ToSByte ("0"), "0");
			Assert.AreEqual (-1, XmlConvert.ToSByte ("-1"), "-1");
			Assert.AreEqual (1, XmlConvert.ToSByte ("1"), "1");
			Assert.AreEqual (127, XmlConvert.ToSByte ("127"), "127");
			Assert.AreEqual (-128, XmlConvert.ToSByte ("-128"), "-128");
			try {
				XmlConvert.ToSByte ("128");
				Assert.Fail ("128");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToSByte ("-129");
				Assert.Fail ("-129");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToSByte ("0x80");
				Assert.Fail ("0x80");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToSingle ()
		{
			Assert.AreEqual (1.0d/0.0d, XmlConvert.ToSingle ("INF"));
			Assert.AreEqual (-1.0d/0.0d, XmlConvert.ToSingle ("-INF"));
			Assert.AreEqual (0.0d/0.0d, XmlConvert.ToSingle ("NaN"));
			Assert.AreEqual (789324, XmlConvert.ToSingle ("789324"));
			Assert.AreEqual (42, XmlConvert.ToSingle ("  42  "));
			Assert.AreEqual (float.NaN, XmlConvert.ToSingle ("  NaN  "));
			Assert.AreEqual (float.PositiveInfinity, XmlConvert.ToSingle ("  Infinity  "));
			Assert.AreEqual (float.NegativeInfinity, XmlConvert.ToSingle ("  -Infinity "));
			Assert.AreEqual (float.PositiveInfinity, XmlConvert.ToSingle ("  INF"));
			Assert.AreEqual (float.NegativeInfinity, XmlConvert.ToSingle ("  -INF "));
		}
		
		[Test]
		public void ToStringTest ()//not done
		{
			// Don't include TimeZone value for test value.
			string dateString = 
				XmlConvert.ToString (new DateTime (2003, 5, 5));
			Assert.AreEqual (33, dateString.Length);
			Assert.AreEqual (dateString.Substring (0, 27), "2003-05-05T00:00:00.0000000");

			// Must not throw an exception...
			Assert.IsNotNull ("-P10675199DT2H48M5.4775808S", XmlConvert.ToString (TimeSpan.MinValue));
		}

		[Test]
		public void FromTimeSpan ()
		{
			// bug #77252
			TimeSpan t1 = TimeSpan.FromTicks (
				TimeSpan.TicksPerSecond + 1);
			Assert.AreEqual ("PT1.0000001S", XmlConvert.ToString (t1), "#1");

			// XAttributeTest.CastTimeSpans():#5d
			t1 = new TimeSpan (2710L);
			Assert.AreEqual ("PT0.000271S", XmlConvert.ToString (t1), "#2");
			t1 = new TimeSpan (27100000L);
			Assert.AreEqual ("PT2.71S", XmlConvert.ToString (t1), "#3");
		}

		[Test]
		public void ToTimeSpan ()
		{
			Assert.AreEqual (new TimeSpan (0, 0, 0, 0, 1), XmlConvert.ToTimeSpan ("PT0.001S"), "#1");
			// bug #76328
			Assert.AreEqual (new TimeSpan (0, 0, 0, 0, 100), XmlConvert.ToTimeSpan ("PT0.1S"), "#2");
			Assert.AreEqual (new TimeSpan (0, 0, 0, 0, 100), XmlConvert.ToTimeSpan ("PT0.100S"), "#3");
			Assert.AreEqual (new TimeSpan (0, 0, 0, 0, 10), XmlConvert.ToTimeSpan ("PT0.010S"), "#4");
			Assert.AreEqual (new TimeSpan (0, 0, 0, 0, 10), XmlConvert.ToTimeSpan ("PT0.01S"), "#5");

			// bug #77252
			Assert.AreEqual (TimeSpan.FromTicks (TimeSpan.TicksPerSecond + 1), XmlConvert.ToTimeSpan ("PT1.0000001S"), "#6");

			Assert.AreEqual (TimeSpan.MinValue, XmlConvert.ToTimeSpan ("-P10675199DT2H48M5.4775808S"), "#7");

			Assert.AreEqual (TimeSpan.MaxValue, XmlConvert.ToTimeSpan ("P10675199DT2H48M5.4775807S"), "#8");

			Assert.AreEqual (TimeSpan.FromDays (2), XmlConvert.ToTimeSpan (" \r\n   \tP2D  "), "#9");
		}
		
		[Test]
		public void ToUInt16 ()
		{
			Assert.AreEqual (0, XmlConvert.ToUInt16 ("0"), "0");
			Assert.AreEqual (1, XmlConvert.ToUInt16 ("1"), "1");
			Assert.AreEqual (ushort.MaxValue, XmlConvert.ToUInt16 ("65535"), "65535");
			try {
				ushort.Parse ("65536", CultureInfo.CurrentCulture);
				Assert.Fail ("ushort.Parse(current culture)");
			} catch (OverflowException) {
			}
			try {
				ushort.Parse ("65536", CultureInfo.InvariantCulture);
				Assert.Fail ("ushort.Parse(invariant culture)");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt16 ("65536");
				Assert.Fail ("65536");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt16 ("0x10000");
				Assert.Fail ("0x10000");
			} catch (FormatException) {
			}
			// LAMESPEC: it is not fixable as there is no public
			// member of UInt16 that treats this as FormatException
			// while others above as either OverflowException or
			// FormatException respectively.
			// (.NET uses internal member in UInt16 here);
			//try {
			//	XmlConvert.ToUInt16 ("-101");
			//	Assert.Fail ("-101");
			//} catch (FormatException) {
			//}
		}
		
		[Test]
		public void ToUInt32 ()
		{
			Assert.AreEqual (0, XmlConvert.ToUInt32 ("0"), "0");
			Assert.AreEqual (1, XmlConvert.ToUInt32 ("1"), "1");
			Assert.AreEqual (uint.MaxValue, XmlConvert.ToUInt32 ("4294967295"), "4294967295");
			try {
				uint.Parse ("4294967296", CultureInfo.CurrentCulture);
				Assert.Fail ("uint.Parse(current culture)");
			} catch (OverflowException) {
			}
			try {
				uint.Parse ("4294967296", CultureInfo.InvariantCulture);
				Assert.Fail ("uint.Parse(invariant culture)");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt32 ("4294967296");
				Assert.Fail ("4294967296");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt32 ("0x10000");
				Assert.Fail ("0x10000");
			} catch (FormatException) {
			}
			// LAMESPEC: it is not fixable as there is no public
			// member of UInt32 that treats this as FormatException
			// while others above as either OverflowException or
			// FormatException respectively.
			// (.NET uses internal member in UInt32 here);
			//try {
			//	XmlConvert.ToUInt32 ("-101");
			//	Assert.Fail ("-101");
			//} catch (FormatException) {
			//}
		}
		
		[Test]
		public void ToUInt64 ()
		{
			Assert.AreEqual (0, XmlConvert.ToUInt64 ("0"), "0");
			Assert.AreEqual (1, XmlConvert.ToUInt64 ("1"), "1");
			Assert.AreEqual (ulong.MaxValue, XmlConvert.ToUInt64 ("18446744073709551615"), "18446744073709551615");
			try {
				ulong.Parse ("18446744073709551616", CultureInfo.CurrentCulture);
				Assert.Fail ("ulong.Parse(current culture)");
			} catch (OverflowException) {
			}
			try {
				ulong.Parse ("18446744073709551616", CultureInfo.InvariantCulture);
				Assert.Fail ("ulong.Parse(invariant culture)");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt64 ("18446744073709551616");
				Assert.Fail ("18446744073709551616");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt64 ("0x10000");
				Assert.Fail ("0x10000");
			} catch (FormatException) {
			}
			// LAMESPEC: it is not fixable as there is no public
			// member of UInt64 that treats this as FormatException
			// while others above as either OverflowException or
			// FormatException respectively.
			// (.NET uses internal member in UInt64 here);
			//try {
			//	XmlConvert.ToUInt64 ("-101");
			//	Assert.Fail ("-101");
			//} catch (FormatException) {
			//}
		}
		
		[Test]
		public void VerifyName ()
		{
			VerifyNameValid ("a");
			VerifyNameValid ("a1");
			VerifyNameValid ("\u3041");
			VerifyNameValid ("a:b");
			VerifyNameValid ("_");
			VerifyNameValid ("__");
			VerifyNameValid ("_1");
			VerifyNameValid (":");
			VerifyNameValid (":a");
			VerifyNameValid ("a.b");
		}

		[Test]
		public void VerifyNameInvalid ()
		{
			VerifyNameInvalid ("!");
			VerifyNameInvalid ("_a!b");
			VerifyNameInvalid ("?a");
			VerifyNameInvalid (" ");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void VerifyNameNull ()
		{
			XmlConvert.VerifyName (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void VerifyNameEmpty ()
		{
			XmlConvert.VerifyName ("");
		}

		private void VerifyNameValid (string value)
		{
			try {
				XmlConvert.VerifyName (value);
			} catch (XmlException) {
				Assert.Fail (String.Format ("'{0}'", value));
			}
		}

		private void VerifyNameInvalid (string value)
		{
			try {
				XmlConvert.VerifyName (value);
				Assert.Fail (value);
			} catch (XmlException) {
			}
		}

		[Test]
		public void VerifyNCName ()
		{
			Assert.AreEqual ("foo", XmlConvert.VerifyNCName ("foo"));
			try {
				XmlConvert.VerifyNCName ("?foo");
				Assert.Fail ();
			} catch (XmlException) {}
			try {
				XmlConvert.VerifyNCName (":foo");
				Assert.Fail ();
			} catch (XmlException) {}
			try {
				XmlConvert.VerifyNCName ("foo:bar");
				Assert.Fail ();
			} catch (XmlException) {}
			try {
				XmlConvert.VerifyNCName ("foo:bar:baz");
				Assert.Fail ();
			} catch (XmlException) {}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void VerifyNCNameNull ()
		{
			XmlConvert.VerifyNCName (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void VerifyNCNameEmpty ()
		{
			XmlConvert.VerifyNCName ("");
		}

		[Test]
		public void DurationZero () // bug #77350
		{
			Assert.AreEqual ("PT0S", XmlConvert.ToString (TimeSpan.FromSeconds (0)));
		}

#if NET_2_0
		[Test]
		public void VerifyTOKEN ()
		{
			VerifyToken ("", true);
			VerifyToken (" ", false);
			VerifyToken ("A", true);
			VerifyToken ("!", true);
			VerifyToken (" !", false);
			VerifyToken ("! ", false);
			VerifyToken ("! !", true);
			VerifyToken ("!\t!", false);
			VerifyToken ("!\n!", false);
			VerifyToken ("!\r!", false);
			VerifyToken ("###", true);
		}

		private void VerifyToken (string s, bool success)
		{
			try {
				XmlConvert.VerifyTOKEN (s);
				if (success)
					return;
				Assert.Fail (s + "should fail");
			} catch (XmlException ex) {
				if (success)
					Assert.Fail (s + "should not fail");
			}
		}

		[Test]
		public void XmlDateTimeSerializationModeAndMaxValue ()
		{
			Assert.AreEqual ("9999-12-31T23:59:59.9999999", XmlConvert.ToString (DateTime.MaxValue, XmlDateTimeSerializationMode.Unspecified).Substring (0, 27), "#1");
			Assert.AreEqual ("9999-12-31T23:59:59.9999999Z", XmlConvert.ToString (DateTime.MaxValue, XmlDateTimeSerializationMode.Utc), "#2");
			Assert.AreEqual ("9999-12-31T23:59:59.9999999", XmlConvert.ToString (DateTime.MaxValue, XmlDateTimeSerializationMode.RoundtripKind), "#3");
			Assert.AreEqual ("9999-12-31T23:59:59.9999999", XmlConvert.ToString (DateTime.MaxValue, XmlDateTimeSerializationMode.Local).Substring (0, 27), "#4");
			// direct formatting string - no difference
			Assert.AreEqual ("9999-12-31T23:59:59.9999999Z", XmlConvert.ToString (DateTime.MaxValue, "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ"), "#5");
			Assert.AreEqual ("9999-12-31T23:59:59.9999999", XmlConvert.ToString (DateTime.MaxValue, "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz").Substring (0, 27), "#6");
		}

		[Test]
		public void XmlDateTimeSerializationModeRountripKind ()
		{
			string format = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
			string s = XmlConvert.ToString (DateTime.UtcNow, format);
			Assert.AreEqual ('Z', s [s.Length -1], "#1-1");
			// LAMESPEC: .NET has a bug here that 'K' in format string does not reflect 'Z' as Utc Kind.
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=307694
			//Assert.AreEqual (DateTimeKind.Utc, XmlConvert.ToDateTime (s, format).Kind, "#1-2");

			s = XmlConvert.ToString (DateTime.UtcNow, XmlDateTimeSerializationMode.RoundtripKind);
			Assert.AreEqual ('Z', s [s.Length -1], "#2-1");
			Assert.AreEqual (DateTimeKind.Utc, XmlConvert.ToDateTime (s, XmlDateTimeSerializationMode.RoundtripKind).Kind, "#2-2");
		}
		
		[Test]
		public void XmlDateTimeSerializationModeUnspecified ()
		{
			Assert.AreEqual (27, XmlConvert.ToString (new DateTime (DateTime.MaxValue.Ticks, DateTimeKind.Utc), XmlDateTimeSerializationMode.Unspecified).Length, "#1");
			DateTime dt1 = XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000+02:00", XmlDateTimeSerializationMode.Unspecified);
			DateTime dt2 = XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000", XmlDateTimeSerializationMode.Unspecified);
			Assert.AreEqual (false, dt1 == dt2, "#2");
			XmlConvert.ToDateTime ("2006-05-30T09:48:32.0Z", XmlDateTimeSerializationMode.Unspecified);
			string format = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
			XmlConvert.ToDateTime (XmlConvert.ToString (DateTime.UtcNow, format), XmlDateTimeSerializationMode.Unspecified);
		}
		
		[Test]
		public void XmlDateTimeSerializationModeLocal ()
		{
			XmlConvert.ToDateTime ("2010-11-10", XmlDateTimeSerializationMode.Local); // bug #655089
			XmlConvert.ToDateTime ("2010-11", XmlDateTimeSerializationMode.Local);
		}
		
		[Test]
		public void XmlDateTimeSerializationModeUtc ()
		{
			Assert.AreEqual (27, XmlConvert.ToString (new DateTime (DateTime.MaxValue.Ticks, DateTimeKind.Utc), XmlDateTimeSerializationMode.Unspecified).Length, "#1");
			DateTime dt1 = XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000+02:00", XmlDateTimeSerializationMode.Utc);
			DateTime dt2 = XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000", XmlDateTimeSerializationMode.Utc);
			Assert.AreEqual (false, dt1 == dt2, "#2");
			XmlConvert.ToDateTime ("2006-05-30T09:48:32.0Z", XmlDateTimeSerializationMode.Utc);
			XmlConvert.ToDateTime ("2006-05-30T09:48:32.0+02:00", XmlDateTimeSerializationMode.Utc);
			XmlConvert.ToDateTime ("2008-06-11T11:09:47.125Z", XmlDateTimeSerializationMode.Utc);
		}

		[Test]
		public void XmlDateTimeSerializationModeSeveralFormats ()
		{
			XmlDateTimeSerializationMode m = XmlDateTimeSerializationMode.RoundtripKind;
			XmlConvert.ToDateTime ("0001", m);
			XmlConvert.ToDateTime ("0001Z", m);
			XmlConvert.ToDateTime ("0001+09:00", m);
			XmlConvert.ToDateTime ("0001-02", m);
			XmlConvert.ToDateTime ("0001-02Z", m);
			XmlConvert.ToDateTime ("0001-02+09:00", m);
			XmlConvert.ToDateTime ("0001-02-03", m);
			XmlConvert.ToDateTime ("0001-02-03Z", m);
			XmlConvert.ToDateTime ("0001-02-03+09:00", m);
			XmlConvert.ToDateTime ("--02-03", m);
			XmlConvert.ToDateTime ("--02-03Z", m);
			XmlConvert.ToDateTime ("--02-03+09:00", m);
			XmlConvert.ToDateTime ("---03", m);
			XmlConvert.ToDateTime ("---03Z", m);
			XmlConvert.ToDateTime ("---03+09:00", m);
			XmlConvert.ToDateTime ("10:20:30", m);
			XmlConvert.ToDateTime ("10:20:30Z", m);
			XmlConvert.ToDateTime ("10:20:30+09:00", m);
			XmlConvert.ToDateTime ("0001-02-03T10:20:30", m);
			XmlConvert.ToDateTime ("0001-02-03T10:20:30Z", m);
			XmlConvert.ToDateTime ("0001-02-03T10:20:30+09:00", m);
			XmlConvert.ToDateTime ("0001-02-03T10:20:30.00", m);
			XmlConvert.ToDateTime ("0001-02-03T10:20:30.00Z", m);
			XmlConvert.ToDateTime ("0001-02-03T10:20:30.00+09:00", m);
			XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000", m);
			XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000Z", m);
			XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000+09:00", m);

			try {
				XmlConvert.ToDateTime ("0001-02-03T", m);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}
			try {
				XmlConvert.ToDateTime ("0001-02-03T10:20", m);
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
			try {
				XmlConvert.ToDateTime ("0001-02-03T10:20:30.", m);
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test] // see http://smdn.invisiblefulmoon.net/misc/forum/programming/#n10
		public void DateTimeOffsetTimezoneRoundtrip ()
		{
			Assert.AreEqual (new DateTimeOffset (2009, 11, 05, 20, 16, 22, TimeSpan.FromHours (9)),  XmlConvert.ToDateTimeOffset ("2009-11-05T20:16:22+09:00"), "#1");
		}

		[Test]
		public void DateTimeOffsetWithWhitespace ()
		{
			var s = "   2010-01-02T00:00:00Z \t";
			XmlConvert.ToDateTime (s);
		}
#endif
	}
}

