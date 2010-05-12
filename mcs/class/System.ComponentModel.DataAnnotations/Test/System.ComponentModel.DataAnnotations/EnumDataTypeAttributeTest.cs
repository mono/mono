//
// EnumDataTypeAttributeTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
#if NET_4_0
	[TestFixture]
	public class EnumDataTypeAttributeTest
	{
		[Test]
		public void Constructor ()
		{
			var attr = new EnumDataTypeAttribute (typeof (string));

			Assert.AreEqual (DataType.Custom, attr.DataType, "#A1-1");
			Assert.AreEqual (typeof (string), attr.EnumType, "#A1-2");

			attr = new EnumDataTypeAttribute (typeof (TestEnum));
			Assert.AreEqual (DataType.Custom, attr.DataType, "#B1-1");
			Assert.AreEqual (typeof (TestEnum), attr.EnumType, "#B1-2");

			attr = new EnumDataTypeAttribute (null);
			Assert.AreEqual (DataType.Custom, attr.DataType, "#C1-1");
			Assert.AreEqual (null, attr.EnumType, "#C1-2");
		}

		[Test]
		public void IsValid ()
		{
			var attr = new EnumDataTypeAttribute (typeof (string));
			
			try {
				attr.IsValid (null);
				Assert.Fail ("#A1-1");
			} catch (InvalidOperationException) {
				// success
			}

			try {
				attr.IsValid ("stuff");
				Assert.Fail ("#A1-2");
			} catch (InvalidOperationException) {
				// success
			}

			attr = new EnumDataTypeAttribute (typeof (TestEnum));
			Assert.IsTrue (attr.IsValid (null), "#A2-1");
			Assert.IsTrue (attr.IsValid (1), "#A2-2");
			Assert.IsFalse (attr.IsValid (0), "#A2-3");
			Assert.IsTrue (attr.IsValid (TestEnum.Two), "#A2-4");
			Assert.IsFalse (attr.IsValid (AnotherEnum.Five), "#A2-5");
			Assert.IsFalse (attr.IsValid ("stuff"), "#A2-5");

			AnotherEnum val = AnotherEnum.Six;
			Assert.IsFalse (attr.IsValid (val), "#A2-6");

			Assert.IsTrue (attr.IsValid (String.Empty), "#A2-7");
			Assert.IsTrue (attr.IsValid ("Three"), "#A2-8");
			Assert.IsFalse (attr.IsValid ("Four"), "#A2-9");
			Assert.IsFalse (attr.IsValid (true), "#A2-10");
			Assert.IsFalse (attr.IsValid (' '), "#A2-11");
			Assert.IsFalse (attr.IsValid (0.12F), "#A2-12");
			Assert.IsTrue (attr.IsValid ((short) 1), "#A2-13");
			Assert.IsFalse (attr.IsValid (12.3M), "#A2-14");
			Assert.IsFalse (attr.IsValid (12.3D), "#A2-15");
			Assert.IsTrue (attr.IsValid ((long) 1), "#A2-16");

			attr = new EnumDataTypeAttribute (typeof (AnotherEnum));
			Assert.IsTrue (attr.IsValid (null), "#A3-1");
			Assert.IsTrue (attr.IsValid (4), "#A3-2");
			Assert.IsFalse (attr.IsValid (0), "#A3-3");
			Assert.IsTrue (attr.IsValid (AnotherEnum.Five), "#A3-4");
			Assert.IsFalse (attr.IsValid (TestEnum.One), "#A3-5");
			Assert.IsFalse (attr.IsValid ("stuff"), "#A3-5");

			val = AnotherEnum.Four;
			Assert.IsTrue (attr.IsValid (val), "#A3-6");

			Assert.IsTrue (attr.IsValid (String.Empty), "#A3-7");
			Assert.IsTrue (attr.IsValid ("Four"), "#A3-8");
			Assert.IsFalse (attr.IsValid ("Three"), "#A3-9");
			Assert.IsTrue (attr.IsValid (12), "#A3-10");
			Assert.IsTrue (attr.IsValid ("Five, Six"), "#A3-11");
			Assert.IsFalse (attr.IsValid (true), "#A3-12");
			Assert.IsFalse (attr.IsValid (' '), "#A3-13");
			Assert.IsFalse (attr.IsValid (0.12), "#A3-14");
		}
	}

	enum TestEnum
	{
		One = 1,
		Two,
		Three
	}

	[Flags]
	enum AnotherEnum
	{
		Four = 4,
		Five = 8,
		Six = 16
	}
#endif
}
