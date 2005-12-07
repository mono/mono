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
// PropertyConverterTest.cs
//
// Author:
//	Jackson Harper (jackson@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//


using System;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;

using NUnit.Framework;

namespace MonoTests.System.Web.UI {

	enum TestEnum {
		Default,
		Normal,
		LowerCase,
		UpperCase,
	}

	[Flags]
	enum TestFlags {
		A,
		B,
		C
	}

	[TestFixture]
	public class PropertyConverterTest {

		[Test]
		public void EnumFromString ()
		{
			object e = TestEnum.Default;

			e = PropertyConverter.EnumFromString (typeof (TestEnum), "Normal");
			Assert.AreEqual (TestEnum.Normal, e, "Normal");

			e = PropertyConverter.EnumFromString (typeof (TestEnum), "lowercase");
			Assert.AreEqual (TestEnum.LowerCase, e, "Lower Case");

			e = PropertyConverter.EnumFromString (typeof (TestEnum), "UPPERCASE");
			Assert.AreEqual (TestEnum.UpperCase, e, "Upper Case");

			e = PropertyConverter.EnumFromString (typeof (TestEnum), "DoesntExist");
			Assert.AreEqual (null, e, "Doesn't Exist");

			e = PropertyConverter.EnumFromString (typeof (TestEnum), "TestEnum.Normal");
			Assert.AreEqual (null, e, "Full Name");
		}

		[Test]
		public void TestFromStringFlags ()
		{
			object e = TestEnum.Default;

			e = PropertyConverter.EnumFromString (typeof (TestFlags), "A");
			Assert.AreEqual (e, TestFlags.A, "Normal");

			e = PropertyConverter.EnumFromString (typeof (TestFlags), "A, B");
			Assert.AreEqual (e, TestFlags.A | TestFlags.B, "Multiple");

			e = PropertyConverter.EnumFromString (typeof (TestFlags), "foo");
			Assert.AreEqual (e, null, "Bad");
		}

		[Test]
		public void EnumToString ()
		{
			Assert.AreEqual (PropertyConverter.EnumToString (typeof (TestEnum), 1),
					"Normal", "Normal");
			Assert.AreEqual (PropertyConverter.EnumToString (typeof (TestEnum), 25),
					"25", "Decimal");
		}

		[Test]
		public void EnumToStringFlags ()
		{
			Assert.AreEqual (PropertyConverter.EnumToString (typeof (TestFlags), 0),
					"A", "A");
			Assert.AreEqual (PropertyConverter.EnumToString (typeof (TestFlags), 3),
					"B, C", "Multiple");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void EnumToStringWrongBaseType ()
		{
			PropertyConverter.EnumToString (typeof (TestEnum), "foo");
		}

		public void TestObjectFromString ()
		{
			Assert.AreEqual (PropertyConverter.ObjectFromString (
						 typeof (string), null, "value"),
					"value", "String Type");      
			MemberInfo mi = this.GetType ().GetProperty ("AllowedConverterProperty");
			Assert.AreEqual (PropertyConverter.ObjectFromString (
						 typeof (int), mi, "ConverterValue"),
					"ConverterValue", "Converter Value");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestObjectFromStringNullRef ()
		{
			PropertyConverter.ObjectFromString (typeof (int), // can't be string
					null, "foobar");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void TestObjectFromStringCantConvert ()
		{
			MemberInfo mi = this.GetType ().GetProperty ("NotAllowedConverterProperty");
			PropertyConverter.ObjectFromString (typeof (int), // can't be string
					mi, "foobar");
		}

		[TypeConverter (typeof (AllowedMagicTypeConverter))]
		public string AllowedConverterProperty {
			get { return "AllowedConverterProperty"; }
		}

		[TypeConverter (typeof (NotAllowedMagicTypeConverter))]
		public string NotAllowedConverterProperty {
			get { return "NotAllowedConverterProperty"; }
		}

		public class AllowedMagicTypeConverter : MagicTypeConverter {

			public AllowedMagicTypeConverter () : base (true)
			{
			}
		}

		public class NotAllowedMagicTypeConverter : MagicTypeConverter {

			public NotAllowedMagicTypeConverter () : base (false)
			{
			}
		}

		public abstract class MagicTypeConverter : TypeConverter {

			private bool allow;

			public MagicTypeConverter (bool allow)
			{
				this.allow = allow;
			}

			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				return allow;
			}

			public override object ConvertFrom (ITypeDescriptorContext context,
					CultureInfo culture, object value)
			{
				return "ConverterValue";
			}
		}
	}
}
