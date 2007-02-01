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
