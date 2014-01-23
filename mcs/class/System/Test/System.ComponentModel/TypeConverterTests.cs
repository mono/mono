//
// System.ComponentModel.TypeConverter test cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2005 Novell, Inc. (http://www.ximian.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class TypeConverterTests
	{
		[Test]
		public void DefaultImplementation ()
		{
			BConverter converter = new BConverter ();
			C c = new C ();

			Assert.IsNull (converter.GetProperties (c), "#1");
			Assert.IsNull (converter.GetProperties (null, c), "#2");
			Assert.IsNull (converter.GetProperties (null, c, null), "#3");

			Assert.IsNull (converter.GetProperties (null), "#4");
			Assert.IsNull (converter.GetProperties (null, null), "#5");
			Assert.IsNull (converter.GetProperties (null, null, null), "#6");
			Assert.IsFalse (converter.GetCreateInstanceSupported (), "#7");
			Assert.IsFalse (converter.GetCreateInstanceSupported (null), "#8");
			Assert.IsFalse (converter.GetPropertiesSupported (), "#9");
			Assert.IsFalse (converter.GetPropertiesSupported (null), "#10");

			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "#11");
			Assert.IsTrue (converter.CanConvertFrom (null, typeof (InstanceDescriptor)), "#12");
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#13");
			Assert.IsTrue (converter.CanConvertTo (null, typeof (string)), "#14");
		}

		[Test]
#if TARGET_JVM
		[Ignore ("TD BUG ID: 7229")]
#endif
		public void GetProperties ()
		{
			PropertyDescriptorCollection properties = null;
			C c = new C ();
			TypeConverter converter = TypeDescriptor.GetConverter (c);

			Assert.AreEqual (typeof (AConverter).FullName, converter.GetType ().FullName, "#1");

			properties = converter.GetProperties (c);
			Assert.AreEqual (1, properties.Count, "#2");
			Assert.AreEqual ("A", properties[0].Name, "#3");

			// ensure collection is read-only
			try {
				properties.Clear ();
				Assert.Fail ("#4");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			properties = converter.GetProperties (null, c);
			Assert.AreEqual (1, properties.Count, "#5");
			Assert.AreEqual ("A", properties[0].Name, "#6");


			// ensure collection is read-only
			try {
				properties.Clear ();
				Assert.Fail ("#7");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			properties = converter.GetProperties (null, c, null);
			Assert.AreEqual (2, properties.Count, "#8");

			// ensure collection is read-only
			try {
				properties.Clear ();
				Assert.Fail ("#9");
			} catch (NotSupportedException) {
				// read-only collection cannot be modified
			}

			properties = converter.GetProperties (null);
			Assert.IsNull (properties, "#10");

			properties = converter.GetProperties (null, null);
			Assert.IsNull (properties, "#11");

			properties = converter.GetProperties (null, null, null);
			Assert.IsNull (properties, "#12");
		}

		[Test]
		public void GetConvertFromException ()
		{
			MockTypeConverter converter = new MockTypeConverter ();

			try {
				converter.GetConvertFromException (null);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// MockTypeConverter cannot convert from (null)
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (MockTypeConverter).Name) != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("(null)") != -1, "#A6");
			}

			try {
				converter.GetConvertFromException ("B");
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// MockTypeConverter cannot convert from System.String
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (MockTypeConverter).Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).FullName) != -1, "#B6");
			}
		}

		[Test]
		public void GetConvertToException ()
		{
			MockTypeConverter converter = new MockTypeConverter ();

			try {
				converter.GetConvertToException (null, typeof (DateTime));
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				// 'MockTypeConverter' is unable to convert '(null)'
				// to 'System.DateTime'
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (MockTypeConverter).Name + "'") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'(null)'") != -1, "#A6");
				Assert.IsTrue (ex.Message.IndexOf ("'" + typeof (DateTime).FullName + "'") != -1, "#A7");
			}

			try {
				converter.GetConvertToException ("B", typeof (DateTime));
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// 'MockTypeConverter' is unable to convert 'System.String'
				// to 'System.DateTime'
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (MockTypeConverter).Name) != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).FullName) != -1, "#B6");
			}
		}
		
		[Test]
		public void ConvertToWithCulture ()
		{
			var culture = CultureInfo.CreateSpecificCulture ("sv-se");
			
			var converter = TypeDescriptor.GetConverter (typeof (string));
			Assert.AreEqual ("0,5", (string) converter.ConvertTo (null, culture, 0.5, typeof (string)));
		}

		public class FooConverter<T> : TypeConverter
		{
		}

		[TypeConverter (typeof (FooConverter<string>))]
		public string FooProperty {
			get { return ""; }
		}

		[Test]
		public void TestGenericTypeConverterInstantiation ()
		{
			Assert.IsNotNull (GetType ().GetProperty ("FooProperty").GetCustomAttributes (false));
		}

		[ExpectedException (typeof (NullReferenceException))]
		public void GetConvertToException_DestinationType_Null ()
		{
			MockTypeConverter converter = new MockTypeConverter ();
			converter.GetConvertToException ("B", (Type) null);
		}

		[Test]
		public void IsValid ()
		{
			var tc = new TypeConverter ();
			Assert.IsFalse (tc.IsValid (null));
		}
	}

	[TypeConverter (typeof (AConverter))]
	public class C
	{
		[Browsable (true)]
		public int A {
			get { return 0; }
		}

		[Browsable (false)]
		public int B {
			get { return 0; }
		}
	}

	public class MockTypeConverter : TypeConverter
	{
		public new Exception GetConvertFromException (object value)
		{
			return base.GetConvertFromException (value);
		}

		public new Exception GetConvertToException (object value, Type destinationType)
		{
			return base.GetConvertToException (value, destinationType);
		}
	}

	public class AConverter : TypeConverter
	{
		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			if (value is C) {
				return TypeDescriptor.GetProperties (value, attributes);
			}
			return base.GetProperties (context, value, attributes);
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}

	public class BConverter : TypeConverter
	{
	}
}
