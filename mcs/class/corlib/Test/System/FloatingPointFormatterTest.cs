// FloatingPointFormatterTest.cs - NUnit Test Cases for the System.FloatingPointFormatter class
//
// Authors:
// 	Duncan Mak (duncan@ximian.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell Inc.
// 

using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;

namespace MonoTests.System
{
	[TestFixture]
	public class FloatingPointFormatterTest : Assertion
	{
		[Test]
		public void Format1 ()
		{
			AssertEquals ("F1", "100000000000000", 1.0e+14.ToString ());
			AssertEquals ("F2", "1E+15", 1.0e+15.ToString ());
			AssertEquals ("F3", "1E+16", 1.0e+16.ToString ());
			AssertEquals ("F4", "1E+17", 1.0e+17.ToString ());
		}

		[Test]
		public void FormatStartsWithDot ()
		{
			CultureInfo ci = new CultureInfo ("en-US");
			double val = 12345.1234567890123456;
			string s = val.ToString(".0################;-.0################;0.0", ci);
			AssertEquals ("#1", "12345.123456789", s);

			s = (-val).ToString(".0################;-.0#######;#########;0.0", ci);
			AssertEquals ("#2", "-12345.12345679", s);

			s = 0.0.ToString(".0################;-.0#######;+-0", ci);
			AssertEquals ("#3", "+-0", s);
		}

		[Test]
		public void Permille ()
		{
			CultureInfo ci = CultureInfo.InvariantCulture;
			AssertEquals ("485.7\u2030", (0.4857).ToString ("###.###\u2030"));
		}
        }
}
