//
// System.Xml.XmlConvertTests.cs
//
// Authors: Atsushi Enomoto (ginga@kit.hi-ho.ne.jp), Jon Kessler (jwkpiano1@comcast.net)
//
// (C) 2003 Atsushi Enomoto, Jon Kessler
//

using System;
using System.Xml;
using NUnit.Framework;

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
		public void ToDateTime ()//fails on Mono
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
		public void ToGuid ()
		{
			AssertEquals (new Guid ("ca761232-ed42-11ce-bacd-00aa0057b223"), XmlConvert.ToGuid ("ca761232-ed42-11ce-bacd-00aa0057b223"));
		}
	
		[Test]
		public void ToInt16 ()//not done
		{
			
		}
		
		[Test]
		public void ToInt32 ()//not done
		{
			
		}
		
		[Test]
		public void ToInt64 ()//not done
		{
			
		}
		
		[Test]
		public void ToSByte ()//not done
		{
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
		public void ToTimeSpan ()//not done
		{
			
		}
		
		[Test]
		public void ToUInt16 ()//not done
		{
			
		}
		
		[Test]
		public void ToUInt32 ()//not done
		{
			
		}
		
		[Test]
		public void ToUInt64 ()//not done
		{
			
		}
		
		[Test]
		public void VerifyName ()//not done
		{
			
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
	}
}

