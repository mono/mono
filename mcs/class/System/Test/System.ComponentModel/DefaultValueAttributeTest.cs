//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
#if !MOBILE && !XAMMAC_4_5
using System.Drawing;
#endif
using NUnit.Framework;

namespace MonoTests.System.ComponentModel {

	[TestFixture]
	public class DefaultValueAttributeTest {

		[Test]
		public void Null ()
		{
			DefaultValueAttribute dva = new DefaultValueAttribute (null);
			Assert.IsNull (dva.Value, "Value");

			Assert.IsFalse (dva.Equals (null), "Equals(null)");

			DefaultValueAttribute dva2 = new DefaultValueAttribute (null);
			Assert.IsTrue (dva.Equals (dva2), "Equals(new)");

			Assert.AreEqual (dva.GetHashCode (), dva2.GetHashCode (), "GetHashCode");
		}

		[Test]
		public void Bool ()
		{
			DefaultValueAttribute dvat = new DefaultValueAttribute (true);
			Assert.IsTrue ((bool) dvat.Value, "Value");

			Assert.IsFalse (dvat.Equals (true), "Equals(true)");
			Assert.IsTrue (dvat.Equals (new DefaultValueAttribute (true)), "Equals(new)");

			Assert.AreEqual (1, dvat.GetHashCode (), "GetHashCode");
		}

#if !MOBILE && !XAMMAC_4_5
		[DefaultValue (typeof (Color), "Black")]
		public Color Bar { get; set; }

		// https://github.com/mono/mono/issues/12362
		[Test]
		public void Bug_12362 ()
		{
			var prop = typeof (DefaultValueAttributeTest).GetProperty ("Bar");
			var attr = (DefaultValueAttribute)prop.GetCustomAttributes (true) [0];
			var value = attr.Value;
			Assert.IsNotNull (value);
			Assert.AreEqual (typeof (Color), value.GetType ());
		}
#endif
	}
}
