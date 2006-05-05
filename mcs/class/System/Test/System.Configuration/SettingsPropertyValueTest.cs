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

#if NET_2_0

using System;
using System.IO;
using System.Configuration;
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


#if notyet
			/* the msdn docs say that
			 * SettingsPropertyValue pessimistically sets
			 * IsDirty to true when you use
			 * v.PropertyValue on non-primitive types */

			/* try out a non-value type */
			p = new SettingsProperty ("property",
						  typeof (StringWriter),
						  null,
						  true,
						  null,
						  SettingsSerializeAs.String,
						  null,
						  true,
						  false);
			v = new SettingsPropertyValue (p);

			Assert.IsNull (v.PropertyValue, "A5");

			Console.WriteLine (v.PropertyValue);
			Assert.IsTrue (v.IsDirty, "A6");
#endif
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
			
#if notyet
			v.Deserialized = false;
			v.SerializedValue = foo;

			Assert.AreEqual (10, v.PropertyValue, "A4");
#endif
		}
	}

}

#endif
