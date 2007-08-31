//
// System.Configuration.KeyValueConfigurationElementTest.cs - Unit tests
// for System.Configuration.KeyValueConfigurationElement.
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
using System.Configuration;
using System.ComponentModel;
using NUnit.Framework;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class KeyValueConfigurationElementTest
	{
		class Poker : KeyValueConfigurationElement
		{
			public Poker (string name, string value)
				: base (name, value)
			{
			}

			protected override void Init ()
			{
				Console.WriteLine (Environment.StackTrace);
				base.Init();
			}

			public ConfigurationPropertyCollection GetProperties () {
				return base.Properties;
			}
		}


		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void Properties ()
		{
			Poker p = new Poker ("name", "value");
			ConfigurationPropertyCollection props = p.GetProperties();

			Assert.IsNotNull (props, "A1");
			Assert.AreEqual (2, props.Count, "A2");

			ConfigurationProperty prop;

			prop = props["key"];
			Assert.AreEqual ("key", prop.Name, "A3");
			Assert.IsNull   (prop.Description, "A4");
			Assert.AreEqual (typeof (string), prop.Type, "A5");
			Assert.AreEqual (typeof (StringConverter), prop.Converter.GetType(), "A6");
			Assert.IsNotNull (prop.Validator, "Anull");
			Assert.AreEqual (typeof (DefaultValidator), prop.Validator.GetType(), "A7");
			Assert.AreEqual ("", prop.DefaultValue, "A8");
			Assert.IsTrue   (prop.IsKey, "A9");
			Assert.IsTrue   (prop.IsRequired, "A10");

			Assert.IsFalse  (prop.IsDefaultCollection, "A11");

			prop = props["value"];
			Assert.AreEqual ("value", prop.Name, "A12");
			Assert.IsNull   (prop.Description, "A13");
			Assert.AreEqual (typeof (string), prop.Type, "A14");
			Assert.AreEqual (typeof (StringConverter), prop.Converter.GetType(), "A15");
			Assert.AreEqual (typeof (DefaultValidator), prop.Validator.GetType(), "A16");
			Assert.AreEqual ("", prop.DefaultValue, "A17");
			Assert.IsFalse  (prop.IsKey, "A18");
			Assert.IsFalse  (prop.IsRequired, "A19");

			Assert.IsFalse  (prop.IsDefaultCollection, "A20");
		}
	}

}

#endif
