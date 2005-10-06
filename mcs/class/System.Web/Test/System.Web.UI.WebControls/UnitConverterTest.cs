//
// Tests for System.Web.UI.WebControls.Unit.cs 
//
// Author:
//	Ben Maurer <bmaurer@novell.com>
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

using NUnit.Framework;
using System;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls {
	class UnitConverterTest {
		UnitConverter c = new UnitConverter ();
		
		[Test]
		public void TestConvertTo ()
		{
			Assert.Equals (c.ConvertTo (new Unit (1), typeof (string)), new Unit (1).ToString ());
			Assert.Equals (c.ConvertTo ("1 px", typeof (Unit)), new Unit (1));
			Assert.IsTrue (c.CanConvertTo (typeof (string)));
			Assert.IsTrue (c.CanConvertTo (typeof (Unit)));	
		}

		[Test]
		public void TestConvertFrom ()
		{
			Assert.Equals (c.ConvertFrom (new Unit (1)), new Unit (1).ToString ());
			Assert.Equals (c.ConvertFrom ("1 px"), new Unit (1));
			Assert.IsTrue (c.CanConvertFrom (typeof (string)));
			Assert.IsTrue (c.CanConvertFrom (typeof (Unit)));	
		}
	}
}
