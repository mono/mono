//
// MonoTests.System.ComponentModel.NullableConverterTest
//
// Author:
//      Ivan N. Zlatev  <contact@i-nz.net>
//
// Copyright (C) 2008 Ivan N. Zlatev
//

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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class NullableConverterTest
	{
		[TypeConverter (typeof(MyTypeConverter))]
		private struct MyType
		{
			int value;
		}

		private class MyTypeConverter : TypeConverter
		{
			public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
			{
				return true;
			}

			public override TypeConverter.StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
			{
				return new TypeConverter.StandardValuesCollection (new object[] {});
			}
		}

		[Test]
		public void PropertyValues ()
		{
			NullableConverter converter = new NullableConverter (typeof(MyType?));
			Assert.AreEqual (typeof(MyType?), converter.NullableType, "#1");
			Assert.AreEqual (typeof(MyType), converter.UnderlyingType, "#2");
			Assert.AreEqual (typeof(MyTypeConverter), converter.UnderlyingTypeConverter.GetType(), "#2");
		}

		[Test]
		public void CanConvertFrom ()
		{
			NullableConverter converter = new NullableConverter (typeof(MyType?));
			Assert.IsTrue (converter.CanConvertFrom (null, typeof(MyType)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (null, typeof(object)), "#2");
		}

		[Test]
		public void CanConvertTo ()
		{
			NullableConverter converter = new NullableConverter (typeof(MyType?));
			Assert.IsTrue (converter.CanConvertTo (null, typeof(MyType)), "#1");
			Assert.IsFalse (converter.CanConvertTo (null, typeof(object)), "#2");
		}

		[Test]
		public void ConvertFrom_EmptyString ()
		{
			NullableConverter converter = new NullableConverter (typeof(MyType?));
			Assert.IsNull (converter.ConvertFrom (null, null, String.Empty), "#1");
		}

		[Test]
		public void GetStandardValues ()
		{
			NullableConverter converter = new NullableConverter (typeof(MyType?));
			Assert.IsTrue (converter.GetStandardValuesSupported (null), "#1");
			TypeConverter.StandardValuesCollection values = converter.GetStandardValues (null);
			// MyTypeConverter returns an empty values collection, but
			// NullableConverter adds a null value to the set.
			Assert.AreEqual (1, values.Count, "#2");
			Assert.AreEqual (null, values[0], "#3");
		}
	}
}
#endif
