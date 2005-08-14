//
// Tests for System.Drawing.ColorConverter.cs 
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.ComponentModel.Design.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Drawing
{
	[TestFixture]
	public class ColorConverterTest
	{
		private ColorConverter converter = new ColorConverter();

		[Test]
		public void TestCanConvertFrom ()
		{
			Assert.IsTrue (converter.CanConvertFrom (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertFrom (typeof (Object)), "#2");
			Assert.IsFalse (converter.CanConvertFrom (typeof (int)), "#3");
		}

		[Test]
		public void TestCanConvertTo ()
		{
			Assert.IsTrue (converter.CanConvertTo (typeof (string)), "#1");
			Assert.IsFalse (converter.CanConvertTo (typeof (Object)), "#2");
			Assert.IsFalse (converter.CanConvertTo (typeof (int)), "#3");
			Assert.IsTrue (converter.CanConvertTo (typeof (InstanceDescriptor)), "#4");
		}

		[Test]
		public void TestConvertFrom ()
		{
			Assert.AreEqual (Color.Empty, converter.ConvertFrom (string.Empty), "#1");
			Assert.AreEqual (Color.Empty, converter.ConvertFrom (" "), "#2");
			Assert.AreEqual (Color.Red, converter.ConvertFrom ("Red"), "#3");
			Assert.AreEqual (Color.Red, converter.ConvertFrom (" Red "), "#4");
		}

		[Test]
		public void TestConvertTo ()
		{
			Assert.AreEqual (string.Empty, converter.ConvertTo (Color.Empty, typeof (string)), "#1");
			Assert.AreEqual ("Red", converter.ConvertTo (Color.Red, typeof(string)), "#2");
			Assert.AreEqual (string.Empty, converter.ConvertTo (null, typeof (string)), "#3");
			Assert.AreEqual ("test", converter.ConvertTo ("test", typeof (string)), "#4");
		}
	}
}
