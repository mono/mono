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
			((JsonArray) j).Add (null);
			str = j.ToString ();
			Assert.AreEqual (str, "[1, 2, 3, null, null]");
		}

		// Test that we correctly serialize JsonObject with null elements.
		[Test]
		public void ToStringOnJsonObjectWithNulls () {
			var j = JsonValue.Load (new StringReader ("{\"a\":null,\"b\":2}"));
			Assert.AreEqual (2, j.Count, "itemcount");
			Assert.AreEqual (JsonType.Object, j.JsonType, "type");
			var str = j.ToString ();
			Assert.AreEqual (str, "{\"a\": null, \"b\": 2}");
		}

		[Test]
		public void JsonObjectOrder () {
			var obj = new JsonObject ();
			obj["a"] = 1;
			obj["c"] = 3;
			obj["b"] = 2;
			var str = obj.ToString ();
			Assert.AreEqual (str, "{\"a\": 1, \"b\": 2, \"c\": 3}");
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
		public void CheckIntegers ()
		{
			Assert.AreEqual (sbyte.MinValue, (sbyte) JsonValue.Parse (new JsonPrimitive (sbyte.MinValue).ToString ()));
			Assert.AreEqual (sbyte.MaxValue, (sbyte) JsonValue.Parse (new JsonPrimitive (sbyte.MaxValue).ToString ()));
			Assert.AreEqual (byte.MinValue, (byte) JsonValue.Parse (new JsonPrimitive (byte.MinValue).ToString ()));
			Assert.AreEqual (byte.MaxValue, (byte) JsonValue.Parse (new JsonPrimitive (byte.MaxValue).ToString ()));

			Assert.AreEqual (short.MinValue, (short) JsonValue.Parse (new JsonPrimitive (short.MinValue).ToString ()));
			Assert.AreEqual (short.MaxValue, (short) JsonValue.Parse (new JsonPrimitive (short.MaxValue).ToString ()));
			Assert.AreEqual (ushort.MinValue, (ushort) JsonValue.Parse (new JsonPrimitive (ushort.MinValue).ToString ()));
			Assert.AreEqual (ushort.MaxValue, (ushort) JsonValue.Parse (new JsonPrimitive (ushort.MaxValue).ToString ()));

			Assert.AreEqual (int.MinValue, (int) JsonValue.Parse (new JsonPrimitive (int.MinValue).ToString ()));
			Assert.AreEqual (int.MaxValue, (int) JsonValue.Parse (new JsonPrimitive (int.MaxValue).ToString ()));
			Assert.AreEqual (uint.MinValue, (uint) JsonValue.Parse (new JsonPrimitive (uint.MinValue).ToString ()));
			Assert.AreEqual (uint.MaxValue, (uint) JsonValue.Parse (new JsonPrimitive (uint.MaxValue).ToString ()));

			Assert.AreEqual (long.MinValue, (long) JsonValue.Parse (new JsonPrimitive (long.MinValue).ToString ()));
			Assert.AreEqual (long.MaxValue, (long) JsonValue.Parse (new JsonPrimitive (long.MaxValue).ToString ()));
			Assert.AreEqual (ulong.MinValue, (ulong) JsonValue.Parse (new JsonPrimitive (ulong.MinValue).ToString ()));
			Assert.AreEqual (ulong.MaxValue, (ulong) JsonValue.Parse (new JsonPrimitive (ulong.MaxValue).ToString ()));
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

		// Convert a string to json and parse the string, then compare the result to the original value
		void CheckString (string str)
		{
			var json = new JsonPrimitive (str).ToString ();
			// Check whether the string is valid Unicode (will throw for broken surrogate pairs)
			new UTF8Encoding (false, true).GetBytes (json);
			string jvalue = (string) JsonValue.Parse (json);
			Assert.AreEqual (str, jvalue);
		}
		
		// String handling: http://tools.ietf.org/html/rfc7159#section-7
		[Test]
		public void CheckStrings () 
		{
			Assert.AreEqual ("\"test\"", new JsonPrimitive ("test").ToString ());
			// Handling of characters
			Assert.AreEqual ("\"f\"", new JsonPrimitive ('f').ToString ());
			Assert.AreEqual ('f', (char) JsonValue.Parse ("\"f\""));

			// Control characters with special escape sequence
			Assert.AreEqual ("\"\\b\\f\\n\\r\\t\"", new JsonPrimitive ("\b\f\n\r\t").ToString ());
			// Other characters which must be escaped
			Assert.AreEqual (@"""\""\\""", new JsonPrimitive ("\"\\").ToString ());
			// Control characters without special escape sequence
			for (int i = 0; i < 32; i++)
				if (i != '\b' && i != '\f' && i != '\n' && i != '\r' && i != '\t')
					Assert.AreEqual ("\"\\u" + i.ToString ("x04") + "\"", new JsonPrimitive ("" + (char) i).ToString ());

			// JSON does not require U+2028 and U+2029 to be escaped, but
			// JavaScript does require this:
			// http://stackoverflow.com/questions/2965293/javascript-parse-error-on-u2028-unicode-character/9168133#9168133
			Assert.AreEqual ("\"\\u2028\\u2029\"", new JsonPrimitive ("\u2028\u2029").ToString ());

			// '/' also does not have to be escaped, but escaping it when
			// preceeded by a '<' avoids problems with JSON in HTML <script> tags
			Assert.AreEqual ("\"<\\/\"", new JsonPrimitive ("</").ToString ());
			// Don't escape '/' in other cases as this makes the JSON hard to read
			Assert.AreEqual ("\"/bar\"", new JsonPrimitive ("/bar").ToString ());
			Assert.AreEqual ("\"foo/bar\"", new JsonPrimitive ("foo/bar").ToString ());

			CheckString ("test\b\f\n\r\t\"\\/</\0x");
			for (int i = 0; i < 65536; i++)
				CheckString ("x" + ((char) i));

			// Check broken surrogate pairs
			CheckString ("\ud800");
			CheckString ("x\ud800");
			CheckString ("\udfff\ud800");
			CheckString ("\ude03\ud912");
			CheckString ("\uc000\ubfff");
			CheckString ("\udfffx");
			// Valid strings should not be escaped:
			Assert.AreEqual ("\"\ud7ff\"", new JsonPrimitive ("\ud7ff").ToString ());
			Assert.AreEqual ("\"\ue000\"", new JsonPrimitive ("\ue000").ToString ());
			Assert.AreEqual ("\"\ud800\udc00\"", new JsonPrimitive ("\ud800\udc00").ToString ());
			Assert.AreEqual ("\"\ud912\ude03\"", new JsonPrimitive ("\ud912\ude03").ToString ());
			Assert.AreEqual ("\"\udbff\udfff\"", new JsonPrimitive ("\udbff\udfff").ToString ());
		}
	}
}

// vim: noexpandtab
// Local Variables:
// tab-width: 4
// c-basic-offset: 4
// indent-tabs-mode: t
// End:
