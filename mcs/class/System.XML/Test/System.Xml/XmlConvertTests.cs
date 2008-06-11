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

using AssertType = NUnit.Framework.Assert;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlConvertTests : Assertion
	{
		private void AssertName (string result, string source)
		{
			AssertEquals (result,
				XmlConvert.EncodeName (source));
		}
		
		private void AssertNmToken (string result, string source)
		{
			AssertEquals (result,
				XmlConvert.EncodeNmToken (source));
		}
		
		[Test]
		public void DecodeName ()
		{
			AssertEquals (null, XmlConvert.DecodeName (null));
			AssertEquals ("", XmlConvert.DecodeName (""));
			AssertEquals ("Test", XmlConvert.DecodeName ("Test"));
			AssertEquals ("_Test", XmlConvert.DecodeName ("_Test"));
			AssertEquals ("_hello_friends", XmlConvert.DecodeName ("_hello_friends"));
			AssertEquals ("_hello friends", XmlConvert.DecodeName ("_hello friends"));
			AssertEquals (" ", XmlConvert.DecodeName ("_x0020_"));
		}
		
		[Test]
		public void EncodeLocalName ()
		{
			AssertNull (XmlConvert.EncodeLocalName (null));
			AssertEquals (String.Empty, XmlConvert.EncodeLocalName (String.Empty));
			AssertEquals ("Hello_x003A__x0020_", XmlConvert.EncodeLocalName ("Hello: "));
			AssertEquals ("Hello", XmlConvert.EncodeLocalName ("Hello"));
		}
		
		[Test]
		public void EncodeName ()
		{
			AssertNull (XmlConvert.EncodeName (null));
			AssertEquals (String.Empty, XmlConvert.EncodeName (String.Empty));
			AssertName ("Test", "Test");
			AssertName ("Hello_x0020_my_x0020_friends.", "Hello my friends.");
			AssertName ("_x0031_23", "123");
			AssertName ("_x005F_x0031_23", "_x0031_23");
		}
		
		[Test]
		public void EncodeNmToken ()
		{
			AssertNull (XmlConvert.EncodeNmToken (null));
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
			AssertEquals (true, XmlConvert.ToBoolean ("  1 "));
			AssertEquals (true, XmlConvert.ToBoolean (" true "));
			AssertEquals (false, XmlConvert.ToBoolean (" 0 "));
			AssertEquals (false, XmlConvert.ToBoolean (" false "));
			try
			{
				AssertEquals (false, XmlConvert.ToBoolean (" invalid "));
			}
			catch (FormatException)
			{
			}
		}
		
		[Test]
		public void ToByte ()
		{
			AssertEquals (255, XmlConvert.ToByte ("255"));
		}
		
		[Test]
		public void ToChar ()
		{
			AssertEquals ('x', XmlConvert.ToChar ("x"));
		}
		
		[Test]
		public void ToDateTime ()
		{
			//dateTime
			AssertEquals (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00").Ticks);
			AssertEquals (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0").Ticks);
			AssertEquals (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00").Ticks);
			AssertEquals (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000").Ticks);
			AssertEquals (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000").Ticks);
			AssertEquals (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00000").Ticks);
			AssertEquals (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000000").Ticks);
			AssertEquals (632001798000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000000").Ticks);
			/*
			// These tests also failed on MS.NET
			AssertEquals (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00+13:00").Ticks);
			AssertEquals (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0+13:00").Ticks);
			AssertEquals (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00+13:00").Ticks);
			AssertEquals (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000+13:00").Ticks);
			AssertEquals (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000+13:00").Ticks);
			AssertEquals (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00000+13:00").Ticks);
			AssertEquals (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000000+13:00").Ticks);
			AssertEquals (632001150000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000000+13:00").Ticks);
			AssertEquals (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00Z").Ticks);
			AssertEquals (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0Z").Ticks);
			AssertEquals (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00Z").Ticks);
			AssertEquals (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000Z").Ticks);
			AssertEquals (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000Z").Ticks);
			AssertEquals (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.00000Z").Ticks);
			AssertEquals (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.000000Z").Ticks);
			AssertEquals (632001618000000000L, XmlConvert.ToDateTime ("2003-09-26T13:30:00.0000000Z").Ticks);
			*/
			//time
			DateTime t1 = new DateTime (DateTime.Today.Year, 1, 1);
			t1 = DateTime.Today + new TimeSpan (12,0,0);
			AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00").Ticks);
			AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.0").Ticks);
			AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.00").Ticks);
			AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.000").Ticks);
			AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.0000").Ticks);
			AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.00000").Ticks);
			AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.000000").Ticks);
			AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.0000000").Ticks);
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00+13:00").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.f+13:00").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ff+13:00").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fff+13:00").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffff+13:00").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffff+13:00").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffffff+13:00").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffffff+13:00").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00Z").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fZ").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffZ").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffZ").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffffZ").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffffZ").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.ffffffZ").Ticks);//doesn't work on .NET
			//AssertEquals (t1.Ticks, XmlConvert.ToDateTime ("12:00:00.fffffffZ").Ticks);//doesn't work on .NET
			//date
			AssertEquals (632001312000000000L, XmlConvert.ToDateTime ("2003-09-26").Ticks);
//			AssertEquals (632000664000000000L, XmlConvert.ToDateTime ("2003-09-26+13:00").Ticks);
//			AssertEquals (632001132000000000L, XmlConvert.ToDateTime ("2003-09-26Z").Ticks);
			//gYearMonth
			AssertEquals (631979712000000000L, XmlConvert.ToDateTime ("2003-09").Ticks);
//			AssertEquals (631979064000000000L, XmlConvert.ToDateTime ("2003-09+13:00").Ticks);
//			AssertEquals (631979532000000000L, XmlConvert.ToDateTime ("2003-09Z").Ticks);
			//gYear
			AssertEquals (631769760000000000L, XmlConvert.ToDateTime ("2003").Ticks);
//			AssertEquals (631769076000000000L, XmlConvert.ToDateTime ("2003+13:00").Ticks);
//			AssertEquals (631769544000000000L, XmlConvert.ToDateTime ("2003Z").Ticks);
			//gMonthDay
// Don't try locale-dependent test
//			AssertEquals (632001312000000000L, XmlConvert.ToDateTime ("--09-26").Ticks);//shouldn't have a hardcoded value
//			AssertEquals (632000664000000000L, XmlConvert.ToDateTime ("--09-26+13:00").Ticks);//shouldn't have a hardcoded value
//			AssertEquals (632001132000000000L, XmlConvert.ToDateTime ("--09-26Z").Ticks);//shouldn't have a hardcoded value
			//gDay
// Don't try locale-dependent test
//			AssertEquals (631791360000000000L, XmlConvert.ToDateTime ("---26").Ticks);//shouldn't have a hardcoded value
//			AssertEquals (631790676000000000L, XmlConvert.ToDateTime ("---26+13:00").Ticks);//shouldn't have a hardcoded value
//			AssertEquals (631791144000000000L, XmlConvert.ToDateTime ("---26Z").Ticks);//shouldn't have a hardcoded value
			try
			{
				AssertEquals (45L, XmlConvert.ToDateTime (";ljdfas;kl").Ticks);
			}
			catch (Exception)
			{
			}
		}
		
		[Test]
		public void ToDecimal ()
		{
			AssertEquals (1.987, XmlConvert.ToDecimal ("1.987"));
		}
		
		[Test]
		public void ToDouble ()
		{
			AssertEquals (1.0d/0.0d, XmlConvert.ToDouble ("INF"));
			AssertEquals (-1.0d/0.0d, XmlConvert.ToDouble ("-INF"));
			AssertEquals (0.0d/0.0d, XmlConvert.ToDouble ("NaN"));
			AssertEquals (789324, XmlConvert.ToDouble ("789324"));
		}
		
		[Test]
		public void ToDoubleRoundtrip ()
		{
			// bug #320424
			string s = XmlConvert.ToString (double.MaxValue);
			AssertEquals (double.MaxValue, XmlConvert.ToDouble (s));
		}
		
		[Test]
		public void ToGuid ()
		{
			AssertEquals (new Guid ("ca761232-ed42-11ce-bacd-00aa0057b223"), XmlConvert.ToGuid ("ca761232-ed42-11ce-bacd-00aa0057b223"));
		}
	
		[Test]
		public void ToInt16 ()
		{
			AssertType.AreEqual (0, XmlConvert.ToInt16 ("0"), "0");
			AssertType.AreEqual (-1, XmlConvert.ToInt16 ("-1"), "-1");
			AssertType.AreEqual (1, XmlConvert.ToInt16 ("1"), "1");
			AssertType.AreEqual (32767, XmlConvert.ToInt16 ("32767"), "32767");
			AssertType.AreEqual (-32768, XmlConvert.ToInt16 ("-32768"), "-32768");
			try {
				XmlConvert.ToInt16 ("32768");
				AssertType.Fail ("32768");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt16 ("-32769");
				AssertType.Fail ("-32769");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt16 ("0x100");
				AssertType.Fail ("0x100");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToInt32 ()
		{
			AssertType.AreEqual (0, XmlConvert.ToInt32 ("0"), "0");
			AssertType.AreEqual (-1, XmlConvert.ToInt32 ("-1"), "-1");
			AssertType.AreEqual (1, XmlConvert.ToInt32 ("1"), "1");
			AssertType.AreEqual (int.MaxValue, XmlConvert.ToInt32 ("2147483647"), "2147483647");
			AssertType.AreEqual (int.MinValue, XmlConvert.ToInt32 ("-2147483648"), "-2147483648");
			try {
				int.Parse ("2147483648", CultureInfo.CurrentCulture);
				AssertType.Fail ("int.Parse(current culture)");
			} catch (OverflowException) {
			}
			try {
				int.Parse ("2147483648", CultureInfo.InvariantCulture);
				AssertType.Fail ("int.Parse(invariant culture)");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt32 ("2147483648");
				AssertType.Fail ("2147483648");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt32 ("-2147483649");
				AssertType.Fail ("-2147483649");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt32 ("0x10000");
				AssertType.Fail ("0x10000");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToInt64 ()
		{
			AssertType.AreEqual (0, XmlConvert.ToInt64 ("0"), "0");
			AssertType.AreEqual (-1, XmlConvert.ToInt64 ("-1"), "-1");
			AssertType.AreEqual (1, XmlConvert.ToInt64 ("1"), "1");
			AssertType.AreEqual (long.MaxValue, XmlConvert.ToInt64 ("9223372036854775807"), "9223372036854775807");
			AssertType.AreEqual (long.MinValue, XmlConvert.ToInt64 ("-9223372036854775808"), "-9223372036854775808");
			try {
				XmlConvert.ToInt64 ("9223372036854775808");
				AssertType.Fail ("9223372036854775808");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt64 ("-9223372036854775809");
				AssertType.Fail ("-9223372036854775809");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToInt64 ("0x10000");
				AssertType.Fail ("0x10000");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToSByte ()
		{
			AssertType.AreEqual (0, XmlConvert.ToSByte ("0"), "0");
			AssertType.AreEqual (-1, XmlConvert.ToSByte ("-1"), "-1");
			AssertType.AreEqual (1, XmlConvert.ToSByte ("1"), "1");
			AssertType.AreEqual (127, XmlConvert.ToSByte ("127"), "127");
			AssertType.AreEqual (-128, XmlConvert.ToSByte ("-128"), "-128");
			try {
				XmlConvert.ToSByte ("128");
				AssertType.Fail ("128");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToSByte ("-129");
				AssertType.Fail ("-129");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToSByte ("0x80");
				AssertType.Fail ("0x80");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToSingle ()//not done
		{
			
		}
		
		[Test]
		public void ToStringTest ()//not done
		{
			// Don't include TimeZone value for test value.
			string dateString = 
				XmlConvert.ToString (new DateTime (2003, 5, 5));
			AssertEquals (33, dateString.Length);
			AssertEquals ("2003-05-05T00:00:00.0000000", dateString.Substring (0, 27));
		}

		[Test]
		public void FromTimeSpan ()
		{
			// bug #77252
			TimeSpan t1 = TimeSpan.FromTicks (
				TimeSpan.TicksPerSecond + 1);
			AssertEquals ("PT1.0000001S", XmlConvert.ToString (t1));
		}

		[Test]
		public void ToTimeSpan ()
		{
			AssertEquals ("#1", new TimeSpan (0, 0, 0, 0, 1),
				XmlConvert.ToTimeSpan ("PT0.001S"));
			// bug #76328
			AssertEquals ("#2", new TimeSpan (0, 0, 0, 0, 100),
				XmlConvert.ToTimeSpan ("PT0.1S"));
			AssertEquals ("#3", new TimeSpan (0, 0, 0, 0, 100),
				XmlConvert.ToTimeSpan ("PT0.100S"));
			AssertEquals ("#4", new TimeSpan (0, 0, 0, 0, 10),
				XmlConvert.ToTimeSpan ("PT0.010S"));
			AssertEquals ("#5", new TimeSpan (0, 0, 0, 0, 10),
				XmlConvert.ToTimeSpan ("PT0.01S"));

			// bug #77252
			AssertEquals ("#6",
				TimeSpan.FromTicks (TimeSpan.TicksPerSecond + 1),
				XmlConvert.ToTimeSpan ("PT1.0000001S"));

			AssertEquals ("#7",
				TimeSpan.MinValue,
				XmlConvert.ToTimeSpan ("-P10675199DT2H48M5.4775808S"));

			AssertEquals ("#8",
				TimeSpan.MaxValue,
				XmlConvert.ToTimeSpan ("P10675199DT2H48M5.4775807S"));
		}
		
		[Test]
		public void ToUInt16 ()
		{
			AssertType.AreEqual (0, XmlConvert.ToUInt16 ("0"), "0");
			AssertType.AreEqual (1, XmlConvert.ToUInt16 ("1"), "1");
			AssertType.AreEqual (ushort.MaxValue, XmlConvert.ToUInt16 ("65535"), "65535");
			try {
				ushort.Parse ("65536", CultureInfo.CurrentCulture);
				AssertType.Fail ("ushort.Parse(current culture)");
			} catch (OverflowException) {
			}
			try {
				ushort.Parse ("65536", CultureInfo.InvariantCulture);
				AssertType.Fail ("ushort.Parse(invariant culture)");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt16 ("65536");
				AssertType.Fail ("65536");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt16 ("0x10000");
				AssertType.Fail ("0x10000");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToUInt32 ()
		{
			AssertType.AreEqual (0, XmlConvert.ToUInt32 ("0"), "0");
			AssertType.AreEqual (1, XmlConvert.ToUInt32 ("1"), "1");
			AssertType.AreEqual (uint.MaxValue, XmlConvert.ToUInt32 ("4294967295"), "4294967295");
			try {
				uint.Parse ("4294967296", CultureInfo.CurrentCulture);
				AssertType.Fail ("uint.Parse(current culture)");
			} catch (OverflowException) {
			}
			try {
				uint.Parse ("4294967296", CultureInfo.InvariantCulture);
				AssertType.Fail ("uint.Parse(invariant culture)");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt32 ("4294967296");
				AssertType.Fail ("4294967296");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt32 ("0x10000");
				AssertType.Fail ("0x10000");
			} catch (FormatException) {
			}
		}
		
		[Test]
		public void ToUInt64 ()
		{
			AssertType.AreEqual (0, XmlConvert.ToUInt64 ("0"), "0");
			AssertType.AreEqual (1, XmlConvert.ToUInt64 ("1"), "1");
			AssertType.AreEqual (ulong.MaxValue, XmlConvert.ToUInt64 ("18446744073709551615"), "18446744073709551615");
			try {
				ulong.Parse ("18446744073709551616", CultureInfo.CurrentCulture);
				AssertType.Fail ("ulong.Parse(current culture)");
			} catch (OverflowException) {
			}
			try {
				ulong.Parse ("18446744073709551616", CultureInfo.InvariantCulture);
				AssertType.Fail ("ulong.Parse(invariant culture)");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt64 ("18446744073709551616");
				AssertType.Fail ("18446744073709551616");
			} catch (OverflowException) {
			}
			try {
				XmlConvert.ToUInt64 ("0x10000");
				AssertType.Fail ("0x10000");
			} catch (FormatException) {
			}
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
				AssertType.Fail (String.Format ("'{0}'", value));
			}
		}

		private void VerifyNameInvalid (string value)
		{
			try {
				XmlConvert.VerifyName (value);
				AssertType.Fail (value);
			} catch (XmlException) {
			}
		}

		[Test]
		public void VerifyNCName ()
		{
			AssertEquals ("foo", XmlConvert.VerifyNCName ("foo"));
			try {
				XmlConvert.VerifyNCName ("?foo");
				Fail ();
			} catch (XmlException) {}
			try {
				XmlConvert.VerifyNCName (":foo");
				Fail ();
			} catch (XmlException) {}
			try {
				XmlConvert.VerifyNCName ("foo:bar");
				Fail ();
			} catch (XmlException) {}
			try {
				XmlConvert.VerifyNCName ("foo:bar:baz");
				Fail ();
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
			AssertEquals ("PT0S", XmlConvert.ToString (TimeSpan.FromSeconds (0)));
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
				AssertType.Fail (s + "should fail");
			} catch (XmlException ex) {
				if (success)
					AssertType.Fail (s + "should not fail");
			}
		}

		[Test]
		public void XmlDateTimeSerializationModeAndMaxValue ()
		{
			AssertEquals ("#1", "9999-12-31T23:59:59.9999999", XmlConvert.ToString (DateTime.MaxValue, XmlDateTimeSerializationMode.Unspecified).Substring (0, 27));
			AssertEquals ("#2", "9999-12-31T23:59:59.9999999Z", XmlConvert.ToString (DateTime.MaxValue, XmlDateTimeSerializationMode.Utc));
			AssertEquals ("#3", "9999-12-31T23:59:59.9999999", XmlConvert.ToString (DateTime.MaxValue, XmlDateTimeSerializationMode.RoundtripKind));
			AssertEquals ("#4", "9999-12-31T23:59:59.9999999", XmlConvert.ToString (DateTime.MaxValue, XmlDateTimeSerializationMode.Local).Substring (0, 27));
			// direct formatting string - no difference
			AssertEquals ("#5", "9999-12-31T23:59:59.9999999Z", XmlConvert.ToString (DateTime.MaxValue, "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ"));
			AssertEquals ("#6", "9999-12-31T23:59:59.9999999", XmlConvert.ToString (DateTime.MaxValue, "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz").Substring (0, 27));
		}

		[Test]
		public void XmlDateTimeSerializationModeRountripKind ()
		{
			string format = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
			string s = XmlConvert.ToString (DateTime.UtcNow, format);
			AssertType.AreEqual ('Z', s [s.Length -1], "#1-1");
			// LAMESPEC: .NET has a bug here that 'K' in format string does not reflect 'Z' as Utc Kind.
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=307694
			//AssertType.AreEqual (DateTimeKind.Utc, XmlConvert.ToDateTime (s, format).Kind, "#1-2");

			s = XmlConvert.ToString (DateTime.UtcNow, XmlDateTimeSerializationMode.RoundtripKind);
			AssertType.AreEqual ('Z', s [s.Length -1], "#2-1");
			AssertType.AreEqual (DateTimeKind.Utc, XmlConvert.ToDateTime (s, XmlDateTimeSerializationMode.RoundtripKind).Kind, "#2-2");
		}
		
		[Test]
		public void XmlDateTimeSerializationModeUnspecified ()
		{
			AssertEquals ("#1", 27, XmlConvert.ToString (new DateTime (DateTime.MaxValue.Ticks, DateTimeKind.Utc), XmlDateTimeSerializationMode.Unspecified).Length);
			DateTime dt1 = XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000+02:00", XmlDateTimeSerializationMode.Unspecified);
			DateTime dt2 = XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000", XmlDateTimeSerializationMode.Unspecified);
			AssertEquals ("#2", false, dt1 == dt2);
			XmlConvert.ToDateTime ("2006-05-30T09:48:32.0Z", XmlDateTimeSerializationMode.Unspecified);
			string format = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
			XmlConvert.ToDateTime (XmlConvert.ToString (DateTime.UtcNow, format), XmlDateTimeSerializationMode.Unspecified);
		}
		
		[Test]
		public void XmlDateTimeSerializationModeUtc ()
		{
			AssertEquals ("#1", 27, XmlConvert.ToString (new DateTime (DateTime.MaxValue.Ticks, DateTimeKind.Utc), XmlDateTimeSerializationMode.Unspecified).Length);
			DateTime dt1 = XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000+02:00", XmlDateTimeSerializationMode.Utc);
			DateTime dt2 = XmlConvert.ToDateTime ("0001-02-03T10:20:30.0000", XmlDateTimeSerializationMode.Utc);
			AssertEquals ("#2", false, dt1 == dt2);
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
				AssertType.Fail ("#1");
			} catch (FormatException) {
			}
			try {
				XmlConvert.ToDateTime ("0001-02-03T10:20", m);
				AssertType.Fail ("#2");
			} catch (FormatException) {
			}
			try {
				XmlConvert.ToDateTime ("0001-02-03T10:20:30.", m);
				AssertType.Fail ("#3");
			} catch (FormatException) {
			}
		}
#endif
	}
}

