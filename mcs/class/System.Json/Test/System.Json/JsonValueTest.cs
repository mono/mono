//
// JsonValueTest.cs: Tests for JSonValue
//
// Copyright 2011 Xamarin, Inc.
//
// Authors:
//   Miguel de Icaza
//
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Json;
using System.Globalization;
using System.Threading;

namespace MonoTests.System
{
	[TestFixture]
	public class JsonValueTests {
		// Tests that a trailing comma is allowed in dictionary definitions
		[Test]
		public void LoadWithTrailingComma ()
		{
			var j = JsonValue.Load (new StringReader ("{ \"a\": \"b\",}"));
			Assert.AreEqual (1, j.Count, "itemcount");
			Assert.AreEqual (JsonType.String, j ["a"].JsonType, "type");
			Assert.AreEqual ("b", (string) j ["a"], "value");

			JsonValue.Parse ("[{ \"a\": \"b\",}]");
		}

		[Test]
		public void LoadWithTrailingComma2 ()
		{
			JsonValue.Parse ("[{ \"a\": \"b\",}]");
		}

		// Test that we correctly serialize JsonArray with null elements.
		[Test]
		public void ToStringOnJsonArrayWithNulls () {
			var j = JsonValue.Load (new StringReader ("[1,2,3,null]"));
			Assert.AreEqual (4, j.Count, "itemcount");
			Assert.AreEqual (JsonType.Array, j.JsonType, "type");
			var str = j.ToString ();
			Assert.AreEqual (str, "[1, 2, 3, null]");
		}

		[Test]
		public void QuoteEscapeBug_20869 () 
		{
			Assert.AreEqual ((new JsonPrimitive ("\"\"")).ToString (), "\"\\\"\\\"\"");
		}

		void ExpectError (string s)
		{
			try {
				JsonValue.Parse (s);
				Assert.Fail ("Expected ArgumentException for `" + s + "'");
			} catch (ArgumentException) {
			}
		}

		// Test whether an exception is thrown for invalid JSON
		[Test]
		public void CheckErrors () 
		{
			ExpectError (@"-");
			ExpectError (@"- ");
			ExpectError (@"1.");
			ExpectError (@"1. ");
			ExpectError (@"1e+");
			ExpectError (@"1 2");
			ExpectError (@"077");

			ExpectError (@"[1,]");

			//ExpectError (@"{""a"":1,}"); // Not valid JSON, allowed anyway
		}

		// Parse a json string and compare to the expected value
		void CheckDouble (double expected, string json)
		{
			double jvalue = (double) JsonValue.Parse (json);
			Assert.AreEqual (expected, jvalue);
		}

		// Convert a number to json and parse the string, then compare the result to the original value
		void CheckDouble (double number)
		{
			double jvalue = (double) JsonValue.Parse (new JsonPrimitive (number).ToString ());
			Assert.AreEqual (number, jvalue); // should be exactly the same
		}

		[Test]
		public void CheckNumbers () 
		{
			CheckDouble (0, "0");
			CheckDouble (0, "-0");
			CheckDouble (0, "0.00");
			CheckDouble (0, "-0.00");
			CheckDouble (1, "1");
			CheckDouble (1.1, "1.1");
			CheckDouble (-1, "-1");
			CheckDouble (-1.1, "-1.1");
			CheckDouble (1e-10, "1e-10");
			CheckDouble (1e+10, "1e+10");
			CheckDouble (1e-30, "1e-30");
			CheckDouble (1e+30, "1e+30");

			CheckDouble (1, "\"1\"");
			CheckDouble (1.1, "\"1.1\"");
			CheckDouble (-1, "\"-1\"");
			CheckDouble (-1.1, "\"-1.1\"");

			CheckDouble (double.NaN, "\"NaN\"");
			CheckDouble (double.PositiveInfinity, "\"Infinity\"");
			CheckDouble (double.NegativeInfinity, "\"-Infinity\"");

			ExpectError ("NaN");
			ExpectError ("Infinity");
			ExpectError ("-Infinity");

			Assert.AreEqual ("1.1", new JsonPrimitive (1.1).ToString ());
			Assert.AreEqual ("-1.1", new JsonPrimitive (-1.1).ToString ());
			Assert.AreEqual ("1E-20", new JsonPrimitive (1e-20).ToString ());
			Assert.AreEqual ("1E+20", new JsonPrimitive (1e+20).ToString ());
			Assert.AreEqual ("1E-30", new JsonPrimitive (1e-30).ToString ());
			Assert.AreEqual ("1E+30", new JsonPrimitive (1e+30).ToString ());
			Assert.AreEqual ("\"NaN\"", new JsonPrimitive (double.NaN).ToString ());
			Assert.AreEqual ("\"Infinity\"", new JsonPrimitive (double.PositiveInfinity).ToString ());
			Assert.AreEqual ("\"-Infinity\"", new JsonPrimitive (double.NegativeInfinity).ToString ());

			Assert.AreEqual ("1E-30", JsonValue.Parse ("1e-30").ToString ());
			Assert.AreEqual ("1E+30", JsonValue.Parse ("1e+30").ToString ());

			CheckDouble (1);
			CheckDouble (1.1);
			CheckDouble (1.25);
			CheckDouble (-1);
			CheckDouble (-1.1);
			CheckDouble (-1.25);
			CheckDouble (1e-20);
			CheckDouble (1e+20);
			CheckDouble (1e-30);
			CheckDouble (1e+30);
			CheckDouble (3.1415926535897932384626433);
			CheckDouble (3.1415926535897932384626433e-20);
			CheckDouble (3.1415926535897932384626433e+20);
			CheckDouble (double.NaN);
			CheckDouble (double.PositiveInfinity);
			CheckDouble (double.NegativeInfinity);
			CheckDouble (double.MinValue);
			CheckDouble (double.MaxValue);

			// A number which needs 17 digits (see http://stackoverflow.com/questions/6118231/why-do-i-need-17-significant-digits-and-not-16-to-represent-a-double)
			CheckDouble (18014398509481982.0);

			// Values around the smallest positive decimal value
			CheckDouble (1.123456789e-29);
			CheckDouble (1.123456789e-28);

			CheckDouble (1.1E-29, "0.000000000000000000000000000011");
			// This is being parsed as a decimal and rounded to 1e-28, even though it can be more accurately be represented by a double
			//CheckDouble (1.1E-28, "0.00000000000000000000000000011");
		}

		// Retry the test with different locales
		[Test]
		public void CheckNumbersCulture () 
		{
			CultureInfo old = Thread.CurrentThread.CurrentCulture;
			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("en");
				CheckNumbers ();
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("fr");
				CheckNumbers ();
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("de");
				CheckNumbers ();
			} finally {
				Thread.CurrentThread.CurrentCulture = old;
			}
		}
	}
}
