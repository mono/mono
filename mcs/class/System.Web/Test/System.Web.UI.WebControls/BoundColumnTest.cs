//
// BoundColumnTest.cs
//
// Author:
//	Jackson Harper (jackson@ximian.com)
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
using System.Collections;
using System.Diagnostics;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls {

	public class BoundColumnPoker : BoundColumn {

		public string FormatData (object data)
		{
			return FormatDataValue (data);
		}
	}

	[TestFixture]
	public class BoundColumnTest {

		[Test]
		public void FormatDataValue ()
		{
			BoundColumnPoker p = new BoundColumnPoker ();

			p.DataFormatString = String.Empty;
			p.Initialize ();
			Assert.AreEqual ("test", p.FormatData ("test"), "A1");
			
			p.DataFormatString = "{0} hello";
			p.Initialize ();
			Assert.AreEqual ("test hello", p.FormatData ("test"), "A2");
			
			p.DataFormatString = "{0}";
			p.Initialize ();
			Assert.AreEqual ("test", p.FormatData ("test"), "A3");
			
			p.DataFormatString = "{0}";
			p.Initialize ();
			Assert.AreEqual (String.Empty, p.FormatData (String.Empty), "A4");

			p.DataFormatString = "{0}";
			p.Initialize ();
			p.DataFormatString = "i am bad";
			Assert.AreEqual ("foo", p.FormatData ("foo"), "A5");

			p.DataFormatString = "{0}";
			p.Initialize ();
			Assert.AreEqual (String.Empty, p.FormatData (null), "A6");
		}
	}
}

