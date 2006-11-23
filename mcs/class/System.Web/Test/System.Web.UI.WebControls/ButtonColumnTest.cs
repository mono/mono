//
// ButtonColumnTest.cs
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

	public class ButtonColumnPoker : ButtonColumn {

		public string FormatData (object data)
		{
			return FormatDataTextValue (data);
		}
	}

	[TestFixture]
	public class ButtonColumnTest {

		[Test]
		public void Defaults ()
		{
			ButtonColumn bc = new ButtonColumn ();
#if NET_2_0
			Assert.AreEqual ("", bc.ValidationGroup, "ValidationGroup");
			Assert.AreEqual (false, bc.CausesValidation, "CausesValidation"); 
#endif
		}

		[Test]
		public void AssignedProperties ()
		{
			ButtonColumn bc = new ButtonColumn ();
#if NET_2_0
			Assert.AreEqual ("", bc.ValidationGroup, "ValidationGroup#1");
			bc.ValidationGroup = "test";
			Assert.AreEqual ("test", bc.ValidationGroup, "ValidationGroup#2");
			Assert.AreEqual (false, bc.CausesValidation, "CausesValidation#1");
			bc.CausesValidation = true;
			Assert.AreEqual (true, bc.CausesValidation, "CausesValidation#2");
#endif
		}

		[Test]
		public void FormatDataValue ()
		{
			ButtonColumnPoker p = new ButtonColumnPoker ();

			p.DataTextFormatString = String.Empty;
			p.Initialize ();
			Assert.AreEqual ("test", p.FormatData ("test"), "A1");
			
			p.DataTextFormatString = "{0} hello";
			p.Initialize ();
			Assert.AreEqual ("test hello", p.FormatData ("test"), "A2");
			
			p.DataTextFormatString = "{0}";
			p.Initialize ();
			Assert.AreEqual ("test", p.FormatData ("test"), "A3");
			
			p.DataTextFormatString = "{0}";
			p.Initialize ();
			Assert.AreEqual (String.Empty, p.FormatData (String.Empty), "A4");

			p.DataTextFormatString = "{0}";
			p.Initialize ();
			p.DataTextFormatString = "i am bad";
			Assert.AreEqual ("i am bad", p.FormatData ("foo"), "A5");

			p.DataTextFormatString = "{0}";
			p.Initialize ();
			Assert.AreEqual (String.Empty, p.FormatData (null), "A6");
		}
	}
}

