//
// System.Xml.XmlConvertTests.cs
//
// Author: Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlConvertTests
	{
		private void AssertName (string result, string source)
		{
			Assertion.AssertEquals (result,
				XmlConvert.EncodeName (source));
		}

		private void AssertNmToken (string result, string source)
		{
			Assertion.AssertEquals (result,
				XmlConvert.EncodeNmToken (source));
		}

		[Test]
		public void EncodeName ()
		{
			AssertName ("Test", "Test");
			AssertName ("Hello_x0020_my_x0020_friends.", "Hello my friends.");
			AssertName ("_x0031_23", "123");
			AssertName ("_x005F_x0031_23", "_x0031_23");
		}

		[Test]
		public void EncodeNmToken ()
		{
			AssertNmToken ("Test", "Test");
			AssertNmToken ("Hello_x0020_my_x0020_friends.", "Hello my friends.");
			AssertNmToken ("123", "123");
			AssertNmToken ("_x005F_x0031_23", "_x0031_23");
		}

		[Test]
		public void DateToString ()
		{
			// Don't include TimeZone value for test value.
			string dateString = 
				XmlConvert.ToString (new DateTime (2003, 5, 5));
			Assertion.AssertEquals (33, dateString.Length);
			Assertion.AssertEquals ("2003-05-05T00:00:00.0000000", dateString.Substring (0, 27));
		}
	}
}
