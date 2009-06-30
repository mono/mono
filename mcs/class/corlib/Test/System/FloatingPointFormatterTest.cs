// FloatingPointFormatterTest.cs - NUnit Test Cases for the System.FloatingPointFormatter class
//
// Authors:
// 	Duncan Mak (duncan@ximian.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell Inc.
// 

using System;
using System.Globalization;
using System.IO;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class FloatingPointFormatterTest 
	{
		CultureInfo old_culture;

		[SetUp]
		public void SetUp ()
		{
			old_culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = old_culture;
		}

		[Test]
		public void Format1 ()
		{
			Assert.AreEqual ("100000000000000", 1.0e+14.ToString (), "F1");
			Assert.AreEqual ("1E+15", 1.0e+15.ToString (), "F2");
			Assert.AreEqual ("1E+16", 1.0e+16.ToString (), "F3");
			Assert.AreEqual ("1E+17", 1.0e+17.ToString (), "F4");
		}

		[Test]
		public void FormatStartsWithDot ()
		{
			CultureInfo ci = new CultureInfo ("en-US");
			double val = 12345.1234567890123456;
			string s = val.ToString(".0################;-.0################;0.0", ci);
			Assert.AreEqual ("12345.123456789", s, "#1");

			s = (-val).ToString(".0################;-.0#######;#########;0.0", ci);
			Assert.AreEqual ("-12345.12345679", s, "#2");

			s = 0.0.ToString(".0################;-.0#######;+-0", ci);
			Assert.AreEqual ("+-0", s, "#3");
		}

		[Test]
		public void PermillePercent ()
		{
			CultureInfo ci = CultureInfo.InvariantCulture;
			Assert.AreEqual ("485.7\u2030", (0.4857).ToString ("###.###\u2030"), "#1");
			NumberFormatInfo nfi = new NumberFormatInfo ();
			nfi.NegativeSign = "";
			nfi.PerMilleSymbol = "m";
			nfi.PercentSymbol = "percent";
			Assert.AreEqual ("m485.7", 0.4857.ToString ("\u2030###.###", nfi), "#2");
			Assert.AreEqual ("485.7m", 0.4857.ToString ("###.###\u2030", nfi), "#3");
			Assert.AreEqual ("percent48.57", 0.4857.ToString ("%###.###", nfi), "#4");
			Assert.AreEqual ("48.57percent", 0.4857.ToString ("###.###%", nfi), "#5");
		}

		[Test]
		public void LiteralMixed ()
		{
			CultureInfo ci = CultureInfo.InvariantCulture;
			Assert.AreEqual ("test 235", 234.56.ToString ("'test' ###", ci), "#1");
			Assert.AreEqual ("235 test", 234.56.ToString ("### 'test'", ci), "#2");
			Assert.AreEqual ("234 test.56", 234.56.ToString ("### 'test'.###", ci), "#3");
			Assert.AreEqual ("hoge 235", 234.56.ToString ("'hoge' ###", ci), "#1");
			Assert.AreEqual ("235 hoge", 234.56.ToString ("### 'hoge'", ci), "#2");
			Assert.AreEqual ("234 hoge.56", 234.56.ToString ("### 'hoge'.###", ci), "#3");
		}
	}
}
