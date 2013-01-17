//
// QueryStringConverterTest.cs
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
using System.ComponentModel;
using System.Globalization;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class QueryStringConverterTest
	{
		QueryStringConverter c;

		[SetUp]
		public void Setup ()
		{
			c = new QueryStringConverter ();
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
			Assert.IsFalse (c.CanConvert (typeof (XmlQualifiedName)), "#16");
			Assert.IsTrue (c.CanConvert (typeof (object)), "#17");
			Assert.IsFalse (c.CanConvert (typeof (QueryStringConverter)), "#18");
			// TypeConverterAttribute does not help it.
			Assert.IsFalse (c.CanConvert (typeof (MyConvertible)), "#19");
			Assert.IsTrue (c.CanConvert (typeof (DemoEnum)), "#20");
		}

		// ConvertStringToValue

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertValueToStringInvalidCast ()
		{
			c.ConvertValueToString ("ABC", typeof (char));
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertValueToStringInvalidCast2 ()
		{
			c.ConvertValueToString (123, typeof (string));
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertValueToStringInvalidCast3 ()
		{
			c.ConvertValueToString ("123", typeof (int));
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertValueToStringInvalidCast4 ()
		{
			c.ConvertValueToString (123.45, typeof (int));
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ConvertValueToStringInvalidCast5 ()
		{
			// umm...
			c.ConvertValueToString (123, typeof (double));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertValueToStringNullToValueType ()
		{
			c.ConvertValueToString (null, typeof (char));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertValueToStringDbNull ()
		{
			c.ConvertValueToString (DBNull.Value, typeof (DBNull));
		}

		[Test]
		public void ConvertValueToStringNullToString ()
		{
			Assert.IsNull (c.ConvertValueToString (null, typeof (string)));
		}

		[Test]
		public void ConvertValueToString ()
		{
			Assert.AreEqual ("A", c.ConvertValueToString ('A', typeof (char)), "#1");
			Assert.AreEqual ("}}}", c.ConvertValueToString ("}}}", typeof (string)), "#2");
			Assert.AreEqual ("123", c.ConvertValueToString (123.0, typeof (double)), "#3");
		}

		[Test]
		public void ConvertValueToStringEnum ()
		{
			string stringValue = c.ConvertValueToString (DemoEnum.Value2, typeof (DemoEnum));
			Assert.AreEqual ("Value2", stringValue);
		}

		// ConvertStringToValue

		[Test]
		public void ConvertStringToValueEnum ()
		{
			Assert.AreEqual (DemoEnum.Value3, (DemoEnum)c.ConvertStringToValue ("Value3", typeof(DemoEnum)));
			Assert.AreEqual (DemoEnum.Value2, (DemoEnum)c.ConvertStringToValue ("value2", typeof(DemoEnum)));
		}

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
		public void ConvertStringToValueNullToValueType ()
		{
			// hmm, it passes.
			Assert.AreEqual (default (char), c.ConvertStringToValue (null, typeof (char)));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertStringToValueDbNull ()
		{
			c.ConvertStringToValue (null, typeof (DBNull));
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
			Assert.AreEqual ("}}}", c.ConvertStringToValue ("}}}", typeof (string)), "#2");
			Assert.AreEqual (123.0, c.ConvertStringToValue ("123.0", typeof (double)), "#3");
			Assert.AreEqual (123.0, c.ConvertStringToValue ("123", typeof (double)), "#4");
		}

		// Types

		public enum DemoEnum
		{
			Value1,
			Value2,
			Value3,
			Value4,
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
}
