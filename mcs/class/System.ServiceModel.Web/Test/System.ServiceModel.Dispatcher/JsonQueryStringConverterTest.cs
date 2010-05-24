//
// JsonQueryStringConverterTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class JsonQueryStringConverterTest
	{
		JsonQueryStringConverter c;

		[SetUp]
		public void Setup ()
		{
			c = new JsonQueryStringConverter ();
		}

		[Test]
		public void CanConvert ()
		{
			Assert.IsTrue (c.CanConvert (typeof (bool)), "#1");
			Assert.IsTrue (c.CanConvert (typeof (char)), "#2");
			Assert.IsTrue (c.CanConvert (typeof (double)), "#3");
			Assert.IsTrue (c.CanConvert (typeof (decimal)), "#4");
			Assert.IsTrue (c.CanConvert (typeof (float)), "#5");
			Assert.IsTrue (c.CanConvert (typeof (string)), "#6");
			Assert.IsTrue (c.CanConvert (typeof (int)), "#7");
			Assert.IsTrue (c.CanConvert (typeof (byte)), "#8");
			Assert.IsTrue (c.CanConvert (typeof (sbyte)), "#9");
			Assert.IsTrue (c.CanConvert (typeof (long)), "#10");
			Assert.IsTrue (c.CanConvert (typeof (ulong)), "#11");
			Assert.IsTrue (c.CanConvert (typeof (DateTime)), "#12");
			Assert.IsTrue (c.CanConvert (typeof (DateTimeOffset)), "#13");
			Assert.IsTrue (c.CanConvert (typeof (TimeSpan)), "#14");
			Assert.IsTrue (c.CanConvert (typeof (Guid)), "#15");
			Assert.IsTrue (c.CanConvert (typeof (XmlQualifiedName)), "#16");
			Assert.IsTrue (c.CanConvert (typeof (object)), "#17");
			Assert.IsTrue (c.CanConvert (typeof (QueryStringConverter)), "#18");
			// TypeConverterAttribute does not help it.
			Assert.IsFalse (c.CanConvert (typeof (MyConvertible)), "#19");
			Assert.IsTrue (c.CanConvert (typeof (MyPublicClass)), "#20");
			Assert.IsTrue (c.CanConvert (typeof (MyNestedPublicClass)), "#21");
			Assert.IsTrue (c.CanConvert (typeof (MyNestedPrivateClass)), "#22");
			Assert.IsTrue (c.CanConvert (typeof (List<int>)), "#23");
			Assert.IsTrue (c.CanConvert (typeof (List<MyPublicClass>)), "#24");
			// FIXME: enable it
			//Assert.IsFalse (c.CanConvert (typeof (List<MyInternalClass>)), "#25");
		}

		// ConvertValueToString

		[Test]
		public void ConvertValueToStringValidCast ()
		{
			Assert.AreEqual ("\"ABC\"", c.ConvertValueToString ("ABC", typeof (char)));
		}

		[Test]
		[Ignore ("huh? .NET converts to 123, not \"123\"")]
		public void ConvertValueToStringValidCast2 ()
		{
			Assert.AreEqual ("\"123\"", c.ConvertValueToString (123, typeof (string)));
		}

		[Test]
		[Ignore ("huh? .NET converts to \"123\", not 123")]
		public void ConvertValueToStringValidCast3 ()
		{
			Assert.AreEqual ("123", c.ConvertValueToString ("123", typeof (int)));
		}

		[Test]
		public void ConvertValueToStringValidCast4 ()
		{
			Assert.AreEqual ("123.45", c.ConvertValueToString (123.45, typeof (int)));
		}

		[Test]
		public void ConvertValueToStringValidCast5 ()
		{
			Assert.AreEqual ("123", c.ConvertValueToString (123, typeof (double)));
		}

		[Test]
		public void ConvertValueToStringValidCast6 ()
		{
			// umm... should be out of range
			Assert.AreEqual ("12345", c.ConvertValueToString (12345, typeof (byte)));
		}

		[Test]
		public void ConvertValueToStringNullToValueType ()
		{
			Assert.AreEqual (null, c.ConvertValueToString (null, typeof (char)));
		}

		[Test]
		public void ConvertValueToStringDbNull ()
		{
			Assert.AreEqual ("{}", c.ConvertValueToString (DBNull.Value, typeof (DBNull)));
		}

		[Test]
		public void ConvertValueToStringDbNull2 ()
		{
			Assert.AreEqual ("\"\"", c.ConvertValueToString ("", typeof (DBNull)));
		}

		[Test]
		public void ConvertValueToStringDbNull3 ()
		{
			Assert.AreEqual (null, c.ConvertValueToString (null, typeof (DBNull)));
		}

		[Test]
		public void ConvertValueToStringDbNull4 ()
		{
			// ... so, DBNull is just converted to String
			Assert.AreEqual ("\"ABC\"", c.ConvertValueToString ("ABC", typeof (DBNull)));
		}

		[Test]
		public void ConvertValueToStringQName ()
		{
			Assert.AreEqual ("\"foo:\"", c.ConvertValueToString (new XmlQualifiedName ("foo"), typeof (XmlQualifiedName)), "#1");
			Assert.AreEqual ("\"foo:urn:bar\"", c.ConvertValueToString (new XmlQualifiedName ("foo", "urn:bar"), typeof (XmlQualifiedName)), "#2");
		}

		[Test]
		public void ConvertValueToStringNullToString ()
		{
			Assert.IsNull (c.ConvertValueToString (null, typeof (string)));
		}

		[Test]
		public void ConvertValueToString ()
		{
			Assert.AreEqual ("\"A\"", c.ConvertValueToString ('A', typeof (char)), "#1");
			Assert.AreEqual ("\"}}}\"", c.ConvertValueToString ("}}}", typeof (string)), "#2");
			Assert.AreEqual ("123", c.ConvertValueToString (123.0, typeof (double)), "#3");
		}

		// ConvertStringToValue

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertStringToValueInvalidCast ()
		{
			c.ConvertStringToValue ("ABC", typeof (char));
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		[Category ("NotWorking")]
		public void ConvertStringToValueInvalidCast2 ()
		{
			c.ConvertStringToValue ("-123", typeof (uint));
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertStringToValueInvalidCast4 ()
		{
			c.ConvertStringToValue ("123.45", typeof (int));
		}

		[Test]
		public void ConvertStringToValueString1 ()
		{
			// hmm ...
			Assert.AreEqual ("-123.45", c.ConvertStringToValue ("-123.45", typeof (string)));
		}

		[Test]
		public void ConvertStringToValueString2 ()
		{
			Assert.AreEqual ("ABC", c.ConvertStringToValue ("\"ABC\"", typeof (string)));
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void ConvertStringToValueString3 ()
		{
			// missing closing '"'
			Assert.AreEqual ("\"ABC", c.ConvertStringToValue ("\"ABC", typeof (string)));
			// this test exposes that .NET uses DataContractJsonSerializer internally.
		}

		[Test]
		public void ConvertStringToValueNullToValueType ()
		{
			// hmm, it passes.
			Assert.AreEqual (default (char), c.ConvertStringToValue (null, typeof (char)));
		}

		[Test]
		public void ConvertStringToValueDbNull ()
		{
			Assert.AreEqual (null, c.ConvertStringToValue (null, typeof (DBNull)));
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void ConvertStringToValueDbNull2 ()
		{
			c.ConvertStringToValue ("", typeof (DBNull));
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void ConvertStringToValueDbNull3 ()
		{
			c.ConvertStringToValue ("ABC", typeof (DBNull));
		}

		[Test]
		public void ConvertStringToValueDbNull4 ()
		{
			Assert.AreEqual (DBNull.Value, c.ConvertStringToValue ("{}", typeof (DBNull)));
		}

		[Test]
		public void ConvertStringToValueNullToString ()
		{
			Assert.IsNull (c.ConvertStringToValue (null, typeof (string)));
		}

		[Test]
		public void ConvertStringToValue ()
		{
			Assert.AreEqual ('A', c.ConvertStringToValue ("A", typeof (char)), "#1");
			// likely .NET bug: it should fail to deserialize as it is not a valid JSON string.
			//Assert.AreEqual ("}}}", c.ConvertStringToValue ("}}}", typeof (string)), "#2");
			Assert.AreEqual (123.0, c.ConvertStringToValue ("123.0", typeof (double)), "#3");
			Assert.AreEqual (123.0, c.ConvertStringToValue ("123", typeof (double)), "#4");
			Assert.AreEqual ("A", c.ConvertStringToValue ("\"A\"", typeof (string)), "#5");
			// LAMESPEC: it should either always expect or preserve double-quotes. This behavior is just inconsistent.
			Assert.AreEqual ("A", c.ConvertStringToValue ("A", typeof (string)), "#6");
			Assert.AreEqual ("A%22B", c.ConvertStringToValue ("A%22B", typeof (string)), "#7"); // it's not either covered by url escaping.
		}

		// Types

		public class MyNestedPublicClass
		{
		}

		public class MyNestedPrivateClass
		{
		}
	}

	// non-nested types

	public class MyPublicClass
	{
	}

	class MyInternalClass
	{
	}

	[TypeConverter (typeof (MyTypeConverter))]
	class MyConvertible
	{
	}

	class MyTypeConverter : TypeConverter
	{
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string))
				return "hogehoge";
			throw new Exception ();
		}
	}
}
