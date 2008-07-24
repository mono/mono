//
// StringValueConverterTest.cs
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2008 Gert Driesen
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
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Design;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System.Diagnostics.Design
{
	[TestFixture]
	public class StringValueConverterTest
	{
		TypeConverter converter;

		[SetUp]
		public void SetUp ()
		{
			Assembly a = typeof (LogConverter).Assembly;
			Type type = a.GetType ("System.Diagnostics.Design.StringValueConverter");
			Assert.IsNotNull (type);
			converter = (TypeConverter) Activator.CreateInstance (type);
		}

		[Test]
		public void CanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (int)), "#2");
			Assert.IsTrue (converter.CanConvertFrom (typeof (InstanceDescriptor)), "#3");
		}

		[Test]
		public void CanConvertTo ()
		{
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertTo (typeof (int)), "#2");
			Assert.IsFalse (converter.CanConvertTo (typeof (InstanceDescriptor)), "#3");
		}

		[Test]
		public void ConvertFrom ()
		{
			Assert.AreEqual ("AbC", converter.ConvertFrom ("AbC"), "#1");
			Assert.IsNull (converter.ConvertFrom (string.Empty), "#2");
			Assert.IsNull (converter.ConvertFrom (" \r "), "#3");
			Assert.AreEqual ("AbC", converter.ConvertFrom (" \nAbC\r "), "#4");
		}

		[Test]
		public void ConvertFrom_Value_Null ()
		{
			try {
				converter.ConvertFrom (null);
				Assert.Fail ("#1");
			} catch (NotSupportedException ex) {
				// StringValueConverter cannot convert from (null)
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("StringValueConverter") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("(null)") != -1, "#6");
			}
		}

		[Test]
		public void ConvertTo ()
		{
			Assert.AreEqual ("AbC", converter.ConvertTo ("AbC", typeof (string)), "#1");
			Assert.AreEqual (string.Empty, converter.ConvertTo (string.Empty, typeof (string)), "#2");
			Assert.AreEqual (" \r ", converter.ConvertTo (" \r ", typeof (string)), "#3");
			Assert.AreEqual (" \nAbC\r ", converter.ConvertTo (" \nAbC\r ", typeof (string)), "#4");
		}
	}
}
