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
// Copyright (c) 2007 Novell, Inc.
//

#if NET_2_0

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class KeysConverterTest : TestHelper
	{
		[Test]
		public void CanConvertTo ()
		{
			KeysConverter c = new KeysConverter ();

			Assert.AreEqual (true, c.CanConvertTo (null, typeof (string)), "A1");
			Assert.AreEqual (false, c.CanConvertTo (null, typeof (int)), "A2");
			Assert.AreEqual (false, c.CanConvertTo (null, typeof (float)), "A3");
			Assert.AreEqual (false, c.CanConvertTo (null, typeof (object)), "A4");
			Assert.AreEqual (false, c.CanConvertTo (null, typeof (Enum)), "A5");
			Assert.AreEqual (true, c.CanConvertTo (null, typeof (Enum [])), "A6");
		}
	}
}
#endif
