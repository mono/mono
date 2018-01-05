//
// System.Configuration.SettingsPropertyValueTest.cs - Unit tests for
// System.Configuration.SettingsPropertyValue.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace MonoTests.System.Configuration {

	[TestFixture]
	public class SettingsPropertyValueTest {

		[Test]
		public void Properties ()
		{
			SettingsProperty p = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   10,
								   SettingsSerializeAs.String,
								   null,
								   true,
								   false);

			SettingsPropertyValue v = new SettingsPropertyValue (p);

			Assert.IsFalse (v.Deserialized, "A1");
			Assert.IsFalse (v.IsDirty, "A2");
			Assert.AreEqual ("property", v.Name, "A3");
			Assert.AreEqual (p, v.Property, "A4");
			Assert.AreEqual ((object)10, v.PropertyValue, "A5");
			Assert.AreEqual (null, v.SerializedValue, "A6");
			Assert.IsTrue (v.UsingDefaultValue, "A7");

			/* test that setting v.PropertyValue to
			 * something else causes SerializedValue to
			 * become not-null */
			v.PropertyValue = (object)5;
			Assert.AreEqual ("5", v.SerializedValue, "A9");

			/* test to see whether or not changing
			 * SerializeAs causes SerializedValue to
			 * change */
			p.SerializeAs = SettingsSerializeAs.Xml;
			Assert.AreEqual ("5", v.SerializedValue, "A11"); /* nope.. */

			/* only changing PropertyValue does */
			v.PropertyValue = (object)7;
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<int>7</int>", ((string)v.SerializedValue).Replace ("\r\n", "\n"), "A13");
		}

		[Test]
		public void Properties_ChangeSerialzeAs ()
		{
			SettingsProperty p = new SettingsProperty ("property",
				typeof (int),
				null,
				true,
				10,
				SettingsSerializeAs.String,
				null,
				true,
				false);

			SettingsPropertyValue v = new SettingsPropertyValue (p);

			// test that setting SerializeAs after changing v.PropertyValue causes
			// SerializedValue to be in the new format
			v.PropertyValue = (object)5;
			p.SerializeAs = SettingsSerializeAs.Xml;
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<int>5</int>", ((string)v.SerializedValue).Replace("\r\n", "\n"), "A99");
		}

		[Test]
		public void Dirty ()
		{
			SettingsProperty p = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   10,
								   SettingsSerializeAs.String,
								   null,
								   true,
								   false);

			SettingsPropertyValue v = new SettingsPropertyValue (p);

			Assert.AreEqual (10, v.PropertyValue, "A0");
			Assert.IsFalse (v.IsDirty, "A1");

			/* set PropertyValue to something else */
			v.PropertyValue = 5;
			Assert.IsTrue (v.IsDirty, "A2");
			v.IsDirty = false;

			/* set PropertyValue to the same thing */
			v.PropertyValue = 5;
			Assert.IsTrue (v.IsDirty, "A3");

			/* try out a non-value type */
			p = new SettingsProperty ("property",
						  typeof (StringWriter),
						  null,
						  true,
						  "",
						  SettingsSerializeAs.String,
						  null,
						  true,
						  false);
			v = new SettingsPropertyValue (p);

			Assert.IsNotNull (v.PropertyValue, "A5");

			Console.WriteLine (v.PropertyValue);
			Assert.IsTrue (v.IsDirty, "A6");
		}

		[Test]
		public void UsingDefaultValue ()
		{
			SettingsProperty p = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   10,
								   SettingsSerializeAs.String,
								   null,
								   true,
								   false);

			SettingsPropertyValue v = new SettingsPropertyValue (p);

			Assert.AreEqual (10, v.PropertyValue, "A1");
			Assert.IsTrue (v.UsingDefaultValue, "A2");

			/* set PropertyValue to something else */
			v.PropertyValue = 5;
			Assert.IsFalse (v.UsingDefaultValue, "A3");

			/* set PropertyValue back to the default */
			v.PropertyValue = 10;
			Assert.IsFalse (v.UsingDefaultValue, "A4");
		}

		[Test]
		public void String_Deserialize ()
		{
			SettingsProperty p = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   10,
								   SettingsSerializeAs.String,
								   null,
								   true,
								   false);

			SettingsPropertyValue v = new SettingsPropertyValue (p);
			v.SerializedValue = "123";

			Assert.AreEqual (123, v.PropertyValue, "A1");
			Assert.AreEqual (typeof(int), v.PropertyValue.GetType (), "A2");
			Assert.AreEqual (false, v.UsingDefaultValue, "A3");
		}

		[Test]
		public void Xml_Deserialize ()
		{
			SettingsProperty p = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   10,
								   SettingsSerializeAs.Xml,
								   null,
								   true,
								   false);

			SettingsPropertyValue v = new SettingsPropertyValue (p);
			v.SerializedValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<int>123</int>";

			Assert.AreEqual (123, v.PropertyValue, "A1");
			Assert.AreEqual (typeof(int), v.PropertyValue.GetType (), "A2");
			Assert.AreEqual (false, v.UsingDefaultValue, "A3");
		}

		[Test]
		public void String_Xml_Serialize ()
		{
			SettingsProperty p = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   10,
								   SettingsSerializeAs.String,
								   null,
								   true,
								   false);

			SettingsPropertyValue v = new SettingsPropertyValue (p);

			v.PropertyValue = 10;
			Assert.AreEqual (10, v.PropertyValue, "A1");
			Assert.AreEqual ("10", v.SerializedValue, "A2");

			v.PropertyValue = 10;
			p.SerializeAs = SettingsSerializeAs.Xml;
			
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<int>10</int>", ((string)v.SerializedValue).Replace ("\r\n", "\n"), "A3");

		}

		/// <summary>
		/// This tests the case where we have a SerializedValue but not a PropertyValue.
		/// </summary>
		[Test]
		public void Xml_SerializeNoPropValue ()
		{
			SettingsProperty p = new SettingsProperty ("property",
				typeof (MyData),
				null,
				true,
				10,
				SettingsSerializeAs.Xml,
				null,
				true,
				false);

			SettingsPropertyValue v = new SettingsPropertyValue (p);
			v.SerializedValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<int>10</int>";

			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>\n<int>10</int>", v.SerializedValue);

		}

		[Test]
		public void Binary_Serialize ()
		{
			SettingsProperty p = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   10,
								   SettingsSerializeAs.Binary,
								   null,
								   true,
								   false);

			SettingsPropertyValue v = new SettingsPropertyValue (p);
			byte[] foo;

			v.PropertyValue = 10;

			Assert.AreEqual (typeof (byte[]), v.SerializedValue.GetType(), "A1");
			foo = (byte[])v.SerializedValue;

			v.PropertyValue = 5;
			Assert.AreEqual (5, v.PropertyValue, "A2");


			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream (foo);
			Assert.AreEqual (10, bf.Deserialize (ms), "A3");

			v.Deserialized = false;
			v.SerializedValue = foo;

			Assert.AreEqual (10, v.PropertyValue, "A4");
		}

		[Test]
		public void DefaultValueType ()
		{
			SettingsProperty p1 = new SettingsProperty ("property",
								   typeof (int),
								   null,
								   true,
								   (int) 10,
								   SettingsSerializeAs.String,
								   null,
								   true,
								   false);
			SettingsPropertyValue v1 = new SettingsPropertyValue (p1);
			Assert.AreEqual (typeof (int), v1.PropertyValue.GetType (), "A1");
			Assert.AreEqual (10, v1.PropertyValue, "A2");

			SettingsProperty p2 = new SettingsProperty ("property",
					   typeof (int),
					   null,
					   true,
					   "10",
					   SettingsSerializeAs.String,
					   null,
					   true,
					   false);
			SettingsPropertyValue v2 = new SettingsPropertyValue (p2);
			Assert.AreEqual (typeof (int), v2.PropertyValue.GetType (), "A3");
			Assert.AreEqual (10, v2.PropertyValue, "A4");
		}

		[Serializable]
		public class MyData
		{
			private int intProp = 777;
			public int IntProp
			{
				get { return intProp; }
				set { intProp = value; }
			}
		}

		[Test]
		public void DefaultValueCompexTypeEmpty ()
		{
			SettingsProperty p1 = new SettingsProperty ("property",
								   typeof (MyData),
								   null,
								   true,
								   "",
								   SettingsSerializeAs.String,
								   null,
								   true,
								   false);
			SettingsPropertyValue v1 = new SettingsPropertyValue (p1);
			Assert.IsNotNull (v1.PropertyValue, "A1");
			Assert.AreEqual (typeof (MyData), v1.PropertyValue.GetType (), "A2");
			MyData h = (MyData) v1.PropertyValue;
			Assert.AreEqual (777, h.IntProp, "A3");
		}

		[Test]
		public void DefaultValueCompexType ()
		{
			SettingsProperty p2 = new SettingsProperty ("property",
								   typeof (MyData),
								   null,
								   true,
								   @"<?xml version=""1.0"" encoding=""utf-16""?><MyData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><IntProp>5</IntProp></MyData>",
								   SettingsSerializeAs.Xml,
								   null,
								   true,
								   false);
			SettingsPropertyValue v2 = new SettingsPropertyValue (p2);
			Assert.IsNotNull (v2.PropertyValue, "A1");
			Assert.AreEqual (typeof (MyData), v2.PropertyValue.GetType (), "A2");
			MyData h = (MyData) v2.PropertyValue;
			Assert.AreEqual (5, h.IntProp, "A3");
		}

		[Test]
		public void IsDirtyAndValueDateTime ()
		{
			SettingsProperty sp = new SettingsProperty ("heh");
			sp.PropertyType = typeof (DateTime);

			SettingsPropertyValue spv = new SettingsPropertyValue (sp);
			Assert.IsFalse (spv.IsDirty, "A1");
			Assert.IsNotNull (spv.PropertyValue, "A2");
			Assert.AreEqual (typeof (DateTime), spv.PropertyValue.GetType (), "A3");
			Assert.IsFalse (spv.IsDirty, "A4");
		}

		[Test]
		public void IsDirtyAndValuePrimitive ()
		{
			SettingsProperty sp = new SettingsProperty ("heh");
			sp.PropertyType = typeof (int);

			SettingsPropertyValue spv = new SettingsPropertyValue (sp);
			Assert.IsFalse (spv.IsDirty, "A1");
			Assert.AreEqual (0, spv.PropertyValue, "A2");
			Assert.AreEqual (typeof (int), spv.PropertyValue.GetType (), "A3");
			Assert.IsFalse (spv.IsDirty, "A4");
		}

		[Test]
		public void IsDirtyAndValueDecimal ()
		{
			SettingsProperty sp = new SettingsProperty ("heh");
			sp.PropertyType = typeof (decimal);

			SettingsPropertyValue spv = new SettingsPropertyValue (sp);
			Assert.IsFalse (spv.IsDirty, "A1");
			Assert.AreEqual (0, spv.PropertyValue, "A2");
			Assert.AreEqual (typeof (decimal), spv.PropertyValue.GetType (), "A3");
			Assert.IsTrue (spv.IsDirty, "A4");
		}

		[Test]
		public void IsDirtyAndValueString ()
		{
			SettingsProperty sp = new SettingsProperty ("heh");
			sp.PropertyType = typeof (string);

			SettingsPropertyValue spv = new SettingsPropertyValue (sp);
			Assert.IsFalse (spv.IsDirty, "A1");
			Assert.IsNull (spv.PropertyValue, "A2");
			Assert.IsFalse (spv.IsDirty, "A3");

			SettingsProperty sp2 = new SettingsProperty ("heh");
			sp2.PropertyType = typeof (string);
			sp2.DefaultValue = "";

			SettingsPropertyValue spv2 = new SettingsPropertyValue (sp2);
			Assert.IsFalse (spv2.IsDirty, "A4");
			Assert.IsNotNull (spv2.PropertyValue, "A5");
			Assert.IsFalse (spv2.IsDirty, "A6");
		}

		[Serializable]
		public struct MyData2
		{
			public int intProp;
		}

		[Test]
		public void IsDirtyAndValueMyData2 ()
		{
			SettingsProperty sp = new SettingsProperty ("heh");
			sp.PropertyType = typeof (MyData2);

			SettingsPropertyValue spv = new SettingsPropertyValue (sp);
			Assert.IsFalse (spv.IsDirty, "A1");
			Assert.IsNotNull (spv.PropertyValue, "A2");
			Assert.IsTrue (spv.IsDirty, "A3");
		}

		[Test]
		public void IsDirtyAndValueArrayList ()
		{
			SettingsProperty sp = new SettingsProperty ("heh");
			sp.PropertyType = typeof (ArrayList);

			SettingsPropertyValue spv = new SettingsPropertyValue (sp);
			Assert.IsFalse (spv.IsDirty, "A1");
			Assert.IsNull (spv.PropertyValue, "A2");
			Assert.IsFalse (spv.IsDirty, "A3");

			SettingsProperty sp2 = new SettingsProperty ("heh");
			sp2.PropertyType = typeof (ArrayList);
			sp2.DefaultValue = "";

			SettingsPropertyValue spv2 = new SettingsPropertyValue (sp2);
			Assert.IsFalse (spv2.IsDirty, "A5");
			Assert.IsNotNull (spv2.PropertyValue, "A6");
			Assert.AreEqual (typeof (ArrayList), spv2.PropertyValue.GetType (), "A7");
			Assert.IsTrue (spv2.IsDirty, "A8");
		}
	}
}

