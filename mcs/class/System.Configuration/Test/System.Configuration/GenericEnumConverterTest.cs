//
// System.Configuration.GenericEnumConverterTest.cs - Unit tests
// for System.Configuration.GenericEnumConverter.
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
using NUnit.Framework;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class GenericEnumConverterTest
	{
		enum FooEnum {
			Foo = 1,
			Bar = 2
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_Null ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (null);
		}

		[Test]
		public void Ctor_TypeError ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (object));
		}

		[Test]
		public void CanConvertFrom ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));

			Assert.IsTrue (cv.CanConvertFrom (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertFrom (null, typeof (object)), "A4");
		}

		[Test]
		public void CanConvertTo ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));

			Assert.IsTrue (cv.CanConvertTo (null, typeof (string)), "A1");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (TimeSpan)), "A2");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (int)), "A3");
			Assert.IsFalse (cv.CanConvertTo (null, typeof (object)), "A4");
		}

		[Test]
		public void ConvertFrom ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));
			Assert.AreEqual (FooEnum.Foo, cv.ConvertFrom (null, null, "Foo"), "A1");
			Assert.AreEqual (FooEnum.Bar, cv.ConvertFrom (null, null, "Bar"), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFrom_Case ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));
			Assert.AreEqual (FooEnum.Foo, cv.ConvertFrom (null, null, "foo"), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFrom_InvalidString ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));
			object o;

			o = cv.ConvertFrom (null, null, "baz");
			Assert.IsNull (o, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConvertFrom_Null ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));
			object o;

			o = cv.ConvertFrom (null, null, null);
			Assert.IsNull (o, "A1");
		}

		[Test]
		public void ConvertTo ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));

			Assert.AreEqual ("Foo", cv.ConvertTo (null, null, FooEnum.Foo, typeof (string)), "A1");
			Assert.AreEqual ("Bar", cv.ConvertTo (null, null, FooEnum.Bar, typeof (string)), "A2");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConvertTo_NullError ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));

			Assert.AreEqual ("", cv.ConvertTo (null, null, null, typeof (string)), "A1");
		}

		[Test]
		public void ConvertTo_TypeError1 ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));

			Assert.AreEqual ("0", cv.ConvertTo (null, null, 0, typeof (string)), "A1");
		}

		[Test]
		public void ConvertTo_TypeError2 ()
		{
			GenericEnumConverter cv = new GenericEnumConverter (typeof (FooEnum));

			Assert.AreEqual ("", cv.ConvertTo (null, null, "", typeof (string)), "A1");
		}
	}
}

#endif
